using Model;
using System;
using System.Collections.Generic;
using ViewDB;

namespace BusinessLogic
{
    /// <summary>
    /// Business logic for payments: recording a payment (with teacher notification),
    /// income reporting, and outstanding/overdue queries.
    /// </summary>
    public static class PaymentLogic
    {
        public static List<Payment> SelectPaymentByStudentID(int id)
        {
            return new PaymentDB().SelectPaymentByStudentID(id);
        }

        public static List<Payment> SelectPaymentByTeacherID(int id)
        {
            return new PaymentDB().SelectPaymentByTeacherID(id);
        }

        public static List<Payment> SelectPaymentByPaymentID(int id)
        {
            return new PaymentDB().SelectPaymentByPaymentID(id);
        }

        public static void Pay(Payment payment)
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
                    var student = new UserDB().GetUserById(payment.StudentID, "Student");
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

        public static bool CheckPaid(int id)
        {
            return new PaymentDB().CheckPaid(id);
        }

        public static decimal GetTeacherIncome(int teacherId, DateTime fromDate, DateTime toDate)
        {
            return new PaymentDB().GetTeacherIncome(teacherId, fromDate, toDate);
        }

        public static List<Payment> GetOutstandingPayments(int studentId)
        {
            return new PaymentDB().GetOutstandingPayments(studentId);
        }

        public static List<Payment> GetOverduePayments()
        {
            return new PaymentDB().GetOverduePayments();
        }
    }
}
