from pathlib import Path
import textwrap

import fitz


ROOT = Path(__file__).resolve().parent
APPENDIX = ROOT / "Driver_Moodle_SOURCE_CODE_APPENDIX_v15.pdf"
MAIN_PDF = ROOT / "Driver_Moodle_FINAL_v15_REBUILT.pdf"
COMBINED = ROOT / "Driver_Moodle_FINAL_v15_WITH_CODE.pdf"
INVENTORY = ROOT / "Driver_Moodle_SOURCE_CODE_APPENDIX_v15_FILES.txt"

EXCLUDED_PARTS = {
    ".git", ".claude", ".github", "bin", "obj", "Connected Services",
    "docx_unpacked", "_v9_unpack", "_doc_text", "_generated_assets_v12",
    "_generated_assets_v15", "_render_v12", "_render_v14_docx_skill",
    "nav-map",
}
EXTENSIONS = {".cs", ".xaml", ".cshtml", ".config", ".csproj", ".sln"}

PAGE = fitz.paper_rect("a4-l")
MARGIN_X = 34
TOP = 44
BOTTOM = 30
FONT_SIZE = 6.2
LINE_HEIGHT = 7.45
MAX_CHARS = 178


def source_files():
    files = []
    for path in ROOT.rglob("*"):
        if not path.is_file() or path.suffix.lower() not in EXTENSIONS:
            continue
        rel = path.relative_to(ROOT)
        if any(part in EXCLUDED_PARTS for part in rel.parts):
            continue
        if path.name.startswith("Driver_Moodle_"):
            continue
        files.append(path)
    return sorted(files, key=lambda p: str(p.relative_to(ROOT)).lower())


def clean_line(line):
    return line.replace("\t", "    ").replace("\x00", "")


def wrap_code(line):
    line = clean_line(line.rstrip("\r\n"))
    if not line:
        return [""]
    if len(line) <= MAX_CHARS:
        return [line]
    indent = len(line) - len(line.lstrip(" "))
    continuation = " " * min(indent + 4, 30) + "... "
    wrapped = textwrap.wrap(
        line,
        width=MAX_CHARS,
        subsequent_indent=continuation,
        replace_whitespace=False,
        drop_whitespace=False,
        break_long_words=True,
        break_on_hyphens=False,
    )
    return wrapped or [line[:MAX_CHARS]]


def add_fonts(page):
    page.insert_font(fontname="Arial", fontfile=r"C:\Windows\Fonts\arial.ttf")
    page.insert_font(fontname="ArialBold", fontfile=r"C:\Windows\Fonts\arialbd.ttf")
    page.insert_font(fontname="Consolas", fontfile=r"C:\Windows\Fonts\consola.ttf")


def draw_header(page, file_label, page_number):
    add_fonts(page)
    page.insert_text((MARGIN_X, 22), "Driver Moodle - Full Source Code Appendix",
                     fontname="ArialBold", fontsize=8.3, color=(0.12, 0.30, 0.47))
    page.insert_text((MARGIN_X, 35), file_label[:165],
                     fontname="Arial", fontsize=7.2, color=(0.28, 0.35, 0.42))
    page.draw_line((MARGIN_X, 39), (PAGE.width - MARGIN_X, 39),
                   color=(0.75, 0.80, 0.84), width=0.7)
    page.insert_text((PAGE.width - 90, PAGE.height - 15), f"Page {page_number}",
                     fontname="Arial", fontsize=7, color=(0.35, 0.40, 0.45))


def create_appendix():
    files = source_files()
    inventory_lines = []
    total_lines = 0
    doc = fitz.open()
    page_num = 0

    def new_page(label):
        nonlocal page_num
        page_num += 1
        page = doc.new_page(width=PAGE.width, height=PAGE.height)
        draw_header(page, label, page_num)
        return page, TOP + 10

    page, y = new_page("Appendix cover")
    page.insert_text((MARGIN_X, 105), "Driver Moodle", fontname="ArialBold",
                     fontsize=27, color=(0.12, 0.30, 0.47))
    page.insert_text((MARGIN_X, 145), "Full Source Code Appendix", fontname="ArialBold",
                     fontsize=20, color=(0.12, 0.30, 0.47))
    page.insert_text((MARGIN_X, 190),
                     "The appendix contains repository source files only.",
                     fontname="Arial", fontsize=12, color=(0.25, 0.30, 0.34))
    page.insert_text((MARGIN_X, 215),
                     "Generated folders such as bin, obj and Connected Services are excluded.",
                     fontname="Arial", fontsize=12, color=(0.25, 0.30, 0.34))
    page.insert_text((MARGIN_X, 270), f"Source files: {len(files)}",
                     fontname="ArialBold", fontsize=13, color=(0.12, 0.30, 0.47))

    for file_index, path in enumerate(files, 1):
        rel = str(path.relative_to(ROOT)).replace("\\", "/")
        try:
            text = path.read_text(encoding="utf-8-sig")
        except UnicodeDecodeError:
            text = path.read_text(encoding="cp1255", errors="replace")
        raw_lines = text.splitlines()
        total_lines += len(raw_lines)
        inventory_lines.append(f"{file_index:03d}. {rel} ({len(raw_lines)} lines)")
        page, y = new_page(f"{file_index:03d}/{len(files):03d}  {rel}")
        page.insert_text((MARGIN_X, y), f"FILE {file_index:03d}: {rel}",
                         fontname="ArialBold", fontsize=10.5, color=(0.12, 0.30, 0.47))
        y += 17
        for line_no, line in enumerate(raw_lines, 1):
            fragments = wrap_code(line)
            for frag_index, fragment in enumerate(fragments):
                if y > PAGE.height - BOTTOM:
                    page, y = new_page(f"{file_index:03d}/{len(files):03d}  {rel} (continued)")
                prefix = f"{line_no:04d}  " if frag_index == 0 else "      "
                value = prefix + fragment
                page.insert_text((MARGIN_X, y), value, fontname="Consolas",
                                 fontsize=FONT_SIZE, color=(0.10, 0.13, 0.16))
                y += LINE_HEIGHT

    metadata = {
        "title": "Driver Moodle - Full Source Code Appendix",
        "author": "Eli Rapoport",
        "subject": "Project book source appendix",
    }
    doc.set_metadata(metadata)
    doc.save(APPENDIX, garbage=4, deflate=True)
    INVENTORY.write_text(
        "Driver Moodle - source code appendix inventory\n"
        f"Files: {len(files)}\n"
        f"Source lines: {total_lines}\n\n" +
        "\n".join(inventory_lines),
        encoding="utf-8",
    )
    print(f"APPENDIX={APPENDIX}")
    print(f"FILES={len(files)}")
    print(f"LINES={total_lines}")
    print(f"PAGES={len(doc)}")
    doc.close()


def merge():
    if not MAIN_PDF.exists():
        raise FileNotFoundError(f"Main PDF does not exist: {MAIN_PDF}")
    out = fitz.open()
    main = fitz.open(MAIN_PDF)
    appendix = fitz.open(APPENDIX)
    out.insert_pdf(main)
    out.insert_pdf(appendix)
    out.set_metadata({
        "title": "Driver Moodle - Project Book With Source Code",
        "author": "Eli Rapoport",
        "subject": "Final project book and complete source appendix",
    })
    out.save(COMBINED, garbage=4, deflate=True)
    print(f"COMBINED={COMBINED}")
    print(f"MAIN_PAGES={len(main)}")
    print(f"APPENDIX_PAGES={len(appendix)}")
    print(f"TOTAL_PAGES={len(out)}")
    out.close()
    appendix.close()
    main.close()


if __name__ == "__main__":
    create_appendix()
    merge()
