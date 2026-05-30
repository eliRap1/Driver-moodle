import os
import sys
from io import BytesIO
from pathlib import Path

import fitz
from docx import Document
from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, str(Path(os.environ["TEMP"]) / "codex_bidi"))
from bidi.algorithm import get_display


ROOT = Path(__file__).resolve().parent
DOCX = ROOT / "Driver_Moodle_EXAM_QA_v15.docx"
PDF = ROOT / "Driver_Moodle_EXAM_QA_v15.pdf"

FONT = Path(r"C:\Windows\Fonts\arial.ttf")
BOLD = Path(r"C:\Windows\Fonts\arialbd.ttf")

BLUE = "#1F4E79"
GRAY = "#5B6573"


def visual(text):
    return get_display(text)


def wrap(draw, text, font, max_width):
    words = text.split()
    lines = []
    current = ""
    for word in words:
        candidate = f"{current} {word}".strip()
        box = draw.textbbox((0, 0), visual(candidate), font=font)
        if current and box[2] - box[0] > max_width:
            lines.append(current)
            current = word
        else:
            current = candidate
    if current:
        lines.append(current)
    return lines


def load_qa():
    doc = Document(DOCX)
    paragraphs = [p for p in doc.paragraphs if p.text.strip()]
    title = paragraphs[0].text.strip()
    subtitle = paragraphs[1].text.strip()
    intro = paragraphs[2].text.strip()
    questions = []
    current = None
    for p in paragraphs[3:]:
        text = p.text.strip()
        if p.style.name.startswith("Heading"):
            if current:
                questions.append(current)
            current = [text, ""]
        elif current:
            current[1] = f"{current[1]} {text}".strip()
    if current:
        questions.append(current)
    return title, subtitle, intro, questions


def new_page():
    scale = 2
    img = Image.new("RGB", (595 * scale, 842 * scale), "white")
    return img, ImageDraw.Draw(img), scale


def build_pdf():
    title, subtitle, intro, questions = load_qa()
    title_font = ImageFont.truetype(str(BOLD), 22 * 2)
    subtitle_font = ImageFont.truetype(str(FONT), 14 * 2)
    intro_font = ImageFont.truetype(str(FONT), 10 * 2)
    q_font = ImageFont.truetype(str(BOLD), 12 * 2)
    a_font = ImageFont.truetype(str(FONT), 10 * 2)
    footer_font = ImageFont.truetype(str(FONT), 8 * 2)

    pages = []
    img, draw, scale = new_page()
    x_right = img.width - 46 * scale
    x_left = 46 * scale
    max_width = x_right - x_left
    y = 42 * scale
    draw.text((img.width // 2, y), title, font=title_font, fill=BLUE, anchor="ma")
    y += 30 * scale
    draw.text((img.width // 2, y), visual(subtitle), font=subtitle_font,
              fill="black", anchor="ma")
    y += 31 * scale
    for line in wrap(draw, intro, intro_font, max_width):
        draw.text((x_right, y), visual(line), font=intro_font, fill=GRAY,
                  anchor="ra")
        y += 14 * scale
    y += 8 * scale

    def finish_page(page_number):
        footer = f"אלי רפופורט | Driver Moodle | עמוד {page_number}"
        draw.text((img.width // 2, img.height - 24 * scale), visual(footer),
                  font=footer_font, fill=GRAY, anchor="ma")
        pages.append(img.copy())

    for question, answer in questions:
        q_lines = wrap(draw, question, q_font, max_width)
        a_lines = wrap(draw, answer, a_font, max_width)
        needed = (len(q_lines) * 16 + len(a_lines) * 14 + 9) * scale
        if y + needed > img.height - 48 * scale:
            finish_page(len(pages) + 1)
            img, draw, scale = new_page()
            x_right = img.width - 46 * scale
            x_left = 46 * scale
            max_width = x_right - x_left
            y = 44 * scale
        for line in q_lines:
            draw.text((x_right, y), visual(line), font=q_font, fill=BLUE,
                      anchor="ra")
            y += 16 * scale
        for line in a_lines:
            draw.text((x_right, y), visual(line), font=a_font, fill="black",
                      anchor="ra")
            y += 14 * scale
        y += 9 * scale
    finish_page(len(pages) + 1)

    out = fitz.open()
    for page_img in pages:
        page = out.new_page(width=595, height=842)
        stream = BytesIO()
        page_img.save(stream, format="PNG")
        page.insert_image(page.rect, stream=stream.getvalue())
    if PDF.exists():
        PDF.unlink()
    out.save(PDF, garbage=4, deflate=True)
    out.close()
    print(f"QA_PDF={PDF}")
    print(f"QA_PAGES={len(pages)}")
    print(f"QA_QUESTIONS={len(questions)}")


if __name__ == "__main__":
    build_pdf()
