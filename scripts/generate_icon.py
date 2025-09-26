#!/usr/bin/env python3
"""
Generate a valid Windows .ico file containing multiple sizes from a source image.

Usage:
  python scripts/generate_icon.py --source <path-to-image> --out <path-to-ico>

Defaults:
  --source: tries to auto-detect a PNG in CWD with 'icon' in the name, else first PNG
  --out: gym_icon.ico in CWD
"""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

try:
    from PIL import Image
except Exception as exc:  # pragma: no cover
    print("Pillow is required: pip install pillow", file=sys.stderr)
    raise


DEFAULT_SIZES = [16, 24, 32, 48, 64, 128, 256]


def auto_find_source(cwd: Path) -> Path | None:
    candidates = list(cwd.glob("*icon*.png")) + list(cwd.glob("*.png"))
    return candidates[0] if candidates else None


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate multi-size .ico from an image")
    parser.add_argument("--source", type=str, default=None, help="Source image (PNG preferred)")
    parser.add_argument("--out", type=str, default="gym_icon.ico", help="Output .ico path")
    parser.add_argument("--sizes", type=str, default=None, help="Comma-separated sizes, e.g. 16,32,48,256")
    args = parser.parse_args()

    cwd = Path.cwd()
    out_path = Path(args.out)

    sizes = DEFAULT_SIZES
    if args.sizes:
        try:
            sizes = [int(s.strip()) for s in args.sizes.split(",") if s.strip()]
        except ValueError:
            print("Invalid --sizes; use comma-separated integers, e.g. 16,32,48,256", file=sys.stderr)
            return 2

    source = Path(args.source) if args.source else auto_find_source(cwd)
    if not source or not source.exists():
        print("Source image not found. Provide --source <file.png>.", file=sys.stderr)
        return 1

    try:
        with Image.open(source) as im:
            # Convert to RGBA to ensure proper alpha handling
            im = im.convert("RGBA")

            # Save ICO with provided sizes; Pillow will generate each size from the source
            im.save(
                out_path,
                format="ICO",
                sizes=[(sz, sz) for sz in sizes],
            )

        print(f"Wrote icon: {out_path} with sizes: {sizes}")
        return 0
    except Exception as exc:
        print(f"Failed to generate ICO: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
