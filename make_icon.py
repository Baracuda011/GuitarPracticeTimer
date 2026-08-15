"""Generates Assets/app.ico (pure stdlib: no Pillow needed).

Draws a supersampled, anti-aliased timer mark: dark rounded square,
mint progress ring with a gap at the top, and a clock hand.
"""
import math
import os
import struct
import zlib

BG = (0x15, 0x18, 0x21)
RING = (0x7F, 0xD1, 0xAE)
HAND = (0xEC, 0xEE, 0xF3)

SS = 4  # supersample factor
SIZES = [16, 24, 32, 48, 64, 128, 256]


def rounded_rect_inside(x, y, w, h, r):
    """True if point (x, y) is inside a w*h rounded rect with corner radius r."""
    cx = min(max(x, r), w - r)
    cy = min(max(y, r), h - r)
    return (x - cx) ** 2 + (y - cy) ** 2 <= r * r


def render(size):
    """Returns a list of RGBA rows (each a bytearray) for one icon size."""
    n = size * SS
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

    # Box-downsample SSxSS blocks to the target size.
    rows = []
    for y in range(size):
        out = bytearray(size * 4)
        for x in range(size):
            ar = ag = ab = aa = 0
            for sy in range(SS):
                base = ((y * SS + sy) * n + x * SS) * 4
                for sx in range(SS):
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
                out[o + 3] = aa // (SS * SS)
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


def main():
    images = [png(render(s), s) for s in SIZES]

    header = struct.pack("<HHH", 0, 1, len(images))
    offset = len(header) + 16 * len(images)
    entries, blobs = b"", b""
    for size, blob in zip(SIZES, images):
        dim = 0 if size >= 256 else size
        entries += struct.pack("<BBBBHHII", dim, dim, 0, 0, 1, 32, len(blob), offset)
        offset += len(blob)
        blobs += blob

    out_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Assets")
    os.makedirs(out_dir, exist_ok=True)
    path = os.path.join(out_dir, "app.ico")
    with open(path, "wb") as fh:
        fh.write(header + entries + blobs)
    print(f"wrote {path} ({os.path.getsize(path)} bytes)")


if __name__ == "__main__":
    main()
