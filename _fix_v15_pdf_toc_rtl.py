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
TMP = ROOT / "_Driver_Moodle_FINAL_v15_REBUILT_toc_fixed.pdf"
PREVIEW = ROOT / "_render_v15_main" / "toc-rtl-preview.png"

FONT = Path(r"C:\Windows\Fonts\arial.ttf")
BOLD = Path(r"C:\Windows\Fonts\arialbd.ttf")

ENTRIES = [
    (0, "בדיקת דרישות המחוון", 3),
    (0, "פרק 1: מבוא ותרחישי שימוש", 4),
    (1, "למה בחרתי בפרויקט", 4),
    (1, "מי משתמש במערכת", 4),
    (1, "Use Case", 4),
    (0, "פרק 2: בסיס הנתונים", 6),
    (1, "טבלאות מרכזיות", 6),
    (1, "קשרים בין הטבלאות", 6),
    (1, "קשרים לוגיים נוספים", 7),
    (1, "גישה לבסיס הנתונים", 8),
    (0, "פרק 3: צד השרת", 10),
    (1, "מבנה כללי", 10),
    (1, "פרויקט Model", 11),
    (1, "פרויקט ViewDB", 12),
    (1, "פרויקט BusinessLogic", 13),
    (1, "שירות WCF", 13),
    (1, "קובצי App.config והגדרות הרשת", 13),
    (0, "פרק 4: צד הלקוח", 14),
    (1, "לקוח WPF", 14),
    (2, "Binding, XAML ו-IValueConverter", 15),
    (1, "לקוח Web", 15),
    (1, "לקוח MAUI / Android", 15),
    (2, "Async / Tasks", 15),
    (0, "פרק 5: תהליכים מרכזיים ואבטחה", 16),
    (1, "קביעת שיעור", 16),
    (1, "התראות", 16),
    (1, "תשלומים ודוחות", 17),
    (1, "בדיקות קלט ואבטחה", 17),
    (0, "פרק 6: מדריך הפעלה ובדיקות", 18),
    (1, "הפעלה קצרה", 18),
    (1, "בדיקות שביצעתי", 18),
    (1, "הערה לגבי מסכי הקורסים", 18),
    (0, "פרק 7: נספחים", 19),
    (1, "קבצים שכדאי לפתוח בזמן הבחינה", 19),
    (1, "סיכום קצר", 19),
]


def draw_toc():
    scale = 2
    width, height = 595 * scale, 842 * scale
    img = Image.new("RGB", (width, height), "white")
    draw = ImageDraw.Draw(img)
    title_font = ImageFont.truetype(str(BOLD), 17 * scale)
    row_font = ImageFont.truetype(str(FONT), 9 * scale)
    row_bold = ImageFont.truetype(str(BOLD), 9 * scale)
    footer_font = ImageFont.truetype(str(FONT), 8 * scale)

    blue = "#1F4E79"
    gray = "#5B6573"
    x_right = width - 52 * scale
    x_left = 52 * scale
    y = 66 * scale
    draw.text((x_right, y), get_display("תוכן עניינים"), font=title_font,
              fill=blue, anchor="ra")
    y += 31 * scale

    links = []
    for level, text, page in ENTRIES:
        font = row_bold if level == 0 else row_font
        indent = (level * 14) * scale
        row_y = y
        right = x_right - indent
        visual_text = get_display(text)
        bbox = draw.textbbox((right, row_y), visual_text, font=font, anchor="ra")
        text_left = bbox[0]
        draw.text((right, row_y), visual_text, font=font, fill="black",
                  anchor="ra")
        number = str(page)
        draw.text((x_left, row_y), number, font=font, fill="black", anchor="la")
        number_bbox = draw.textbbox((x_left, row_y), number, font=font, anchor="la")
        dot_start = number_bbox[2] + 8 * scale
        dot_end = text_left - 8 * scale
        dot_y = row_y + 8 * scale
        for x in range(int(dot_start), int(dot_end), 4 * scale):
            draw.ellipse((x, dot_y, x + scale, dot_y + scale), fill="#5B6573")
        links.append((row_y - 2 * scale, row_y + 13 * scale, page))
        y += 18 * scale

    footer = "אלי רפופורט | Driver Moodle | עמוד 2"
    draw.text((width // 2, height - 25 * scale), get_display(footer),
              font=footer_font, fill=gray, anchor="ma")
    return img, links


def replace_page(img, links):
    src = fitz.open(PDF)
    if len(src) != 19:
        raise RuntimeError(f"Expected 19 main-document pages, found {len(src)}")

    toc = fitz.open()
    page = toc.new_page(width=src[1].rect.width, height=src[1].rect.height)
    image_stream = BytesIO()
    img.save(image_stream, format="PNG")
    page.insert_image(page.rect, stream=image_stream.getvalue())

    merged = fitz.open()
    merged.insert_pdf(src, from_page=0, to_page=0)
    merged.insert_pdf(toc)
    merged.insert_pdf(src, from_page=2, to_page=len(src) - 1)
    merged_toc = merged[1]
    sy = merged_toc.rect.height / img.height
    for top, bottom, destination_page in links:
        rect = fitz.Rect(42, top * sy, merged_toc.rect.width - 42, bottom * sy)
        merged_toc.insert_link({
            "kind": fitz.LINK_GOTO,
            "from": rect,
            "page": destination_page - 1,
        })
    merged.set_metadata(src.metadata)
    merged.save(TMP, garbage=4, deflate=True)
    src.close()
    toc.close()
    merged.close()
    TMP.replace(PDF)


def main():
    img, links = draw_toc()
    PREVIEW.parent.mkdir(exist_ok=True)
    img.save(PREVIEW)
    replace_page(img, links)
    print(PDF)
    print(PREVIEW)


if __name__ == "__main__":
    main()
