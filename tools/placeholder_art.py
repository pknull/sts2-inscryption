"""Placeholder art for cards and powers that have no generated art yet.

Each card gets a plain portrait with its name (1000x760 plus a 250x190 copy); each power a lettered disc
(64 plus 256). make_art.py overwrites them once real art is in assets/gen/. Existing files are left alone, so
running this never replaces real art.

Run from the repo root: python3 tools/placeholder_art.py
"""
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent.parent
IMAGES = ROOT / "Inscryption" / "images"
FONT = "/usr/share/fonts/truetype/dejavu/DejaVuSerif-Bold.ttf"

# card id -> (title, background colour by card type)
ATTACK, SKILL, POWER = (92, 38, 34), (34, 58, 74), (70, 52, 86)
CARDS = {
    "skinning_knife": ("Skinning Knife", ATTACK),
    "snare": ("Snare", ATTACK),
    "pack_hunt": ("Pack Hunt", ATTACK),
    "ring_the_bell": ("Ring the Bell", ATTACK),
    "death_knell": ("Death Knell", ATTACK),
    "squirrel_bottle": ("Squirrel Bottle", SKILL),
    "hoggy_bank": ("Hoggy Bank", SKILL),
    "hunting_horn": ("Hunting Horn", SKILL),
    "wolf_pelt": ("Wolf Pelt", SKILL),
    "magpies_lens": ("Magpie's Lens", SKILL),
    "ritual_knife": ("Ritual Knife", SKILL),
    "fish_hook": ("Fish Hook", SKILL),
    "hooked_card": ("Hooked", SKILL),
    "bared_fangs": ("Bared Fangs", POWER),
    "the_altar": ("The Altar", POWER),
    "boneyard": ("Boneyard", POWER),
}

# power id -> (letter, colour)
POWERS = {
    "ferocity_power": ("F", (176, 64, 48)),
    "hunting_horn_power": ("H", (196, 140, 52)),
    "altar_power": ("A", (120, 40, 40)),
    "boneyard_power": ("B", (210, 204, 186)),
}


def portrait(title: str, colour: tuple[int, int, int]) -> Image.Image:
    img = Image.new("RGB", (1000, 760), colour)
    draw = ImageDraw.Draw(img)
    darker = tuple(max(0, c - 24) for c in colour)
    for i in range(0, 1000 + 760, 40):
        draw.line([(i, 0), (i - 760, 760)], fill=darker, width=14)
    font = ImageFont.truetype(FONT, 92)
    box = draw.textbbox((0, 0), title, font=font)
    w, h = box[2] - box[0], box[3] - box[1]
    draw.text(((1000 - w) / 2, (760 - h) / 2 - 20), title, font=font, fill=(236, 226, 204))
    small = ImageFont.truetype(FONT, 40)
    note = "art pending"
    box = draw.textbbox((0, 0), note, font=small)
    draw.text(((1000 - (box[2] - box[0])) / 2, 520), note, font=small, fill=(200, 188, 164))
    return img


def disc(letter: str, colour: tuple[int, int, int], size: int) -> Image.Image:
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    m = size // 16
    draw.ellipse([m, m, size - m, size - m], fill=colour + (255,), outline=(30, 24, 20, 255), width=max(2, size // 24))
    font = ImageFont.truetype(FONT, int(size * 0.55))
    box = draw.textbbox((0, 0), letter, font=font)
    w, h = box[2] - box[0], box[3] - box[1]
    ink = (30, 24, 20, 255) if sum(colour) > 450 else (245, 238, 220, 255)
    draw.text(((size - w) / 2 - box[0], (size - h) / 2 - box[1]), letter, font=font, fill=ink)
    return img


def print_photo() -> Image.Image:
    """Stock board sprite for a Fish Hook catch whose photograph is missing: a blank white print."""
    img = Image.new("RGBA", (213, 180), (242, 237, 224, 255))
    draw = ImageDraw.Draw(img)
    draw.rectangle([8, 8, 204, 158], fill=(60, 66, 70, 255))
    font = ImageFont.truetype(FONT, 72)
    box = draw.textbbox((0, 0), "?", font=font)
    draw.text(((213 - (box[2] - box[0])) / 2 - box[0], (166 - (box[3] - box[1])) / 2 - box[1]), "?", font=font,
              fill=(220, 214, 200, 255))
    return img


def drop(size: int, ring: bool = False) -> Image.Image:
    """A blood drop (Luke's energy: Blood, Keeper 2026-10-07), drawn 4x and scaled down for smooth edges."""
    k = 4
    n = size * k
    img = Image.new("RGBA", (n, n), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cx, r = n / 2, n * 0.30
    cy = n * 0.62
    tip = n * 0.08
    outline = (40, 6, 8, 255)
    w = max(k, n // 28)
    d.polygon([(cx, tip), (cx - r * 0.92, cy - r * 0.38), (cx + r * 0.92, cy - r * 0.38)], fill=outline)
    d.ellipse([cx - r - w, cy - r - w, cx + r + w, cy + r + w], fill=outline)
    d.polygon([(cx, tip + w * 1.6), (cx - r * 0.86, cy - r * 0.36), (cx + r * 0.86, cy - r * 0.36)], fill=(150, 14, 20, 255))
    d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(150, 14, 20, 255))
    d.ellipse([cx - r * 0.78, cy - r * 0.62, cx + r * 0.62, cy + r * 0.82], fill=(196, 28, 34, 255))
    d.ellipse([cx - r * 0.55, cy - r * 0.55, cx - r * 0.15, cy - r * 0.05], fill=(255, 196, 190, 220))
    return img.resize((size, size), Image.Resampling.LANCZOS)


def counter_layer(layer: int) -> Image.Image:
    """The five layers BaseLib stacks into an energy counter (128 px; layers 2 and 3 rotate)."""
    n = 128 * 4
    img = Image.new("RGBA", (n, n), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    if layer == 1:
        d.ellipse([40, 40, n - 40, n - 40], fill=(28, 8, 10, 235), outline=(90, 18, 22, 255), width=14)
    elif layer == 2:
        for i in range(12):
            a = i * 30
            box = [24, 24, n - 24, n - 24]
            d.arc(box, a, a + 14, fill=(150, 30, 34, 200), width=10)
    elif layer == 4:
        img.alpha_composite(drop(n).resize((int(n * 0.78), int(n * 0.78)), Image.Resampling.LANCZOS),
                            (int(n * 0.11), int(n * 0.08)))
    return img.resize((128, 128), Image.Resampling.LANCZOS)


def blood() -> None:
    """Luke's energy shown as Blood: the card cost icon, the icon in card text and the energy counter."""
    ui = IMAGES / "charui"
    drop(74).save(ui / "big_energy.png")
    drop(24).save(ui / "text_energy.png")
    for layer in range(1, 6):
        counter_layer(layer).save(ui / f"energy_counter_{layer}.png")


def main() -> None:
    made = []
    blood()
    made.append("blood (energy icons and counter)")
    sprite = IMAGES / "creatures" / "hooked_creature.png"
    if not sprite.exists():
        print_photo().save(sprite)
        made.append("hooked_creature")
    big = IMAGES / "card_portraits" / "big"
    for card_id, (title, colour) in CARDS.items():
        if (big / f"{card_id}.png").exists():
            continue
        art = portrait(title, colour)
        art.save(big / f"{card_id}.png")
        art.resize((250, 190), Image.Resampling.LANCZOS).save(IMAGES / "card_portraits" / f"{card_id}.png")
        made.append(card_id)
    for power_id, (letter, colour) in POWERS.items():
        if (IMAGES / "powers" / f"{power_id}.png").exists():
            continue
        disc(letter, colour, 64).save(IMAGES / "powers" / f"{power_id}.png")
        disc(letter, colour, 256).save(IMAGES / "powers" / "big" / f"{power_id}.png")
        made.append(power_id)
    print(f"placeholders: {', '.join(made) or 'none needed'}")


if __name__ == "__main__":
    main()
