import os
import sys
from io import BytesIO
from pathlib import Path

import fitz
from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, str(Path(os.environ["TEMP"]) / "codex_bidi"))
from bidi.algorithm import get_display


ROOT = Path(__file__).resolve().parent
PDF = ROOT / "Driver_Moodle_FINAL_v15_REBUILT.pdf"
TMP = ROOT / "_Driver_Moodle_FINAL_v15_REBUILT_wording_fixed.pdf"
FONT = Path(r"C:\Windows\Fonts\arial.ttf")


def text_image(text, width_pt, height_pt, font_size_pt, color):
    scale = 4
    img = Image.new("RGB", (int(width_pt * scale), int(height_pt * scale)), "white")
    draw = ImageDraw.Draw(img)
    font = ImageFont.truetype(str(FONT), int(font_size_pt * scale))
    draw.text((img.width // 2, img.height // 2), get_display(text), font=font,
              fill=color, anchor="mm")
    stream = BytesIO()
    img.save(stream, format="PNG")
    return stream.getvalue()


def replace_with_image(page, rect, text, font_size, color):
    page.add_redact_annot(rect, fill=(1, 1, 1))
    page.apply_redactions()
    page.insert_image(rect, stream=text_image(text, rect.width, rect.height,
                                             font_size, color), overlay=True)


def main():
    doc = fitz.open(PDF)
    replace_with_image(
        doc[5],
        fitz.Rect(220.5, 355.4, 254.5, 370.7),
        "מצב",
        9.2,
        "black",
    )
    replace_with_image(
        doc[10],
        fitz.Rect(145, 613.2, 445, 630.4),
        "תרשים 5: מבנה הפתרון לפי התיקיות והפרויקטים האמיתיים בפרויקט.",
        9.0,
        "#5B6573",
    )
    if TMP.exists():
        TMP.unlink()
    doc.save(TMP, garbage=4, deflate=True)
    doc.close()
    TMP.replace(PDF)
    print(PDF)


if __name__ == "__main__":
    main()
