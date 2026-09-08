"""Wraps a published macOS build into GuitarPracticeTimer.app and tars it.

    python make_macos_app.py publish/osx-arm64 --arch apple-silicon
    python make_macos_app.py publish/osx-arm64 publish/osx-x64 --arch universal

`dotnet publish` produces a bare Unix executable. Double-clicking that in
Finder opens a Terminal window, it has no icon, no name, and no Dock identity.
macOS only treats a program as an application when it is inside a bundle - a
directory with a particular shape and an Info.plist describing it.

This builds that bundle. It runs anywhere, not only on macOS: everything here
is directory layout, an XML file and a tar archive, none of which needs Apple
tooling. That matters because the release is built on a CI runner and nobody on
this project owns a Mac.

Given more than one published directory it merges them into one universal
binary, so a single download runs on both Apple silicon and Intel and nobody
has to know which Mac they own. Only two files actually need merging - the
app host and libminiaudio - because SkiaSharp, HarfBuzz and AvaloniaNative
already ship as universal dylibs.

Three details are load-bearing:

  * The executable bit. Windows tar does not record it and `tar --mode` is not
    supported there, so an archive rolled on Windows arrives on macOS as a
    non-executable file and the app cannot start. tarfile lets the mode be set
    explicitly, which is why this is Python rather than a shell one-liner.

  * The universal binary is assembled here rather than with Apple's `lipo`,
    for the same reason: `lipo` only exists on a Mac. CI checks the result with
    the real `lipo` afterwards, so Apple's own tool certifies what this writes.

  * The bundle is *not* signed here. Without a Developer ID certificate macOS
    shows "Apple could not verify this app is free of malware" on first launch;
    right-click -> Open gets past it. Signing and notarisation need a paid
    Apple Developer account and are a separate step.
"""
import argparse
import os
import plistlib
import shutil
import struct
import sys
import tarfile

# Mach-O magic numbers. The thin ones are little-endian on every Mac that
# matters; the fat header is big-endian by definition of the format.
MH_MAGIC_64 = 0xFEEDFACF
MH_MAGIC = 0xFEEDFACE
FAT_MAGIC = 0xCAFEBABE
FAT_MAGIC_64 = 0xCAFEBABF

# 2^14. arm64 slices must start on a 16 KB boundary; x86_64 needs only 4 KB,
# and over-aligning it is harmless, so one value covers both.
FAT_ALIGN = 14

APP_NAME = "GuitarPracticeTimer"
DISPLAY_NAME = "Guitar Practice Timer"
BUNDLE_ID = "com.guitarpracticetimer.app"

# Kept in step with the Desktop csproj's <Version> and the release tag.
VERSION = "1.1.0"

# .NET 10 does not support macOS earlier than this, so claiming any lower would
# promise something the runtime cannot deliver.
MIN_MACOS = "12.0"


def info_plist():
    return {
        "CFBundleName": APP_NAME,
        "CFBundleDisplayName": DISPLAY_NAME,
        "CFBundleIdentifier": BUNDLE_ID,
        "CFBundleExecutable": APP_NAME,
        "CFBundleIconFile": "app.icns",
        "CFBundlePackageType": "APPL",
        "CFBundleInfoDictionaryVersion": "6.0",
        "CFBundleVersion": VERSION,
        "CFBundleShortVersionString": VERSION,
        "LSMinimumSystemVersion": MIN_MACOS,
        "LSApplicationCategoryType": "public.app-category.music",
        # Without this the window is drawn at 1x and scaled up, which on a
        # Retina display looks soft - especially the dial's arc.
        "NSHighResolutionCapable": True,
        # The timer draws its own window chrome and has no menu bar of its own,
        # so it must not be launched as a background-only agent.
        "LSUIElement": False,
        "NSPrincipalClass": "NSApplication",
        "NSSupportsAutomaticGraphicsSwitching": True,
        "CFBundleDevelopmentRegion": "en",
    }


def thin_arch(blob):
    """(cputype, cpusubtype) if this is a single-architecture Mach-O, else None."""
    if len(blob) < 12:
        return None
    magic, cputype, cpusubtype = struct.unpack("<III", blob[:12])
    if magic in (MH_MAGIC_64, MH_MAGIC):
        return cputype, cpusubtype
    return None


def is_fat(blob):
    return len(blob) >= 4 and struct.unpack(">I", blob[:4])[0] in (FAT_MAGIC, FAT_MAGIC_64)


def fat_binary(blobs):
    """One universal binary from several thin Mach-O files.

    The container is a big-endian header - a magic word and a count - then one
    20-byte record per architecture giving its type, where its slice starts,
    how long it is, and what boundary it must start on. The slices follow,
    padded out to that boundary.
    """
    arches = []
    for blob in blobs:
        arch = thin_arch(blob)
        if arch is None:
            raise ValueError("cannot merge: not a single-architecture Mach-O")
        arches.append((arch, blob))

    seen = [a for a, _ in arches]
    if len(set(seen)) != len(seen):
        raise ValueError(f"cannot merge: duplicate architecture in {seen}")

    step = 1 << FAT_ALIGN
    header_len = 8 + 20 * len(arches)

    offsets = []
    pos = header_len
    for _, blob in arches:
        pos = -(-pos // step) * step  # round up to the alignment boundary
        offsets.append(pos)
        pos += len(blob)

    out = bytearray(struct.pack(">II", FAT_MAGIC, len(arches)))
    for ((cputype, cpusubtype), blob), offset in zip(arches, offsets):
        out += struct.pack(">IIIII", cputype, cpusubtype, offset, len(blob), FAT_ALIGN)
    for (_, blob), offset in zip(arches, offsets):
        out += b"\x00" * (offset - len(out))
        out += blob

    return bytes(out)


def merged(name, sources):
    """The bytes for one file, combining every published copy of it.

    Most files are identical across architectures - the managed assemblies, and
    the dylibs that already ship universal - and are passed through untouched.
    Only the ones that genuinely differ are merged.
    """
    blobs = []
    for src in sources:
        path = os.path.join(src, name)
        if not os.path.isfile(path):
            raise SystemExit(f"{name} is in {sources[0]} but not in {src}")
        with open(path, "rb") as fh:
            blobs.append(fh.read())

    if all(b == blobs[0] for b in blobs[1:]):
        return blobs[0]

    if any(is_fat(b) for b in blobs):
        raise SystemExit(f"{name} differs between builds and is already universal")

    return fat_binary(blobs)


def build(published, out_dir, icon):
    app = os.path.join(out_dir, f"{APP_NAME}.app")
    if os.path.exists(app):
        shutil.rmtree(app)

    macos = os.path.join(app, "Contents", "MacOS")
    resources = os.path.join(app, "Contents", "Resources")
    os.makedirs(macos)
    os.makedirs(resources)

    # The whole self-contained payload lives beside the executable: the .NET
    # host looks for its runtime next to itself, so splitting the dylibs into
    # Contents/Frameworks would mean teaching it where they went.
    #
    # The first directory decides what the payload contains; the rest only
    # contribute their architecture of each file.
    for entry in sorted(os.listdir(published[0])):
        src = os.path.join(published[0], entry)
        dst = os.path.join(macos, entry)

        if os.path.isdir(src):
            shutil.copytree(src, dst)
        elif len(published) == 1:
            shutil.copy2(src, dst)
        else:
            with open(dst, "wb") as fh:
                fh.write(merged(entry, published))

    exe = os.path.join(macos, APP_NAME)
    if not os.path.isfile(exe):
        raise SystemExit(f"no {APP_NAME} executable in {published}")
    os.chmod(exe, 0o755)

    shutil.copy2(icon, os.path.join(resources, "app.icns"))

    with open(os.path.join(app, "Contents", "Info.plist"), "wb") as fh:
        plistlib.dump(info_plist(), fh)

    return app


def archive(app, out_dir, arch):
    """Tar the bundle, forcing modes that survive a build on any OS."""
    path = os.path.join(out_dir, f"{APP_NAME}-macos-{arch}.tar.gz")
    root = os.path.basename(app)
    exe_rel = os.path.join(root, "Contents", "MacOS", APP_NAME)

    def fix(item):
        # Directories and the executable need the x bit; nothing else does.
        # dylibs are loaded, not run, so 644 is correct for them.
        if item.isdir():
            item.mode = 0o755
        else:
            item.mode = 0o755 if item.name == exe_rel.replace(os.sep, "/") else 0o644
        item.uid = item.gid = 0
        item.uname = item.gname = ""
        return item

    with tarfile.open(path, "w:gz") as tar:
        tar.add(app, arcname=root, filter=fix)

    return path


def main():
    here = os.path.dirname(os.path.abspath(__file__))

    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("published", nargs="+",
                    help="one directory from `dotnet publish -r osx-*`, or "
                         "several to merge into a universal binary")
    ap.add_argument("--arch", required=True,
                    help="label for the archive name, e.g. universal or apple-silicon")
    ap.add_argument("--out", default=None, help="where to write (default: alongside)")
    ap.add_argument("--icon", default=os.path.join(here, "Assets", "app.icns"))
    ap.add_argument("--no-archive", action="store_true", help="build the .app only")
    args = ap.parse_args()

    for d in args.published:
        if not os.path.isdir(d):
            raise SystemExit(f"not a directory: {d}")
    if not os.path.isfile(args.icon):
        raise SystemExit(f"no icon at {args.icon} - run 'python make_icon.py' first")

    out_dir = args.out or os.path.dirname(os.path.abspath(args.published[0]))
    os.makedirs(out_dir, exist_ok=True)

    app = build(args.published, out_dir, args.icon)
    print(f"built {app}")

    if not args.no_archive:
        path = archive(app, out_dir, args.arch)
        print(f"wrote {path} ({os.path.getsize(path) / 1e6:.1f} MB)")

    return 0


if __name__ == "__main__":
    sys.exit(main())
