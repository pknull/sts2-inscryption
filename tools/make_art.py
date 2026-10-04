"""Turn generated art in assets/gen/ into the files the game loads from Inscryption/images/.

For each creature, one fal image yields two assets:
  - card portrait: the full image, centre-cropped to the card's 1000x760 frame (plus a 250x190 copy);
  - board sprite: the background-removed cutout, trimmed and scaled to the creature's on-board height.

Run from the repo root: python3 tools/make_art.py
"""
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
GEN = ROOT / "assets" / "gen"
IMAGES = ROOT / "Inscryption" / "images"

# card id -> (source image stem, cutout stem, board sprite height in px)
CREATURES = {
    "squirrel": ("squirrel_flat", "squirrel_flat_cut", 90),
    "stoat": ("stoat_flat_probe", "stoat_flat_cut", 110),
    "bullfrog": ("bullfrog_flat", "bullfrog_flat_cut", 120),
    "wolf": ("wolf_flat", "wolf_flat_cut", 180),
}


def cover(img: Image.Image, size: tuple[int, int]) -> Image.Image:
    """Scale to fill `size`, then centre-crop."""
    w, h = size
    scale = max(w / img.width, h / img.height)
    resized = img.resize((round(img.width * scale), round(img.height * scale)), Image.Resampling.LANCZOS)
    left = (resized.width - w) // 2
    top = (resized.height - h) // 2
    return resized.crop((left, top, left + w, top + h))


def sprite(cutout: Image.Image, height: int) -> Image.Image:
    """Trim transparent margins, then scale to `height` keeping the aspect ratio."""
    trimmed = cutout.crop(cutout.getchannel("A").getbbox())
    width = round(trimmed.width * height / trimmed.height)
    return trimmed.resize((width, height), Image.Resampling.LANCZOS)


def main() -> None:
    (IMAGES / "card_portraits" / "big").mkdir(parents=True, exist_ok=True)
    (IMAGES / "creatures").mkdir(parents=True, exist_ok=True)
    for card_id, (source, cutout, height) in CREATURES.items():
        art = Image.open(GEN / f"{source}.png").convert("RGB")
        cover(art, (1000, 760)).save(IMAGES / "card_portraits" / "big" / f"{card_id}.png")
        cover(art, (250, 190)).save(IMAGES / "card_portraits" / f"{card_id}.png")
        board = sprite(Image.open(GEN / f"{cutout}.png").convert("RGBA"), height)
        board.save(IMAGES / "creatures" / f"{card_id}_creature.png")
        print(f"{card_id}: portrait 1000x760 + 250x190, sprite {board.width}x{board.height}")


if __name__ == "__main__":
    main()
