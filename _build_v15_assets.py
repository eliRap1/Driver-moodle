from pathlib import Path
from shutil import copy2

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parent
OUT = ROOT / "_generated_assets_v15"
OUT.mkdir(exist_ok=True)

BLUE = "#1F4E79"
LIGHT_BLUE = "#EAF3F8"
TEAL = "#2D8C9E"
GREEN = "#2F7D5A"
ORANGE = "#B8741A"
GRAY = "#5B6573"
LIGHT_GRAY = "#F5F7F9"
DARK = "#1D2935"
RED = "#B04A4A"
WHITE = "#FFFFFF"


def font(size, bold=False, mono=False):
    if mono:
        path = Path(r"C:\Windows\Fonts\consola.ttf")
    elif bold:
        path = Path(r"C:\Windows\Fonts\arialbd.ttf")
    else:
        path = Path(r"C:\Windows\Fonts\arial.ttf")
    return ImageFont.truetype(str(path), size)


def arrow(draw, start, end, color=GRAY, width=4, dashed=False):
    x1, y1 = start
    x2, y2 = end
    if dashed:
        steps = 18
        for i in range(0, steps, 2):
            a = i / steps
            b = min((i + 1) / steps, 1)
            draw.line((x1 + (x2 - x1) * a, y1 + (y2 - y1) * a,
                       x1 + (x2 - x1) * b, y1 + (y2 - y1) * b),
                      fill=color, width=width)
    else:
        draw.line((x1, y1, x2, y2), fill=color, width=width)
    import math
    angle = math.atan2(y2 - y1, x2 - x1)
    size = 16
    pts = [
        (x2, y2),
        (x2 - size * math.cos(angle - 0.55), y2 - size * math.sin(angle - 0.55)),
        (x2 - size * math.cos(angle + 0.55), y2 - size * math.sin(angle + 0.55)),
    ]
    draw.polygon(pts, fill=color)


def title(draw, text, subtitle=None):
    draw.text((80, 70), text, font=font(52, bold=True), fill=BLUE)
    if subtitle:
        draw.text((82, 140), subtitle, font=font(26), fill=GRAY)
    draw.line((80, 190, 1520, 190), fill="#C8D5DE", width=3)


def rounded_box(draw, xy, heading, lines=None, fill=WHITE, outline=BLUE,
                heading_fill=LIGHT_BLUE, center=False, width=3):
    x1, y1, x2, y2 = xy
    draw.rounded_rectangle(xy, radius=16, fill=fill, outline=outline, width=width)
    draw.rounded_rectangle((x1, y1, x2, min(y1 + 62, y2)), radius=16,
                           fill=heading_fill, outline=outline, width=width)
    draw.rectangle((x1, y1 + 46, x2, min(y1 + 62, y2)), fill=heading_fill)
    draw.text(((x1 + x2) // 2 if center else x1 + 18, y1 + 14), heading,
              font=font(27, bold=True), fill=DARK,
              anchor="ma" if center else None)
    if lines:
        y = y1 + 82
        for item in lines:
            draw.text((x1 + 20, y), item, font=font(22), fill=DARK)
            y += 31


def save(img, name):
    path = OUT / name
    img.save(path, optimize=True)
    print(path)


def copy_cover():
    generated = Path(
        r"C:\Users\eli08\.codex\generated_images"
        r"\019e2034-0815-7702-89fc-0595b4447919"
        r"\ig_05c065ab1eb00e96016a1b500719988191ae8fc1053dc728f3.png"
    )
    copy2(generated, OUT / "driver_moodle_cover_visual.png")
    copy2(ROOT / "_generated_assets_v12" / "makifz_logo.png", OUT / "makifz_logo.png")


def architecture():
    img = Image.new("RGB", (1600, 2200), WHITE)
    d = ImageDraw.Draw(img)
    title(d, "Driver Moodle - System Architecture",
          "A clear view of clients, network service, application layers and database")

    rounded_box(d, (250, 270, 1350, 455), "Clients", center=True, heading_fill="#DDEFF3")
    client_boxes = [
        (320, 350, 610, 425, "WPF desktop"),
        (655, 350, 945, 425, "Web / Razor Pages"),
        (990, 350, 1280, 425, "MAUI / Android"),
    ]
    for x1, y1, x2, y2, label in client_boxes:
        d.rounded_rectangle((x1, y1, x2, y2), radius=14, fill=WHITE, outline=TEAL, width=3)
        d.text(((x1 + x2) / 2, (y1 + y2) / 2), label, font=font(23, bold=True),
               fill=DARK, anchor="mm")

    rounded_box(d, (350, 610, 1250, 810), "WCF network service",
                ["IService1 - service contract", "Service1 - implementation of the operations"],
                fill=WHITE, outline=BLUE, heading_fill="#DBE9F4", center=True)
    arrow(d, (800, 455), (800, 610), TEAL, 5)

    rounded_box(d, (210, 1010, 720, 1230), "BusinessLogic",
                ["Application rules", "List processing", "Supporting algorithms"],
                fill=WHITE, outline=ORANGE, heading_fill="#F9ECD8")
    rounded_box(d, (880, 1010, 1390, 1230), "ViewDB",
                ["BaseDB connection", "SELECT and SaveChanges", "Entity-specific DB classes"],
                fill=WHITE, outline=GREEN, heading_fill="#E5F3EB")
    arrow(d, (660, 810), (470, 1010), ORANGE, 5)
    arrow(d, (940, 810), (1135, 1010), GREEN, 5)

    rounded_box(d, (350, 1440, 1250, 1650), "Access database",
                ["UsersDataBase.accdb", "Tables, keys and relationships", "Stored lesson, payment and notification data"],
                fill=WHITE, outline=BLUE, heading_fill="#DBE9F4", center=True)
    arrow(d, (1135, 1230), (960, 1440), GREEN, 5)

    d.text((155, 1840), "Main request path", font=font(27, bold=True), fill=BLUE)
    d.text((155, 1900),
           "Client -> IService1 -> Service1 -> ViewDB -> Access database",
           font=font(28), fill=DARK)
    d.text((155, 1980),
           "The clients do not connect directly to the database.",
           font=font(25), fill=GRAY)
    save(img, "architecture_portrait.png")


def solution_tree():
    img = Image.new("RGB", (1600, 2200), WHITE)
    d = ImageDraw.Draw(img)
    title(d, "Driver Moodle - Solution Structure",
          "Readable project tree based on the real folders in the repository")
    rows = [
        (0, "Driver-moodle", "root"),
        (1, "WcfServiceLibrary1.sln", "solution"),
        (2, "Model", "project"),
        (3, "Base.cs, UserInfo.cs, Course.cs, Notification.cs ...", "files"),
        (2, "ViewDB", "project"),
        (3, "BaseDB.cs, UserDB.cs, LessonsDB.cs, PaymentDB.cs ...", "files"),
        (2, "BusinessLogic", "project"),
        (3, "AllUsersListAndLofics.cs, AllStudentsListAndLogics.cs ...", "files"),
        (2, "WcfServiceLibrary1", "project"),
        (3, "IService1.cs, Service1.cs, App.config", "files"),
        (1, "driver-client.sln", "solution"),
        (2, "driver-client", "project"),
        (3, "WPF screens: StudentUI, TeacherUI, ScheduleLesson ...", "files"),
        (1, "Driver / Driver.csproj", "solution"),
        (2, "Razor Pages web client", "project"),
        (3, "Student, Teacher and Admin pages", "files"),
        (1, "driver-maui.sln", "solution"),
        (2, "driver-maui", "project"),
        (3, "MAUI pages for Android and desktop", "files"),
    ]
    colors = {"root": BLUE, "solution": TEAL, "project": GREEN, "files": GRAY}
    y = 285
    for level, text, kind in rows:
        x = 130 + level * 105
        if level:
            d.line((x - 60, y + 18, x - 18, y + 18), fill="#C3CDD4", width=3)
        if kind in ("root", "solution", "project"):
            d.rounded_rectangle((x, y, x + 36, y + 36), radius=5,
                                fill=colors[kind], outline=colors[kind])
        else:
            d.rectangle((x + 8, y + 7, x + 30, y + 29), fill="#DDE4E8", outline=GRAY)
        d.text((x + 58, y + 3), text, font=font(27, bold=(kind != "files")),
               fill=DARK if kind != "files" else GRAY)
        y += 87
    d.rounded_rectangle((140, 1950, 1460, 2070), radius=16, fill="#F3F7FA",
                        outline="#C6D4DE", width=2)
    d.text((175, 1980),
           "Each server project has a separate responsibility. The three clients use the same WCF service.",
           font=font(24), fill=DARK)
    save(img, "solution_structure_portrait.png")


def db_box(draw, x, y, w, name, fields, color=BLUE):
    h = 65 + 31 * len(fields) + 15
    rounded_box(draw, (x, y, x + w, y + h), name, fields, outline=color,
                heading_fill="#EDF4F8", width=3)
    return (x, y, x + w, y + h)


def db_core():
    img = Image.new("RGB", (1600, 2400), WHITE)
    d = ImageDraw.Draw(img)
    title(d, "Access Relationships - Core Tables",
          "Solid lines represent relationships stored in the Access database")
    boxes = {}
    boxes["Teacher"] = db_box(d, 600, 270, 390, "Teacher",
                              ["PK  id", "username", "email", "lessonPrice", "isAdmin"], TEAL)
    boxes["Student"] = db_box(d, 600, 650, 390, "Student",
                              ["PK  id", "FK  TeacherId", "username", "Confirmed", "lessonPrice"], TEAL)
    boxes["Lessons"] = db_box(d, 600, 1070, 390, "Lessons",
                              ["PK  LessonID", "FK  StudentID", "FK  TeacherID", "Date", "Time", "paid"], TEAL)
    boxes["Payments"] = db_box(d, 600, 1540, 390, "Payments",
                               ["PK  PaymentID", "FK  StudentID", "LessonId", "Amount", "Paid"], TEAL)
    boxes["Availability"] = db_box(d, 90, 320, 390, "Availability",
                                   ["PK  AvailabilityID", "FK  TeacherID", "availableDays", "startTime", "endTime"], GREEN)
    boxes["Ratings"] = db_box(d, 1100, 320, 390, "Ratings",
                              ["PK  rID", "FK  TeacherID", "Rating", "Rewiew"], GREEN)
    boxes["TeacherSpacialDays"] = db_box(d, 80, 700, 420, "TeacherSpacialDays",
                                        ["PK  ID", "FK  TeacherID", "SelectedDate", "startTime", "endTime"], GREEN)
    boxes["TeacherUnavailableDate"] = db_box(d, 1080, 700, 450, "TeacherUnavailableDate",
                                             ["PK  ID", "FK  TeacherID", "UnavailableDate", "AllDay"], GREEN)
    boxes["PrivateChat"] = db_box(d, 90, 1150, 390, "PrivateChat",
                                  ["PK  ID", "FK  TeacherID", "FK  StudentID", "Message", "SentAt"], ORANGE)
    boxes["SupportTickets"] = db_box(d, 90, 1710, 420, "SupportTickets",
                                     ["PK  TicketId", "UserId", "UserType", "Subject", "Status"], ORANGE)
    boxes["TicketMessages"] = db_box(d, 1080, 1710, 430, "TicketMessages",
                                     ["PK  MessageId", "FK  TicketId", "SenderUsername", "Message", "SentAt"], ORANGE)

    def edge(a, b, color="#667784"):
        ax1, ay1, ax2, ay2 = boxes[a]
        bx1, by1, bx2, by2 = boxes[b]
        start = ((ax1 + ax2) // 2, ay2)
        end = ((bx1 + bx2) // 2, by1)
        if ay1 > by1:
            start = ((ax1 + ax2) // 2, ay1)
            end = ((bx1 + bx2) // 2, by2)
        arrow(d, start, end, color, 3)

    edge("Teacher", "Student")
    edge("Teacher", "Availability")
    edge("Teacher", "Ratings")
    edge("Teacher", "TeacherSpacialDays")
    edge("Teacher", "TeacherUnavailableDate")
    edge("Teacher", "PrivateChat")
    edge("Student", "PrivateChat")
    edge("Student", "Lessons")
    edge("Teacher", "Lessons")
    edge("Student", "Payments")
    edge("Lessons", "Payments")
    edge("SupportTickets", "TicketMessages")

    d.text((100, 2265), "PK = primary key     FK = foreign key", font=font(25), fill=GRAY)
    save(img, "db_relationships_core_portrait.png")


def db_extended():
    img = Image.new("RGB", (1600, 2200), WHITE)
    d = ImageDraw.Draw(img)
    title(d, "Database Tables - Additional Logical Connections",
          "Dashed lines are identifiers managed by the service code")
    boxes = {}
    boxes["Student"] = db_box(d, 600, 260, 390, "Student", ["PK  id", "username", "TeacherId"], TEAL)
    boxes["Teacher"] = db_box(d, 80, 260, 390, "Teacher", ["PK  id", "username", "isAdmin"], TEAL)
    boxes["Notifications"] = db_box(d, 1080, 260, 430, "Notifications",
                                    ["PK  id", "SenderId", "SenderType", "RecipientId", "RecipientType"], ORANGE)
    boxes["GlobalChat"] = db_box(d, 80, 720, 390, "GlobalChat",
                                 ["PK  id", "UserID", "username", "Message"], ORANGE)
    boxes["Courses"] = db_box(d, 600, 720, 390, "Courses",
                              ["PK  Id", "TeacherId", "Title", "IsActive"], GREEN)
    boxes["CourseAccess"] = db_box(d, 1080, 720, 430, "CourseAccess",
                                   ["PK  Id", "CourseId", "StudentId", "GrantedAt"], GREEN)
    boxes["CourseContent"] = db_box(d, 260, 1250, 430, "CourseContent",
                                    ["PK  Id", "CourseId", "Title", "ContentType"], GREEN)
    boxes["CourseProgress"] = db_box(d, 860, 1250, 450, "CourseProgress",
                                     ["PK  Id", "CourseId", "StudentId", "ContentId", "IsCompleted"], GREEN)

    def dashed(a, b, color=ORANGE):
        ax1, ay1, ax2, ay2 = boxes[a]
        bx1, by1, bx2, by2 = boxes[b]
        arrow(d, ((ax1 + ax2) // 2, ay2), ((bx1 + bx2) // 2, by1), color, 3, dashed=True)

    dashed("Teacher", "Notifications")
    dashed("Student", "Notifications")
    dashed("Student", "GlobalChat")
    dashed("Teacher", "GlobalChat")
    dashed("Teacher", "Courses", GREEN)
    dashed("Student", "CourseAccess", GREEN)
    dashed("Courses", "CourseAccess", GREEN)
    dashed("Courses", "CourseContent", GREEN)
    dashed("CourseContent", "CourseProgress", GREEN)
    dashed("Student", "CourseProgress", GREEN)
    d.rounded_rectangle((140, 1860, 1460, 2040), radius=16, fill="#FFF8E8",
                        outline="#D8C58E", width=2)
    d.text((180, 1900), "Why dashed lines?", font=font(27, bold=True), fill=ORANGE)
    d.text((180, 1950),
           "These tables contain matching IDs, but the repository currently manages",
           font=font(24), fill=DARK)
    d.text((180, 1990),
           "their connection in service code instead of an Access foreign-key rule.",
           font=font(24), fill=DARK)
    save(img, "db_relationships_logical_portrait.png")


def uml():
    img = Image.new("RGB", (1600, 2400), WHITE)
    d = ImageDraw.Draw(img)
    title(d, "UML Class Diagram - Main Classes",
          "Inheritance and implementation relationships used in the project")
    boxes = {}
    boxes["Base"] = db_box(d, 610, 260, 380, "Base", ["+ Id : int"], BLUE)
    level2 = [
        ("UserInfo", 90, 640, ["+ Username : string", "+ Email : string", "+ TeacherId : int"]),
        ("Course", 470, 640, ["+ CourseName : string", "+ IsActive : bool"]),
        ("Notification", 850, 640, ["+ RecipientId : int", "+ Message : string", "+ IsRead : bool"]),
        ("SupportTicket", 1230, 640, ["+ TicketId : int", "+ Subject : string", "+ Status : string"]),
    ]
    for name, x, y, lines in level2:
        boxes[name] = db_box(d, x, y, 300, name, lines, TEAL)
        arrow(d, (x + 150, y), (800, 365), BLUE, 3)
    boxes["Calendars"] = db_box(d, 180, 1050, 330, "Calendars", ["+ SpecialDaysList", "+ UnavailableDays"], TEAL)
    boxes["CourseModule"] = db_box(d, 635, 1050, 330, "CourseModule", ["+ CourseId : int", "+ ModuleName : string"], TEAL)
    boxes["StudentModuleProgress"] = db_box(d, 1080, 1050, 390, "StudentModuleProgress",
                                            ["+ StudentId : int", "+ ModuleId : int", "+ IsCompleted : bool"], TEAL)
    for name in ("Calendars", "CourseModule", "StudentModuleProgress"):
        x1, y1, x2, y2 = boxes[name]
        arrow(d, ((x1 + x2) // 2, y1), (800, 365), BLUE, 3)

    boxes["IService1"] = db_box(d, 170, 1530, 430, "IService1 <<interface>>",
                                ["+ CheckUserPassword(...)", "+ AddLessonForStudent(...)", "+ GetUserNotifications(...)"], ORANGE)
    boxes["Service1"] = db_box(d, 930, 1530, 430, "Service1",
                               ["implements IService1", "uses ViewDB classes"], ORANGE)
    arrow(d, (930, 1630), (600, 1630), ORANGE, 4, dashed=True)

    boxes["BaseDB"] = db_box(d, 590, 1930, 420, "BaseDB",
                             ["+ GetConnection()", "# Select(...)", "# SaveChanges(...)"], GREEN)
    d.text((160, 2260),
           "Examples of BaseDB subclasses: UserDB, LessonsDB, PaymentDB, NotificationDB, SupportTicketDB",
           font=font(26), fill=DARK)
    arrow(d, (1145, 1740), (800, 1930), GREEN, 4, dashed=True)
    save(img, "uml_classes_portrait.png")


def usecase():
    img = Image.new("RGB", (1600, 2100), WHITE)
    d = ImageDraw.Draw(img)
    title(d, "Driver Moodle - Use Cases",
          "The main actions are separated by the role of the user")
    roles = [
        ("Student", 120, TEAL, ["Register and sign in", "Schedule a lesson", "View lessons", "Pay and view notifications", "Open a support ticket"]),
        ("Teacher", 605, GREEN, ["Sign in", "Manage schedule", "View students", "Confirm payments", "Send notifications", "View payment reports"]),
        ("Administrator", 1090, ORANGE, ["Sign in", "Manage users", "Handle support tickets", "Review system activity"]),
    ]
    for role, x, color, actions in roles:
        d.ellipse((x + 130, 290, x + 230, 390), outline=color, width=5)
        d.line((x + 180, 390, x + 180, 560), fill=color, width=5)
        d.line((x + 95, 455, x + 265, 455), fill=color, width=5)
        d.line((x + 180, 560, x + 110, 690), fill=color, width=5)
        d.line((x + 180, 560, x + 250, 690), fill=color, width=5)
        d.text((x + 180, 760), role, font=font(31, bold=True), fill=color, anchor="mm")
        y = 860
        for action in actions:
            d.rounded_rectangle((x, y, x + 360, y + 82), radius=18, fill="#F8FAFB", outline=color, width=3)
            d.text((x + 180, y + 41), action, font=font(23), fill=DARK, anchor="mm")
            y += 120
    save(img, "usecase_roles_portrait.png")


def sequence():
    img = Image.new("RGB", (1600, 2050), WHITE)
    d = ImageDraw.Draw(img)
    title(d, "Sequence Diagram - Schedule a Lesson",
          "Example of a complete request from a client screen to the Access database")
    actors = [
        ("Student client", 180),
        ("IService1", 520),
        ("Service1", 820),
        ("LessonsDB", 1120),
        ("Access DB", 1420),
    ]
    for name, x in actors:
        d.rounded_rectangle((x - 120, 290, x + 120, 360), radius=12, fill="#EDF4F8", outline=BLUE, width=3)
        d.text((x, 325), name, font=font(23, bold=True), fill=DARK, anchor="mm")
        d.line((x, 360, x, 1810), fill="#B8C3CB", width=3)
    events = [
        (180, 520, 470, "AddLessonForStudent(sid, date, time)"),
        (520, 820, 590, "Forward operation"),
        (820, 1120, 690, "Validate student and teacher"),
        (1120, 1420, 800, "INSERT INTO Lessons (...)"),
        (1420, 1120, 920, "Lesson saved"),
        (1120, 820, 1020, "Return success"),
        (820, 520, 1120, "Return success"),
        (520, 180, 1220, "Refresh lesson list"),
    ]
    for x1, x2, y, label in events:
        arrow(d, (x1, y), (x2, y), TEAL if x2 > x1 else GREEN, 3)
        d.text(((x1 + x2) / 2, y - 38), label, font=font(20), fill=DARK, anchor="mm")
    d.rounded_rectangle((150, 1550, 1450, 1750), radius=16, fill="#F3F7FA", outline="#C6D4DE", width=2)
    d.text((190, 1590), "What this diagram shows", font=font(27, bold=True), fill=BLUE)
    d.text((190, 1650), "The client sends a service request. The server validates the data,", font=font(24), fill=DARK)
    d.text((190, 1690), "and only the ViewDB layer writes the lesson into the database.", font=font(24), fill=DARK)
    save(img, "sequence_schedule_lesson_portrait.png")


if __name__ == "__main__":
    copy_cover()
    architecture()
    solution_tree()
    db_core()
    db_extended()
    uml()
    usecase()
    sequence()
