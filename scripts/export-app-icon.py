from pathlib import Path

from PIL import Image

master_path = Path(
    r"C:\Users\k-mizukami\.cursor\projects\c-Users-k-mizukami-Desktop-km-timer\assets\km-timer-icon-master.png"
)
out_dir = Path(r"C:\Users\k-mizukami\Desktop\km timer\KmTimer\Assets")
out_dir.mkdir(parents=True, exist_ok=True)

src = Image.open(master_path).convert("RGBA")


def trim_alpha(im: Image.Image, threshold: int = 8, pad_ratio: float = 0.08) -> Image.Image:
    alpha = im.split()[-1]
    bbox = alpha.point(lambda a: 255 if a > threshold else 0).getbbox()
    if not bbox:
        return im
    cropped = im.crop(bbox)
    side = max(cropped.size)
    pad = int(side * pad_ratio)
    canvas = Image.new("RGBA", (side + pad * 2, side + pad * 2), (0, 0, 0, 0))
    ox = (canvas.width - cropped.width) // 2
    oy = (canvas.height - cropped.height) // 2
    canvas.paste(cropped, (ox, oy), cropped)
    return canvas


def fit_square(im: Image.Image, size: int) -> Image.Image:
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    target = int(size * 0.86)
    scaled = im.copy()
    scaled.thumbnail((target, target), Image.Resampling.LANCZOS)
    x = (size - scaled.width) // 2
    y = (size - scaled.height) // 2
    canvas.paste(scaled, (x, y), scaled)
    return canvas


def fit_rect(im: Image.Image, width: int, height: int, bg=(15, 23, 42, 255)) -> Image.Image:
    canvas = Image.new("RGBA", (width, height), bg)
    target = int(min(width, height) * 0.72)
    scaled = im.copy()
    scaled.thumbnail((target, target), Image.Resampling.LANCZOS)
    x = (width - scaled.width) // 2
    y = (height - scaled.height) // 2
    canvas.paste(scaled, (x, y), scaled)
    return canvas


mark = trim_alpha(src)
square_512 = fit_square(mark, 512)
square_512.save(out_dir / "AppIcon.png")

ico_sizes = [16, 24, 32, 48, 64, 128, 256]
ico_images = [fit_square(mark, s) for s in ico_sizes]
ico_images[-1].save(
    out_dir / "AppIcon.ico",
    format="ICO",
    sizes=[(s, s) for s in ico_sizes],
    append_images=ico_images[:-1],
)

exports = {
    "Square44x44Logo.scale-200.png": fit_square(mark, 88),
    "Square44x44Logo.targetsize-24_altform-unplated.png": fit_square(mark, 24),
    "Square44x44Logo.targetsize-48_altform-lightunplated.png": fit_square(mark, 48),
    "Square150x150Logo.scale-200.png": fit_square(mark, 300),
    "StoreLogo.png": fit_square(mark, 50),
    "LockScreenLogo.scale-200.png": fit_square(mark, 48),
    "Wide310x150Logo.scale-200.png": fit_rect(mark, 620, 300),
    "SplashScreen.scale-200.png": fit_rect(mark, 1240, 600),
}

for name, im in exports.items():
    im.save(out_dir / name, format="PNG", optimize=True)

print("Wrote:")
for p in sorted(out_dir.iterdir()):
    if p.suffix.lower() in {".ico", ".png"}:
        im = Image.open(p)
        print(f"  {p.name}: {im.size}")
