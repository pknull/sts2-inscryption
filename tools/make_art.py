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
    "sparrow": ("sparrow_flat", "sparrow_flat_cut", 80),
    "raven": ("raven_flat", "raven_flat_cut", 120),
    "bat": ("bat_flat", "bat_flat_cut", 110),
    "turkey_vulture": ("turkey_vulture_flat", "turkey_vulture_flat_cut", 140),
    "kingfisher": ("kingfisher_flat", "kingfisher_flat_cut", 80),
    "mantis": ("mantis_flat", "mantis_flat_cut", 100),
    "mantis_god": ("mantis_god_flat", "mantis_god_flat_cut", 130),
    "pronghorn": ("pronghorn_flat", "pronghorn_flat_cut", 150),
    "elk": ("elk_flat", "elk_flat_cut", 190),
    "long_elk": ("long_elk_flat", "long_elk_flat_cut", 200),
    "alpha": ("alpha_flat", "alpha_flat_cut", 150),
    "bloodhound": ("bloodhound_flat", "bloodhound_flat_cut", 130),
    "skunk": ("skunk_flat", "skunk_flat_cut", 90),
    "porcupine": ("porcupine_flat", "porcupine_flat_cut", 90),
    "adder": ("adder_flat", "adder_flat_cut", 80),
    "great_white": ("great_white_flat", "great_white_flat_cut", 120),
    "river_otter": ("river_otter_flat", "river_otter_flat_cut", 100),
    "mole": ("mole_flat", "mole_flat_cut", 80),
    "mole_man": ("mole_man_flat", "mole_man_flat_cut", 170),
    "black_goat": ("black_goat_flat", "black_goat_flat_cut", 120),
    "cat": ("cat_flat", "cat_flat_cut", 100),
    "undead_cat": ("undead_cat_flat", "undead_cat_flat_cut", 110),
    "cockroach": ("cockroach_flat", "cockroach_flat_cut", 70),
    "corpse_maggots": ("corpse_maggots_flat", "corpse_maggots_flat_cut", 70),
    "rat_king": ("rat_king_flat", "rat_king_flat_cut", 110),
    "ouroboros": ("ouroboros_flat", "ouroboros_flat_cut", 100),
    "frozen_opossum": ("frozen_opossum_flat", "frozen_opossum_flat_cut", 100),
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


# Rest site: Luke seated on nothing (the scene supplies the log). BaseLib draws a texture-only rest-site character
# with the point 60% down the image on the scene's seat, so the bottom is padded until his seat sits there.
REST_SEAT_ROW = 730  # underside of his hips in luke_rest_nolog_cut.png (1024 px source)
REST_SEAT_LINE = 0.6


def rest_site() -> Image.Image:
    cutout = Image.open(GEN / "luke_rest_nolog_cut.png").convert("RGBA")
    top = cutout.getchannel("A").getbbox()[1]
    figure = sprite(ImageOps.mirror(cutout), 300)
    seat = (REST_SEAT_ROW - top) * figure.height / (cutout.getchannel("A").getbbox()[3] - top)
    canvas = Image.new("RGBA", (figure.width, round(seat / REST_SEAT_LINE)), (0, 0, 0, 0))
    canvas.paste(figure, (0, 0), figure)
    return canvas


def character() -> None:
    """Luke Carder: combat body, character-select icons and splash, top-bar icon, map marker."""
    charui = IMAGES / "charui"
    # The art faces left; mirror it so Luke faces the enemies on the right.
    body = ImageOps.mirror(Image.open(GEN / "luke_body_v2.png").convert("RGB"))
    cutout = ImageOps.mirror(Image.open(GEN / "luke_body_v2_cut.png").convert("RGBA"))

    sprite(cutout, 340).save(charui / "body.png")
    # Shop (standing, larger) and rest site (seated, facing the fire on his right).
    sprite(cutout, 460).save(charui / "merchant.png")
    rest_site().save(charui / "rest_site.png")

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
    print("luke: body, merchant, rest site, select icon + locked, top-bar icon, map marker, select splash")


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


TOTEMS = ["canine", "hooved", "reptile", "avian", "insect", "squirrel"]


# Luke's own Strike and Defend: card portraits only. The punch is drawn facing left; mirrored to face the enemies.
BASICS = {"strike": ("strike_flat", True), "defend": ("defend_flat", False)}


def basics() -> None:
    for card, (source, mirror) in BASICS.items():
        art = Image.open(GEN / f"{source}.png").convert("RGB")
        if mirror:
            art = ImageOps.mirror(art)
        cover(art, (1000, 760)).save(IMAGES / "card_portraits" / "big" / f"{card}.png")
        cover(art, (250, 190)).save(IMAGES / "card_portraits" / f"{card}.png")
    print(f"basics: {len(BASICS)} portraits")


def totems() -> None:
    """Totem card portraits, and the Totem power icon from the Canine totem."""
    for tribe in TOTEMS:
        art = Image.open(GEN / f"totem_{tribe}.png").convert("RGB")
        cover(art, (1000, 760)).save(IMAGES / "card_portraits" / "big" / f"{tribe}_totem.png")
        cover(art, (250, 190)).save(IMAGES / "card_portraits" / f"{tribe}_totem.png")
    cut = Image.open(GEN / "totem_canine_cut.png").convert("RGBA")
    icon(cut, 64).save(IMAGES / "powers" / "totem_power.png")
    icon(cut, 256).save(IMAGES / "powers" / "big" / "totem_power.png")
    print(f"totems: {len(TOTEMS)} portraits, power icon")


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
    totems()
    basics()


if __name__ == "__main__":
    main()
