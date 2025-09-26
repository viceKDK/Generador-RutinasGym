#!/usr/bin/env python3
"""
Remove (make transparent) the uniform background of an image by sampling border pixels.

Usage:
  python scripts/remove_bg_transparent.py --source in.png --out out.png [--tolerance 28] [--feather 12]

Notes:
  - Auto-detects background color from the image border (corners + edges).
  - Applies a soft transparency ramp: fully transparent within tolerance, and
    gradual alpha between tolerance..(tolerance+feather) for smoother edges.
"""
from __future__ import annotations

import argparse
from collections import Counter
from pathlib import Path
from typing import Iterable, Tuple

from PIL import Image


def sample_border_colors(im: Image.Image, step: int = 10) -> list[Tuple[int, int, int]]:
    w, h = im.size
    px = im.convert("RGB")
    samples: list[Tuple[int, int, int]] = []

    # Corners
    corners = [(0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)]
    for x, y in corners:
        samples.append(px.getpixel((x, y)))

    # Top/bottom edges
    for x in range(0, w, max(1, w // step)):
        samples.append(px.getpixel((x, 0)))
        samples.append(px.getpixel((x, h - 1)))

    # Left/right edges
    for y in range(0, h, max(1, h // step)):
        samples.append(px.getpixel((0, y)))
        samples.append(px.getpixel((w - 1, y)))

    return samples


def pick_background_color(colors: Iterable[Tuple[int, int, int]]) -> Tuple[int, int, int]:
    # Choose the most common border color
    counter = Counter(colors)
    bg, _ = counter.most_common(1)[0]
    return bg


def color_distance(c1: Tuple[int, int, int], c2: Tuple[int, int, int]) -> float:
    # Use simple Euclidean distance in RGB
    return ((c1[0] - c2[0]) ** 2 + (c1[1] - c2[1]) ** 2 + (c1[2] - c2[2]) ** 2) ** 0.5


def remove_background(
    im: Image.Image,
    bg_rgb: Tuple[int, int, int],
    tolerance: float = 28.0,
    feather: float = 12.0,
) -> Image.Image:
    w, h = im.size
    src = im.convert("RGBA")
    pixels = src.load()

    hard = tolerance
    soft = tolerance + max(0.0, feather)

    for y in range(h):
        for x in range(w):
            r, g, b, a = pixels[x, y]
            d = color_distance((r, g, b), bg_rgb)
            if d <= hard:
                # fully transparent for near-background
                pixels[x, y] = (r, g, b, 0)
            elif d < soft:
                # soft ramp for smoother edges
                ratio = (d - hard) / (soft - hard)
                new_a = int(a * ratio)
                pixels[x, y] = (r, g, b, new_a)
            # else: keep original

    return src


def main() -> int:
    ap = argparse.ArgumentParser(description="Remove uniform background to transparency")
    ap.add_argument("--source", required=True, help="Input image path (PNG recommended)")
    ap.add_argument("--out", default="icon_transparent.png", help="Output PNG path")
    ap.add_argument("--tolerance", type=float, default=28.0, help="Color distance for full transparency")
    ap.add_argument("--feather", type=float, default=12.0, help="Additional distance for soft edge ramp")
    args = ap.parse_args()

    src_path = Path(args.source)
    if not src_path.exists():
        print(f"Source not found: {src_path}")
        return 1

    im = Image.open(src_path)
    border = sample_border_colors(im, step=20)
    bg = pick_background_color(border)

    out_im = remove_background(im, bg, tolerance=args.tolerance, feather=args.feather)
    out_path = Path(args.out)
    out_im.save(out_path, format="PNG")
    print(f"Saved with transparent background: {out_path} (bg~{bg})")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
