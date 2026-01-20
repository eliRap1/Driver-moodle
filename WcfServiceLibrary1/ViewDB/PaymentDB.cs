using Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace ViewDB
{
    public class Payment : Base
    {
        public int PaymentID { get; set; }
        public int StudentID { get; set; }
        public int TeacherID { get; set; }
        public int Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public int NumberOfPayments { get; set; }
        public int ParcialAmount { get; set; }
        public bool paid { get; set; }
        public int LessonId { get; set; }
        public string Status { get; set; } // Pending, Paid, Overdue, Cancelled
        public string Notes { get; set; }
    }

    public class PaymentDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new Payment();
        }

        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    Payment s = (Payment)entity;
                    s.PaymentID = int.Parse(reader["PaymentID"].ToString());
                    s.StudentID = int.Parse(reader["StudentID"].ToString());
                    s.TeacherID = int.Parse(reader["TeacherID"].ToString());
                    s.Amount = int.Parse(reader["Amount"].ToString());
                    s.PaymentDate = DateTime.Parse(reader["PaymentDate"].ToString());
                    s.PaymentMethod = reader["PaymentMethod"].ToString();
                    s.NumberOfPayments = int.Parse(reader["NumberOfPayments"].ToString());
                    s.paid = bool.Parse(reader["paid"].ToString());

                    // Optional fields
                    try
                    {
                        s.LessonId = int.Parse(reader["LessonId"].ToString());
                    }
                    catch { }

                    try
                    {
                        s.Status = reader["Status"].ToString();
                    }
                    catch { s.Status = s.paid ? "Paid" : "Pending"; }

                    try
                    {
                        s.Notes = reader["Notes"].ToString();
                    }
                    catch { }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("CreateModel Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// SECURE: Get payments by student ID
        /// </summary>
        public List<Payment> SelectPaymentByStudentID(int id)
        {
            string sqlStr = "SELECT * FROM [Payments] WHERE StudentID = ?";
            return Select(sqlStr, new OleDbParameter("@studentId", id))
                .OfType<Payment>()
                .ToList();
        }

        /// <summary>
        /// SECURE: Get payments by teacher ID
        /// </summary>
        public List<Payment> SelectPaymentByTeacherID(int id)
        {
            string sqlStr = "SELECT * FROM [Payments] WHERE TeacherID = ?";
            return Select(sqlStr, new OleDbParameter("@teacherId", id))
                .OfType<Payment>()
                .ToList();
        }

        /// <summary>
        /// SECURE: Get payment by payment ID
        /// </summary>
        public List<Payment> SelectPaymentByPaymentID(int id)
        {
            string sqlStr = "SELECT * FROM [Payments] WHERE PaymentID = ?";
            return Select(sqlStr, new OleDbParameter("@paymentId", id))
                .OfType<Payment>()
                .ToList();
        }

        /// <summary>
        /// SECURE: Get payments by lesson ID
        /// </summary>
        public List<Payment> GetPaymentsByLessonId(int lessonId)
        {
            string sqlStr = "SELECT * FROM [Payments] WHERE LessonId = ?";
            return Select(sqlStr, new OleDbParameter("@lessonId", lessonId))
                .OfType<Payment>()
                .ToList();
        }

        /// <summary>
        /// SECURE: Update payment status
        /// </summary>
        public void PaymentUpdate(Payment payment)
        {
            string sqlStr = "UPDATE [Payments] SET paid = ? WHERE PaymentID = ?";
            SaveChanges(sqlStr,
                new OleDbParameter("@paid", payment.paid),
                new OleDbParameter("@paymentId", payment.PaymentID));
        }

        /// <summary>
        /// SECURE: Process payment
        /// </summary>
        public void Pay(Payment payment)
        {
            // Calculate partial amount if installments
            if (payment.NumberOfPayments > 0)
            {
                payment.ParcialAmount = payment.Amount / payment.NumberOfPayments;
            }

            // Mark lesson as paid if payment is completed
            if (payment.paid && payment.LessonId > 0)
            {
                string sql1 = "UPDATE [Lessons] SET Paid = ? WHERE LessonId = ?";
                SaveChanges(sql1,
                    new OleDbParameter("@paid", true),
                    new OleDbParameter("@lessonId", payment.LessonId));
            }

            // Insert payment record
            string sql = @"INSERT INTO [Payments] 
                (PaymentID, StudentID, TeacherID, Amount, PaymentDate, PaymentMethod, 
                 NumberOfPayments, paid, ParcialAmount, LessonId, Status, Notes)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            SaveChanges(sql,
                new OleDbParameter("@paymentId", payment.PaymentID),
                new OleDbParameter("@studentId", payment.StudentID),
                new OleDbParameter("@teacherId", payment.TeacherID),
                new OleDbParameter("@amount", payment.Amount),
                new OleDbParameter("@paymentDate", payment.PaymentDate),
                new OleDbParameter("@paymentMethod", payment.PaymentMethod),
                new OleDbParameter("@numPayments", payment.NumberOfPayments),
                new OleDbParameter("@paid", payment.paid),
                new OleDbParameter("@parcialAmount", payment.ParcialAmount),
                new OleDbParameter("@lessonId", payment.LessonId),
                new OleDbParameter("@status", payment.Status ?? (payment.paid ? "Paid" : "Pending")),
                new OleDbParameter("@notes", payment.Notes ?? ""));
        }

        /// <summary>
        /// SECURE: Check if payment is completed
        /// </summary>
        public bool CheckPaid(int id)
        {
            string sqlStr = "SELECT * FROM [Payments] WHERE PaymentID = ?";
            List<Payment> list = Select(sqlStr, new OleDbParameter("@paymentId", id))
                .OfType<Payment>()
                .ToList();

            if (list.Count > 0)
                return list[0].paid;

            return false;
        }

        /// <summary>
        /// Get total income for a teacher in a date range
        /// </summary>
        public decimal GetTeacherIncome(int teacherId, DateTime fromDate, DateTime toDate)
        {
            string sql = @"SELECT SUM(Amount) FROM [Payments] 
                          WHERE TeacherID = ? AND paid = ? 
                          AND PaymentDate >= ? AND PaymentDate <= ?";

            object result = SelectScalar(sql,
                new OleDbParameter("@teacherId", teacherId),
                new OleDbParameter("@paid", true),
                new OleDbParameter("@fromDate", fromDate),
                new OleDbParameter("@toDate", toDate));

            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        /// <summary>
        /// Get outstanding payments for a student
        /// </summary>
        public List<Payment> GetOutstandingPayments(int studentId)
        {
            string sql = "SELECT * FROM [Payments] WHERE StudentID = ? AND paid = ? ORDER BY PaymentDate";
            return Select(sql,
                new OleDbParameter("@studentId", studentId),
                new OleDbParameter("@paid", false))
                .OfType<Payment>()
                .ToList();
        }

        /// <summary>
        /// Get overdue payments
        /// </summary>
        public List<Payment> GetOverduePayments()
        {
            string sql = @"SELECT * FROM [Payments] 
                          WHERE paid = ? AND PaymentDate < ? 
                          ORDER BY PaymentDate";

            return Select(sql,
                new OleDbParameter("@paid", false),
                new OleDbParameter("@today", DateTime.Today))
                .OfType<Payment>()
                .ToList();
        }

        /// <summary>
        /// Mark payment as overdue
        /// </summary>
        public void MarkAsOverdue(int paymentId)
        {
            string sql = "UPDATE [Payments] SET Status = ? WHERE PaymentID = ?";
            SaveChanges(sql,
                new OleDbParameter("@status", "Overdue"),
                new OleDbParameter("@paymentId", paymentId));
        }

        /// <summary>
        /// Cancel a payment
        /// </summary>
        public void CancelPayment(int paymentId, string reason)
        {
            string sql = "UPDATE [Payments] SET Status = ?, Notes = ? WHERE PaymentID = ?";
            SaveChanges(sql,
                new OleDbParameter("@status", "Cancelled"),
                new OleDbParameter("@notes", reason),
                new OleDbParameter("@paymentId", paymentId));
        }
    }
}