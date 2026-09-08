"""Checks the committed assets still match their generators (pure stdlib).

Why this is not just `git diff`:

Assets/app.ico embeds PNG frames, and PNG pixel data is zlib-compressed. zlib
does not promise identical output across versions, so the same pixels compress
to a different number of bytes on different Pythons - 8142 locally, 8005 on a
3.14 runner. A byte comparison therefore fails for a reason that has nothing to
do with whether the icon is correct.

So the icon is compared by decoded pixels, which is what actually matters, and
the chime by exact bytes, which it can be because WAV is uncompressed.
"""
import struct
import sys
import zlib

import make_chime
import make_icon


def png_pixels(blob):
    """Decompressed pixel bytes and dimensions of a PNG, ignoring how it was packed."""
    if blob[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError("not a PNG")

    pos = 8
    size = None
    idat = b""

    while pos < len(blob):
        (length,) = struct.unpack(">I", blob[pos:pos + 4])
        tag = blob[pos + 4:pos + 8]
        data = blob[pos + 8:pos + 8 + length]

        if tag == b"IHDR":
            width, height = struct.unpack(">II", data[:8])
            size = (width, height)
        elif tag == b"IDAT":
            idat += data
        elif tag == b"IEND":
            break

        pos += 12 + length  # length + tag + data + crc

    return size, zlib.decompress(idat)


def ico_frames(blob):
    """Every embedded image in an .ico, as (size, pixels)."""
    _, _, count = struct.unpack("<HHH", blob[:6])
    frames = []

    for i in range(count):
        entry = 6 + i * 16
        length, offset = struct.unpack("<II", blob[entry + 8:entry + 16])
        frames.append(png_pixels(blob[offset:offset + length]))

    return frames


def icns_members(blob):
    """Every member of an .icns, as (type, (size, pixels))."""
    if blob[:4] != b"icns":
        raise ValueError("not an icns")

    (declared,) = struct.unpack(">I", blob[4:8])
    if declared != len(blob):
        raise ValueError(f"icns header claims {declared} bytes, file is {len(blob)}")

    members = []
    pos = 8

    while pos < len(blob):
        tag = blob[pos:pos + 4]
        (length,) = struct.unpack(">I", blob[pos + 4:pos + 8])
        members.append((tag, png_pixels(blob[pos + 8:pos + length])))
        pos += length

    return members


def expected_pixels(size):
    """What make_icon.render produces for one size, as raw decoded PNG bytes."""
    return zlib.decompress(zlib.compress(
        b"".join(b"\x00" + bytes(row) for row in make_icon.render(size))))


def check_icon():
    with open("Assets/app.ico", "rb") as fh:
        committed = ico_frames(fh.read())

    expected = [((size, size), expected_pixels(size)) for size in make_icon.SIZES]

    if len(committed) != len(expected):
        return f"icon has {len(committed)} frames, generator produces {len(expected)}"

    for (got_size, got_px), (want_size, want_px) in zip(committed, expected):
        if got_size != want_size:
            return f"icon frame is {got_size}, generator produces {want_size}"
        if got_px != want_px:
            return f"icon frame {got_size[0]}px does not match make_icon.py"

    return None


def check_icns():
    with open("Assets/app.icns", "rb") as fh:
        committed = icns_members(fh.read())

    if len(committed) != len(make_icon.ICNS_MEMBERS):
        return (f"icns has {len(committed)} members, "
                f"generator produces {len(make_icon.ICNS_MEMBERS)}")

    # Render each distinct size once - several members share the same pixels.
    cache = {}

    for (got_tag, (got_size, got_px)), (want_tag, want_edge) in zip(
            committed, make_icon.ICNS_MEMBERS):
        if got_tag != want_tag:
            return (f"icns member is {got_tag.decode()}, "
                    f"generator produces {want_tag.decode()}")
        if got_size != (want_edge, want_edge):
            return (f"icns member {got_tag.decode()} is {got_size}, "
                    f"generator produces {(want_edge, want_edge)}")

        if want_edge not in cache:
            cache[want_edge] = expected_pixels(want_edge)
        if got_px != cache[want_edge]:
            return f"icns member {got_tag.decode()} does not match make_icon.py"

    return None


def check_chime():
    with open("Assets/chime.wav", "rb") as fh:
        committed = fh.read()

    samples = (make_chime.note(*make_chime.NOTE_ONE)
               + make_chime.silence(make_chime.GAP)
               + make_chime.note(*make_chime.NOTE_TWO))
    frames = b"".join(
        struct.pack("<h", max(-32768, min(32767, int(s * make_chime.PEAK * 32767))))
        for s in samples
    )

    if committed[-len(frames):] != frames:
        return "chime.wav does not match make_chime.py"
    return None


def safely(check):
    """A malformed asset raises rather than returning; report it, don't traceback."""
    try:
        return check()
    except Exception as exc:  # noqa: BLE001 - any failure here means "does not match"
        return f"{check.__name__} could not read the asset: {exc}"


def main():
    checks = (safely(check_icon), safely(check_icns), safely(check_chime))
    problems = [p for p in checks if p]

    if problems:
        for p in problems:
            print(f"FAIL: {p}", file=sys.stderr)
        print("\nRun 'python make_icon.py' and 'python make_chime.py', "
              "then commit the result.", file=sys.stderr)
        return 1

    print("Assets/app.ico, Assets/app.icns and Assets/chime.wav "
          "all match their generators.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
