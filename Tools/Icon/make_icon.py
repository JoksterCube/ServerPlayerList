# Builds the ServerPlayerList icon on top of the shared base (Thunderstore/icon_base.png).
# Same corner badge frame as FairCraftingLuck's clover_badge, with a pair of player silhouettes inside.
# Usage (needs Pillow): python make_icon.py <icon_base.png> <output icon.png>
import sys
from PIL import Image, ImageDraw, ImageFilter

base_path, out_path = sys.argv[1], sys.argv[2]
W, SS = 256, 4
base = Image.open(base_path).convert("RGBA")

DARK = (24, 20, 16, 255)

def person(d, x, y, scale, fill, pad):
    # head circle + shoulders that run off the bottom of the badge face, centered on (x, y) = head center
    hr = 7.4 * scale * SS + pad
    d.ellipse([x - hr, y - hr, x + hr, y + hr], fill=fill)
    bw, top = 14 * scale * SS + pad, y + 10.2 * scale * SS - pad
    d.rounded_rectangle([x - bw, top, x + bw, top + 40 * SS], radius=11 * scale * SS + pad, fill=fill)

def players_badge(img, cx=204, cy=204, r=40):
    layer = Image.new("RGBA", (W * SS, W * SS), (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)
    X, Y, R = cx * SS, cy * SS, r * SS
    # shadow, dark rim, gold ring, cream face
    sh = Image.new("RGBA", layer.size, (0, 0, 0, 0))
    ImageDraw.Draw(sh).ellipse([X - R, Y - R + 3 * SS, X + R, Y + R + 3 * SS], fill=(0, 0, 0, 120))
    sh = sh.filter(ImageFilter.GaussianBlur(4 * SS))
    d.ellipse([X - R, Y - R, X + R, Y + R], fill=DARK)
    R2 = R - 3.5 * SS
    d.ellipse([X - R2, Y - R2, X + R2, Y + R2], fill=(176, 132, 62, 255))
    R3 = R2 - 5 * SS
    d.ellipse([X - R3, Y - R3, X + R3, Y + R3], fill=DARK)
    R4 = R3 - 2.5 * SS
    d.ellipse([X - R4, Y - R4, X + R4, Y + R4], fill=(238, 226, 190, 255))
    # two players drawn on their own layer, clipped to the cream face
    fig = Image.new("RGBA", layer.size, (0, 0, 0, 0))
    f = ImageDraw.Draw(fig)
    pad = 1.6 * SS
    back = (X + 9 * SS, Y - 8 * SS, 0.82, (128, 184, 86, 255))
    front = (X - 4 * SS, Y - 2 * SS, 1.0, (92, 150, 62, 255))
    for px, py, s, col in (back, front):
        person(f, px, py, s, DARK, pad)
        person(f, px, py, s, col, 0)
    clip = Image.new("L", layer.size, 0)
    ImageDraw.Draw(clip).ellipse([X - R4, Y - R4, X + R4, Y + R4], fill=255)
    fig.putalpha(Image.composite(fig.getchannel("A"), clip, clip))
    layer = Image.alpha_composite(layer, fig)
    combined = Image.alpha_composite(sh, layer).resize((W, W), Image.LANCZOS)
    return Image.alpha_composite(img, combined)

players_badge(base).save(out_path, optimize=True)
