"""Turn generated art in assets/gen/ into the files the game loads from Inscryption/images/.

For each creature, one fal image yields two assets:
  - card portrait: the full image, centre-cropped to the card's 1000x760 frame (plus a 250x190 copy);
  - board sprite: the background-removed cutout, trimmed and scaled to the creature's on-board height.
Luke Carder's body, icons and character-select splash come from luke_body_v2(_cut).png and
luke_select_bg_v2.png.

Run from the repo root: python3 tools/make_art.py
"""
from pathlib import Path

from PIL import Image, ImageFilter, ImageOps

ROOT = Path(__file__).resolve().parent.parent
GEN = ROOT / "assets" / "gen"
IMAGES = ROOT / "Inscryption" / "images"

# card id -> (source image stem, cutout stem, board sprite height in px)
CREATURES = {
    "squirrel": ("squirrel_flat", "squirrel_flat_cut", 90),
    "stoat": ("stoat_flat_probe", "stoat_flat_cut", 110),
    "bullfrog": ("bullfrog_flat", "bullfrog_flat_cut", 120),
    "wolf": ("wolf_flat", "wolf_flat_cut", 180),
    "grizzly": ("grizzly_flat", "grizzly_flat_cut", 200),
    "river_snapper": ("river_snapper_flat", "river_snapper_flat_cut", 120),
    "ring_worm": ("ring_worm_flat", "ring_worm_flat_cut", 80),
    "urayuli": ("urayuli_flat", "urayuli_flat_cut", 260),
    "amalgam": ("amalgam_flat", "amalgam_flat_cut", 190),
    "geck": ("geck_flat", "geck_flat_cut", 70),
    "opossum": ("opossum_flat", "opossum_flat_cut", 90),
    "coyote": ("coyote_flat", "coyote_flat_cut", 140),
    "rattler": ("rattler_flat", "rattler_flat_cut", 80),
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


def fit_square(img: Image.Image, size: int) -> Image.Image:
    """Scale to fit inside a size x size transparent square, centred."""
    scale = min(size / img.width, size / img.height)
    resized = img.resize((round(img.width * scale), round(img.height * scale)), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    canvas.paste(resized, ((size - resized.width) // 2, (size - resized.height) // 2), resized)
    return canvas


def character() -> None:
    """Luke Carder: combat body, character-select icons and splash, top-bar icon, map marker."""
    charui = IMAGES / "charui"
    # The art faces left; mirror it so Luke faces the enemies on the right.
    body = ImageOps.mirror(Image.open(GEN / "luke_body_v2.png").convert("RGB"))
    cutout = ImageOps.mirror(Image.open(GEN / "luke_body_v2_cut.png").convert("RGBA"))

    sprite(cutout, 340).save(charui / "body.png")

    # Select icons keep the flat background; crop from the head to the knees (132x195 aspect).
    select_box = (230, 90, 704, 790)
    body.crop(select_box).resize((132, 195), Image.Resampling.LANCZOS).save(charui / "char_select_icon.png")
    locked = Image.new("RGB", body.size, (34, 34, 34))
    locked.paste((8, 8, 8), mask=cutout.getchannel("A"))
    locked.crop(select_box).resize((132, 195), Image.Resampling.LANCZOS).save(charui / "char_select_icon_locked.png")

    # Top-bar icon and map marker: head, hand and cards on transparency.
    head = cutout.crop((300, 90, 700, 490))
    fit_square(head, 128).save(charui / "character_icon.png")
    fit_square(head, 128).save(charui / "map_marker.png")

    cover(Image.open(GEN / "luke_select_bg_v2.png").convert("RGB"), (1920, 1080)).save(charui / "char_select_bg.png")
    print("luke: body, select icon + locked, top-bar icon, map marker, select splash")


def icon(cutout: Image.Image, size: int, margin: float = 0.06) -> Image.Image:
    """Trim a cutout and centre it in a size x size square with a small transparent margin."""
    trimmed = cutout.crop(cutout.getchannel("A").getbbox())
    inner = round(size * (1 - 2 * margin))
    return fit_square(fit_square(trimmed, max(trimmed.size)).resize((inner, inner), Image.Resampling.LANCZOS), size)


def outline(img: Image.Image, grow: int = 3) -> Image.Image:
    """White silhouette of `img`, grown by `grow` px: the relic outline the game draws behind the icon."""
    alpha = img.getchannel("A").filter(ImageFilter.MaxFilter(grow * 2 + 1))
    white = Image.new("RGBA", img.size, (255, 255, 255, 0))
    white.putalpha(alpha)
    return white


def icons() -> None:
    """Bones and Creature power icons, Side Deck relic icon."""
    powers, relics = IMAGES / "powers", IMAGES / "relics"
    (powers / "big").mkdir(parents=True, exist_ok=True)
    (relics / "big").mkdir(parents=True, exist_ok=True)
    for power_id, source in {"bones_power": "icon_bones_cut", "creature_power": "icon_paw_cut"}.items():
        cut = Image.open(GEN / f"{source}.png").convert("RGBA")
        icon(cut, 64).save(powers / f"{power_id}.png")
        icon(cut, 256).save(powers / "big" / f"{power_id}.png")
    cut = Image.open(GEN / "icon_side_deck_cut.png").convert("RGBA")
    small = icon(cut, 94, margin=0.1)
    small.save(relics / "side_deck.png")
    outline(small).save(relics / "side_deck_outline.png")
    icon(cut, 256).save(relics / "big" / "side_deck.png")
    (IMAGES / "ui").mkdir(parents=True, exist_ok=True)
    icon(Image.open(GEN / "icon_campfire_cut.png").convert("RGBA"), 256).save(IMAGES / "ui" / "rest_site_campfire.png")
    print("icons: bones_power, creature_power (64 + 256), side_deck relic (94 + outline + 256), campfire rest option")


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
    character()
    icons()


if __name__ == "__main__":
    main()
