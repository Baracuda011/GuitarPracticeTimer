"""Generates Assets/app.ico and Assets/app.icns (pure stdlib: no Pillow needed).

Draws a supersampled, anti-aliased timer mark: dark rounded square,
mint progress ring with a gap at the top, and a clock hand.

The .ico is for Windows; the .icns is the same mark for the macOS .app bundle,
so both platforms show one icon drawn from one source rather than two that have
to be kept in step by hand.
"""
import math
import os
import struct
import zlib

BG = (0x15, 0x18, 0x21)
RING = (0x7F, 0xD1, 0xAE)
HAND = (0xEC, 0xEE, 0xF3)

SIZES = [16, 24, 32, 48, 64, 128, 256]

# macOS icon members, smallest first. The @2x entries carry the same pixels as
# their plain counterpart of the same edge length - ic11 is "16pt at 2x", which
# is a 32px image, exactly what icp5 holds. Rendering is done once per distinct
# size and the blob is shared, which is also what iconutil does.
ICNS_MEMBERS = [
    (b"icp4", 16),     # 16pt
    (b"icp5", 32),     # 32pt
    (b"ic11", 32),     # 16pt @2x
    (b"ic12", 64),     # 32pt @2x
    (b"ic07", 128),    # 128pt
    (b"ic08", 256),    # 256pt
    (b"ic13", 256),    # 128pt @2x
    (b"ic09", 512),    # 512pt
    (b"ic14", 512),    # 256pt @2x
    (b"ic10", 1024),   # 512pt @2x
]

ICNS_SIZES = sorted({size for _, size in ICNS_MEMBERS})


def supersample(size):
    """Anti-aliasing factor for one size.

    4x everywhere it matters. The large macOS members drop to 2x because the
    cost is quadratic - a 1024px icon at 4x means rendering 16.7M pixels in
    pure Python - and at that resolution the edges are already smooth enough
    that the difference is not visible.
    """
    return 4 if size <= 256 else 2


def rounded_rect_inside(x, y, w, h, r):
    """True if point (x, y) is inside a w*h rounded rect with corner radius r."""
    cx = min(max(x, r), w - r)
    cy = min(max(y, r), h - r)
    return (x - cx) ** 2 + (y - cy) ** 2 <= r * r


def render(size):
    """Returns a list of RGBA rows (each a bytearray) for one icon size."""
    ss = supersample(size)
    n = size * ss
    c = n / 2.0
    radius_corner = n * 0.22
    ring_r = n * 0.315
    ring_w = n * 0.085
    gap_half = math.radians(32)  # half-width of the gap at 12 o'clock
    hand_len = n * 0.235
    hand_w = n * 0.052

    hi = bytearray(n * n * 4)
    for py in range(n):
        fy = py + 0.5
        row = py * n * 4
        for px in range(n):
            fx = px + 0.5
            if not rounded_rect_inside(fx, fy, n, n, radius_corner):
                continue

            r, g, b = BG

            dx = fx - c
            dy = fy - c
            dist = math.hypot(dx, dy)

            # Ring, minus the gap centred on 12 o'clock.
            if abs(dist - ring_r) <= ring_w / 2.0:
                ang = math.atan2(dx, -dy)  # 0 at 12 o'clock, grows clockwise
                if abs(ang) > gap_half:
                    r, g, b = RING

            # Clock hand: vertical bar from the centre pointing up.
            if abs(dx) <= hand_w / 2.0 and -hand_len <= dy <= hand_w * 0.5:
                r, g, b = HAND

            o = row + px * 4
            hi[o] = r
            hi[o + 1] = g
            hi[o + 2] = b
            hi[o + 3] = 255

    # Box-downsample ss*ss blocks to the target size.
    rows = []
    for y in range(size):
        out = bytearray(size * 4)
        for x in range(size):
            ar = ag = ab = aa = 0
            for sy in range(ss):
                base = ((y * ss + sy) * n + x * ss) * 4
                for sx in range(ss):
                    o = base + sx * 4
                    a = hi[o + 3]
                    ar += hi[o] * a
                    ag += hi[o + 1] * a
                    ab += hi[o + 2] * a
                    aa += a
            o = x * 4
            if aa:
                out[o] = ar // aa
                out[o + 1] = ag // aa
                out[o + 2] = ab // aa
                out[o + 3] = aa // (ss * ss)
        rows.append(out)
    return rows


def png(rows, size):
    def chunk(tag, data):
        body = tag + data
        return struct.pack(">I", len(data)) + body + struct.pack(">I", zlib.crc32(body))

    raw = b"".join(b"\x00" + bytes(r) for r in rows)
    return (
        b"\x89PNG\r\n\x1a\n"
        + chunk(b"IHDR", struct.pack(">IIBBBBB", size, size, 8, 6, 0, 0, 0))
        + chunk(b"IDAT", zlib.compress(raw, 9))
        + chunk(b"IEND", b"")
    )


def ico(images):
    """Windows .ico containing every frame in SIZES."""
    header = struct.pack("<HHH", 0, 1, len(images))
    offset = len(header) + 16 * len(images)
    entries, blobs = b"", b""

    for size, blob in zip(SIZES, images):
        # 0 means 256 in an .ico directory entry; the field is a single byte.
        dim = 0 if size >= 256 else size
        entries += struct.pack("<BBBBHHII", dim, dim, 0, 0, 1, 32, len(blob), offset)
        offset += len(blob)
        blobs += blob

    return header + entries + blobs


def icns(by_size):
    """macOS .icns built from {size: png_bytes}.

    The container is deliberately simple: a magic word, the total length, then
    one record per member - a four-character type, the record length including
    its own 8-byte header, and the PNG itself.
    """
    body = b""
    for tag, size in ICNS_MEMBERS:
        blob = by_size[size]
        body += tag + struct.pack(">I", len(blob) + 8) + blob

    return b"icns" + struct.pack(">I", len(body) + 8) + body


def main():
    out_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Assets")
    os.makedirs(out_dir, exist_ok=True)

    # Render each distinct size once; both containers draw from the same frames.
    by_size = {s: png(render(s), s) for s in sorted(set(SIZES) | set(ICNS_SIZES))}

    for name, blob in (
        ("app.ico", ico([by_size[s] for s in SIZES])),
        ("app.icns", icns(by_size)),
    ):
        path = os.path.join(out_dir, name)
        with open(path, "wb") as fh:
            fh.write(blob)
        print(f"wrote {path} ({os.path.getsize(path)} bytes)")


if __name__ == "__main__":
    main()
