using BusinessLogic;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using ViewDB;
using Model.Helpers;

namespace WcfServiceLibrary1
{
    public class Service1 : IService1
    {
        private ViewDB.UserDB userDB = new ViewDB.UserDB();
        private AllUsers allUsers = new AllUsers();
        private ChatDB chatDB = new ChatDB();
        private LessonsDB lessonsDB = new LessonsDB();
        private ViewDB.CalendarDB calendarDB = new ViewDB.CalendarDB();

        // ==================== USER OPERATIONS ====================

        public bool AddUser(string name, string password, string email, string phone, bool admin, int tID, int lessonPrice = 200)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== AddUser called ===");
                System.Diagnostics.Debug.WriteLine($"Username: {name}, Email: {email}, IsTeacher: {admin}, TeacherId: {tID}, LessonPrice: {lessonPrice}");

                if (!SecurityHelper.IsSafeString(name, 50))
                {
                    System.Diagnostics.Debug.WriteLine("AddUser: Username failed safety check");
                    return false;
                }

                if (!SecurityHelper.IsSafeString(email, 100))
                {
                    System.Diagnostics.Debug.WriteLine("AddUser: Email failed safety check");
                    return false;
                }

                if (string.IsNullOrEmpty(password))
                {
                    System.Diagnostics.Debug.WriteLine("AddUser: Password is empty");
                    return false;
                }

                if (CheckUserExist(name))
                {
                    System.Diagnostics.Debug.WriteLine("AddUser: User already exists");
                    return false;
                }

                UserInfo user = new UserInfo
                {
                    Username = name,
                    Password = password,
                    Email = email,
                    Phone = phone,
                    IsAdmin = admin,
                    TeacherId = tID,
                    LessonPrice = lessonPrice > 0 ? lessonPrice : 200
                };

                bool worked = false;

                if (admin)
                {
                    // Teacher registration
                    System.Diagnostics.Debug.WriteLine("AddUser: Registering as Teacher");
                    worked = userDB.AddUser(user);
                }
                else
                {
                    // Student registration
                    System.Diagnostics.Debug.WriteLine("AddUser: Registering as Student");
                    worked = userDB.AddStudent(user);
                    if (worked)
                    {
                        int sid = userDB.GetUserID(name, "Student");
                        allUsers.SetStudentId(name, sid);
                        System.Diagnostics.Debug.WriteLine($"AddUser: Student ID assigned: {sid}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"AddUser: Registration result = {worked}");
                return worked;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddUser Exception: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public bool CheckUserExist(string username)
        {
            if (!SecurityHelper.IsSafeString(username, 50))
                return false;

            allUsers = userDB.GetAllStudents();
            var allAdmins = userDB.GetAllTeacher();

            return allUsers.Any(x => x.Username == username) ||
                   allAdmins.Any(x => x.Username == username);
        }

        public bool CheckUserPassword(string username, string password)
        {
            if (!SecurityHelper.IsSafeString(username, 50) ||
                string.IsNullOrEmpty(password))
            {
                return false;
            }

            return userDB.VerifyUserPassword(username, password);
        }

        public UserInfo GetUserById(int id, string table)
        {
            return userDB.GetUserById(id, table);
        }

        public bool CheckUserAdmin(string username)
        {
            if (!SecurityHelper.IsSafeString(username, 50))
                return false;

            var allAdmins = userDB.GetAllTeacher();
            return allAdmins.Any(x => x.Username == username);
        }

        public bool IsUserAdmin(string username)
        {
            return userDB.IsUserAdmin(username);
        }

        public AllUsers GetAllUsers()
        {
            return userDB.GetAllStudents();
        }

        public AllUsers GetAllTeacher()
        {
            return userDB.GetAllTeacher();
        }

        public int GetUserID(string username, string table)
        {
            return userDB.GetUserID(username, table);
        }

        public void TeacherConfirm(int id, int tID)
        {
            userDB.TeacherConfirm(id, tID);
        }

        public List<UserInfo> GetTeacherStudents(int tid)
        {
            return userDB.GetTeacherStudents(tid);
        }

        public bool IsConfirmed(int id)
        {
            return userDB.IsConfirmed(id);
        }

        public int GetTeacherId(int studentId)
        {
            return userDB.GetTeacherId(studentId);
        }

        public void UpdateTeacherId(int sid, int tid)
        {
            userDB.UpdateTeacherId(sid, tid);
        }

        // ==================== ADMIN OPERATIONS ====================

        public void SetAdminStatus(int teacherId, bool isAdmin)
        {
            userDB.SetAdminStatus(teacherId, isAdmin);
        }

        public void ResetPassword(int userId, string table, string newPassword)
        {
            userDB.ResetPassword(userId, table, newPassword);
        }

        // ==================== PRICING OPERATIONS ====================

        public void UpdateLessonPrice(int teacherId, int price)
        {
            userDB.UpdateLessonPrice(teacherId, price);
        }

        public int GetStudentLessonPrice(int studentId)
        {
            return userDB.GetStudentLessonPrice(studentId);
        }

        public void UpdatePaymentMethods(int teacherId, string paymentMethods)
        {
            userDB.UpdatePaymentMethods(teacherId, paymentMethods);
        }

        // ==================== LESSON OPERATIONS ====================

        public void CancelLesson(int lessonId)
        {
            try
            {
                // Get lesson details before cancelling
                var lesson = lessonsDB.GetLessonById(lessonId);

                if (lesson != null)
                {
                    // Get student name
                    var student = userDB.GetUserById(lesson.StudentId, "Student");
                    string studentName = student?.Username ?? "Unknown";

                    // Cancel the lesson
                    lessonsDB.CancelLesson(lessonId);

                    // Send notification to teacher
                    new NotificationDB().SendLessonCancelledNotification(
                        lesson.StudentId,
                        studentName,
                        lesson.TeacherId,
                        lesson.Date,
                        lesson.Time
                    );
                }
                else
                {
                    lessonsDB.CancelLesson(lessonId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CancelLesson Error: {ex.Message}");
                lessonsDB.CancelLesson(lessonId);
            }
        }

        public void MarkLessonPaid(int id)
        {
            lessonsDB.MarkLessonPaid(id);
        }

        public void AddLessonForStudent(int sid, string Date, string time)
        {
            lessonsDB.AddLessonForStudent(sid, Date, time);
        }

        public List<Lessons> GetAllStudentLessons(int id)
        {
            return lessonsDB.GetAllStudentLessons(id);
        }

        public List<Lessons> GetAllTeacherLessons(int tid)
        {
            return lessonsDB.GetAllTeacherLessons(tid);
        }

        // ==================== RATING OPERATIONS ====================

        public void UpdateRating(int tid, int rating, string rewiew)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            userDB.UpdateRating(tid, rating, rewiew);
        }

        public List<string> GetTeacherReviews(int tid)
        {
            return userDB.GetTeacherReviews(tid);
        }

        // ==================== CALENDAR OPERATIONS ====================

        public bool SetTeacherCalendar(Calendars cal, int teacherId)
        {
            return calendarDB.SetTeacherCalendar(cal, teacherId);
        }

        public Calendars GetTeacherCalendar(int teacherId)
        {
            return calendarDB.GetTeacherCalendar(teacherId);
        }

        public List<Calendars> GetTeacherUnavailableDates(int teacherId)
        {
            return calendarDB.GetTeacherUnavailableDates(teacherId);
        }

        public List<Calendars> TeacherSpacialDays(int teacherId)
        {
            return calendarDB.TeacherSpacialDays(teacherId);
        }

        // ==================== CHAT OPERATIONS ====================

        public List<Chats> GetAllChatGlobal()
        {
            return chatDB.GetAllChatGlobal();
        }

        public void AddMessageGlobal(string message, int userid, string username, bool IsTeacher)
        {
            chatDB.AddMessageGlobal(message, userid, username, IsTeacher);
        }

        public List<Chats> GetChatPrivate(int studentid, int teacherid)
        {
            return chatDB.GetChatPrivate(studentid, teacherid);
        }

        public void AddMessagePrivate(string message, int studentid, int teacherid, string username)
        {
            chatDB.AddMessagePrivate(message, studentid, teacherid, username);
        }

        // ==================== PAYMENT OPERATIONS ====================

        public List<Payment> SelectPaymentByStudentID(int id)
        {
            return new PaymentDB().SelectPaymentByStudentID(id);
        }

        public List<Payment> SelectPaymentByTeacherID(int id)
        {
            return new PaymentDB().SelectPaymentByTeacherID(id);
        }

        public List<Payment> SelectPaymentByPaymentID(int id)
        {
            return new PaymentDB().SelectPaymentByPaymentID(id);
        }

        public void Pay(Payment payment)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Pay called ===");
                System.Diagnostics.Debug.WriteLine($"StudentID: {payment.StudentID}");
                System.Diagnostics.Debug.WriteLine($"TeacherID: {payment.TeacherID}");
                System.Diagnostics.Debug.WriteLine($"LessonId: {payment.LessonId}");
                System.Diagnostics.Debug.WriteLine($"Amount: {payment.Amount}");
                System.Diagnostics.Debug.WriteLine($"Paid: {payment.paid}");

                new PaymentDB().Pay(payment);

                // Send payment notification to teacher
                try
                {
                    var student = userDB.GetUserById(payment.StudentID, "Student");
                    string studentName = student?.Username ?? "Student";
                    new NotificationDB().SendPaymentNotification(
                        payment.StudentID,
                        studentName,
                        payment.TeacherID,
                        (int)payment.Amount,
                        payment.PaymentMethod ?? "Unknown"
                    );
                }
                catch (Exception notifEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Payment notification error: {notifEx.Message}");
                }

                System.Diagnostics.Debug.WriteLine("Payment completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Pay Error: {ex.Message}");
                throw;
            }
        }

        public bool CheckPaid(int id)
        {
            return new PaymentDB().CheckPaid(id);
        }

        public decimal GetTeacherIncome(int teacherId, DateTime fromDate, DateTime toDate)
        {
            return new PaymentDB().GetTeacherIncome(teacherId, fromDate, toDate);
        }

        public List<Payment> GetOutstandingPayments(int studentId)
        {
            return new PaymentDB().GetOutstandingPayments(studentId);
        }

        public List<Payment> GetOverduePayments()
        {
            return new PaymentDB().GetOverduePayments();
        }

        // ==================== SUPPORT TICKET OPERATIONS ====================

        public int CreateSupportTicket(SupportTicket ticket)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CreateSupportTicket Called ===");
                System.Diagnostics.Debug.WriteLine($"UserId: {ticket.UserId}");
                System.Diagnostics.Debug.WriteLine($"Username: {ticket.Username}");
                System.Diagnostics.Debug.WriteLine($"UserType: {ticket.UserType}");
                System.Diagnostics.Debug.WriteLine($"Subject: {ticket.Subject}");

                int result = new SupportTicketDB().CreateTicket(ticket);

                System.Diagnostics.Debug.WriteLine($"Ticket created with ID: {result}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateSupportTicket Error: {ex.Message}");
                throw;
            }
        }

        public List<SupportTicket> GetUserTickets(int userId)
        {
            return new SupportTicketDB().GetUserTickets(userId);
        }

        public List<SupportTicket> GetAllTickets()
        {
            return new SupportTicketDB().GetAllTickets();
        }

        public SupportTicket GetTicketById(int ticketId)
        {
            return new SupportTicketDB().GetTicketById(ticketId);
        }

        public void UpdateTicketStatus(int ticketId, string status, string assignedTo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== UpdateTicketStatus called ===");
                System.Diagnostics.Debug.WriteLine($"TicketId: {ticketId}, Status: {status}, AssignedTo: {assignedTo}");

                new SupportTicketDB().UpdateTicketStatus(ticketId, status, assignedTo);

                System.Diagnostics.Debug.WriteLine("UpdateTicketStatus completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTicketStatus Error: {ex.Message}");
                throw;
            }
        }

        public void CloseTicket(int ticketId, string resolution, string adminNotes)
        {
            new SupportTicketDB().CloseTicket(ticketId, resolution, adminNotes);
        }

        public void AddTicketMessage(TicketMessage message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== AddTicketMessage called ===");
                System.Diagnostics.Debug.WriteLine($"TicketId: {message.TicketId}");
                System.Diagnostics.Debug.WriteLine($"Sender: {message.SenderUsername}");
                System.Diagnostics.Debug.WriteLine($"IsAdmin: {message.IsAdmin}");
                System.Diagnostics.Debug.WriteLine($"Message: {message.Message}");

                new SupportTicketDB().AddTicketMessage(message);

                System.Diagnostics.Debug.WriteLine("AddTicketMessage completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddTicketMessage Error: {ex.Message}");
                throw;
            }
        }

        public List<TicketMessage> GetTicketMessages(int ticketId)
        {
            return new SupportTicketDB().GetTicketMessages(ticketId);
        }

        public void UpdateTicketPriority(int ticketId, string priority)
        {
            new SupportTicketDB().UpdateTicketPriority(ticketId, priority);
        }

        // ==================== MIGRATION ====================

        public void MigrateAllPasswords()
        {
            userDB.MigrateAllPasswords();
        }

        // ==================== STUDENT PRICING OPERATIONS ====================

        public void SetStudentLessonPrice(int studentId, int price)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"SetStudentLessonPrice: StudentId={studentId}, Price={price}");
                userDB.SetStudentLessonPrice(studentId, price);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetStudentLessonPrice Error: {ex.Message}");
                throw;
            }
        }

        public void SetStudentDiscount(int studentId, int discountPercent)
        {
            try
            {
                if (discountPercent < 0 || discountPercent > 100)
                    throw new ArgumentException("Discount must be between 0 and 100");

                System.Diagnostics.Debug.WriteLine($"SetStudentDiscount: StudentId={studentId}, Discount={discountPercent}%");
                userDB.SetStudentDiscount(studentId, discountPercent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetStudentDiscount Error: {ex.Message}");
                throw;
            }
        }

        public int GetEffectiveLessonPrice(int studentId)
        {
            try
            {
                return userDB.GetStudentLessonPrice(studentId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetEffectiveLessonPrice Error: {ex.Message}");
                return 200; // Default fallback
            }
        }

        public void UpdateStudentCredentials(int studentId, string email, string phone, int teacherId)
        {
            userDB.UpdateStudentCredentials(studentId, email, phone, teacherId);
        }

        public void UpdateStudentTeacher(int studentId, int newTeacherId)
        {
            userDB.UpdateStudentTeacher(studentId, newTeacherId);
        }

        // ==================== COURSE/LEARNING OPERATIONS ====================

        public List<Course> GetAllCourses()
        {
            return new CourseDB().GetActiveCourses();
        }

        public List<CourseModule> GetCourseModules(int courseId)
        {
            return new CourseModuleDB().GetModulesForCourse(courseId);
        }

        public List<StudentCourseProgress> GetStudentCourseProgress(int studentId)
        {
            return new CourseModuleDB().GetStudentCourseProgress(studentId);
        }

        public bool MarkModuleComplete(int studentId, int moduleId)
        {
            return new CourseModuleDB().MarkModuleComplete(studentId, moduleId);
        }

        public List<StudentModuleProgress> GetStudentCompletedModules(int studentId)
        {
            return new CourseModuleDB().GetStudentCompletedModules(studentId);
        }

        // ==================== COURSE MANAGEMENT OPERATIONS ====================

        public int AddCourse(Course course)
        {
            return new CourseDB().AddCourse(course);
        }

        public int UpdateCourse(Course course)
        {
            return new CourseDB().UpdateCourse(course);
        }

        public int DeactivateCourse(int courseId)
        {
            return new CourseDB().DeactivateCourse(courseId);
        }

        public int AddModule(CourseModule module)
        {
            return new CourseModuleDB().AddModule(module);
        }

        public int UpdateModule(CourseModule module)
        {
            return new CourseModuleDB().UpdateModule(module);
        }

        public int DeleteModule(int moduleId)
        {
            return new CourseModuleDB().DeleteModule(moduleId);
        }

        public Course GetCourseById(int courseId)
        {
            return new CourseDB().GetCourseById(courseId);
        }

        // ==================== NOTIFICATION OPERATIONS ====================

        public int SendNotification(Notification notification)
        {
            return new NotificationDB().SendNotification(notification);
        }

        public List<Notification> GetUserNotifications(int userId, string userType)
        {
            return new NotificationDB().GetUserNotifications(userId, userType);
        }

        public int GetUnreadNotificationCount(int userId, string userType)
        {
            return new NotificationDB().GetUnreadCount(userId, userType);
        }

        public List<Notification> GetUnreadNotifications(int userId, string userType)
        {
            return new NotificationDB().GetUnreadNotifications(userId, userType);
        }

        public void MarkNotificationAsRead(int notificationId)
        {
            new NotificationDB().MarkAsRead(notificationId);
        }

        public void MarkAllNotificationsAsRead(int userId, string userType)
        {
            new NotificationDB().MarkAllAsRead(userId, userType);
        }

        public void DeleteNotification(int notificationId)
        {
            new NotificationDB().DeleteNotification(notificationId);
        }

        public void SendTeacherMessage(int teacherId, string teacherName, int studentId, string title, string message)
        {
            new NotificationDB().SendTeacherMessage(teacherId, teacherName, studentId, title, message);
        }

        public void SendStudentMessage(int studentId, string studentName, int teacherId, string title, string message)
        {
            new NotificationDB().SendStudentMessage(studentId, studentName, teacherId, title, message);
        }

        public void SendLessonCancelledNotification(int studentId, string studentName, int teacherId, string lessonDate, string lessonTime)
        {
            new NotificationDB().SendLessonCancelledNotification(studentId, studentName, teacherId, lessonDate, lessonTime);
        }
    }
}