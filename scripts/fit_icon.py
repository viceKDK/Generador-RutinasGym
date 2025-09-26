#!/usr/bin/env python3
"""
Auto-crop a transparent PNG to its non-transparent content, add padding,
fit to a square, and export a 32x32 ICO (or PNG).

Usage:
  python scripts/fit_icon.py --source icon_transparent.png --out gym_icon.ico --size 32 --padding 8
"""
from __future__ import annotations

import argparse
from pathlib import Path
from PIL import Image


def autocrop_to_content(im: Image.Image, alpha_threshold: int = 1) -> Image.Image:
    rgba = im.convert("RGBA")
    alpha = rgba.split()[3]
    # Create a mask where alpha > threshold
    mask = alpha.point(lambda a: 255 if a > alpha_threshold else 0)
    bbox = mask.getbbox()
    if bbox:
        return rgba.crop(bbox)
    return rgba


def normalize_transparent_pixels(im: Image.Image) -> Image.Image:
    rgba = im.convert("RGBA")
    px = rgba.load()
    w, h = rgba.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                px[x, y] = (255, 255, 255, 0)
    return rgba


def fit_square_with_padding(
    im: Image.Image,
    padding_percent: float = 8.0,
    background=(255, 255, 255, 0),
) -> Image.Image:
    """
    Place content centered in a square canvas. If padding_percent >= 0, add that
    amount of border around the content (as % of content max side). If
    padding_percent < 0, overscale the content so it overflows the canvas by the
    absolute padding amount, effectively making it visually larger.
    """
    w, h = im.size
    side = max(w, h)
    p = padding_percent / 100.0

    if p >= 0:
        pad = int(round(side * p))
        target_side = side + pad * 2
        canvas = Image.new("RGBA", (target_side, target_side), background)
        x = (target_side - w) // 2
        y = (target_side - h) // 2
        canvas.paste(im, (x, y), im)
        return normalize_transparent_pixels(canvas)
    else:
        # Overscale: scale content by factor = 1/(1+2p), with p negative.
        denom = (1 + 2 * p)
        if denom <= 0:
            denom = 0.01  # prevent degenerate values
        scale = 1.0 / denom
        new_w = max(1, int(round(w * scale)))
        new_h = max(1, int(round(h * scale)))
        large = im.resize((new_w, new_h), Image.LANCZOS)
        # Canvas stays at original max side (no positive padding), we paste centered and crop overflow
        target_side = side
        canvas = Image.new("RGBA", (target_side, target_side), background)
        x = (target_side - new_w) // 2
        y = (target_side - new_h) // 2
        canvas.paste(large, (x, y), large)
        return normalize_transparent_pixels(canvas)


def main() -> int:
    ap = argparse.ArgumentParser(description="Crop transparent PNG and fit to square with padding")
    ap.add_argument("--source", required=True, help="Input transparent PNG")
    ap.add_argument("--out", required=True, help="Output path (.ico or .png)")
    ap.add_argument("--size", type=int, default=32, help="Output size (square)")
    ap.add_argument("--padding", type=float, default=8.0, help="Padding percent relative to content's max side (negative overscales)")
    ap.add_argument("--edge-margin", type=int, default=None, help="Exact margin in pixels on each edge in the final size; overrides padding if set")
    args = ap.parse_args()

    src = Path(args.source)
    out = Path(args.out)
    if not src.exists():
        print(f"Source not found: {src}")
        return 1

    im = Image.open(src).convert("RGBA")
    tight = autocrop_to_content(im)

    if args.edge_margin is not None:
        # Compute scale so that max(content) becomes (size - 2*margin)
        cw, ch = tight.size
        side = max(cw, ch)
        target_side = args.size
        margin = max(0, int(args.edge_margin))
        # Guard against degenerate values
        usable = max(1, target_side - 2 * margin)
        scale = usable / float(side if side else 1)
        new_w = max(1, int(round(cw * scale)))
        new_h = max(1, int(round(ch * scale)))
        scaled = tight.resize((new_w, new_h), Image.LANCZOS)
        canvas = Image.new("RGBA", (target_side, target_side), (255, 255, 255, 0))
        x = (target_side - new_w) // 2
        y = (target_side - new_h) // 2
        canvas.paste(scaled, (x, y), scaled)
        final = normalize_transparent_pixels(canvas)
    else:
        fitted = fit_square_with_padding(tight, padding_percent=args.padding)
        final = fitted.resize((args.size, args.size), Image.LANCZOS)

    if out.suffix.lower() == ".ico":
        final.save(out, format="ICO", sizes=[(args.size, args.size)])
    else:
        final.save(out, format="PNG")

    print(f"Saved: {out} size={args.size} padding={args.padding}%")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
