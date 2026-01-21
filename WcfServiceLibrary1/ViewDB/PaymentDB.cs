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
        public string Status { get; set; }
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

                    try { s.LessonId = int.Parse(reader["LessonId"].ToString()); } catch { s.LessonId = 0; }
                    try { s.Status = reader["Status"].ToString(); } catch { s.Status = s.paid ? "Paid" : "Pending"; }
                    try { s.Notes = reader["Notes"].ToString(); } catch { s.Notes = ""; }
                    try { s.ParcialAmount = int.Parse(reader["ParcialAmount"].ToString()); } catch { s.ParcialAmount = 0; }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("PaymentDB CreateModel Error: " + ex.Message);
                }
            }
        }

        private int GetNextPaymentId()
        {
            object result = SelectScalar("SELECT MAX(PaymentID) FROM [Payments]");
            return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) + 1 : 1;
        }

        public List<Payment> SelectPaymentByStudentID(int id)
        {
            return Select("SELECT * FROM [Payments] WHERE StudentID = ?",
                new OleDbParameter("@studentId", id)).OfType<Payment>().ToList();
        }

        public List<Payment> SelectPaymentByTeacherID(int id)
        {
            return Select("SELECT * FROM [Payments] WHERE TeacherID = ?",
                new OleDbParameter("@teacherId", id)).OfType<Payment>().ToList();
        }

        public List<Payment> SelectPaymentByPaymentID(int id)
        {
            return Select("SELECT * FROM [Payments] WHERE PaymentID = ?",
                new OleDbParameter("@paymentId", id)).OfType<Payment>().ToList();
        }

        public List<Payment> GetPaymentsByLessonId(int lessonId)
        {
            return Select("SELECT * FROM [Payments] WHERE LessonId = ?",
                new OleDbParameter("@lessonId", lessonId)).OfType<Payment>().ToList();
        }

        /// <summary>
        /// FIXED: Process payment - updates Lessons table AND inserts payment
        /// </summary>
        public void Pay(Payment payment)
        {
            // Calculate partial amount
            if (payment.NumberOfPayments > 0)
                payment.ParcialAmount = payment.Amount / payment.NumberOfPayments;
            else
            {
                payment.ParcialAmount = payment.Amount;
                payment.NumberOfPayments = 1;
            }

            // STEP 1: Mark the lesson as paid FIRST
            if (payment.paid && payment.LessonId > 0)
            {
                int lessonUpdated = SaveChanges("UPDATE [Lessons] SET [Paid] = ? WHERE [LessonID] = ?",
                    new OleDbParameter("@paid", true),
                    new OleDbParameter("@lessonId", payment.LessonId));

                System.Diagnostics.Debug.WriteLine($"Lesson {payment.LessonId} paid update: {lessonUpdated} rows");
            }

            // STEP 2: Insert payment record
            int nextId = GetNextPaymentId();

            SaveChanges(@"INSERT INTO [Payments] 
                ([PaymentID], [StudentID], [TeacherID], [Amount], [PaymentDate], [PaymentMethod], 
                 [NumberOfPayments], [paid], [ParcialAmount], [LessonId], [Status], [Notes])
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                new OleDbParameter("@paymentId", nextId),
                new OleDbParameter("@studentId", payment.StudentID),
                new OleDbParameter("@teacherId", payment.TeacherID),
                new OleDbParameter("@amount", payment.Amount),
                new OleDbParameter("@paymentDate", payment.PaymentDate),
                new OleDbParameter("@paymentMethod", payment.PaymentMethod ?? "Cash"),
                new OleDbParameter("@numberOfPayments", payment.NumberOfPayments),
                new OleDbParameter("@paid", payment.paid),
                new OleDbParameter("@parcialAmount", payment.ParcialAmount),
                new OleDbParameter("@lessonId", payment.LessonId),
                new OleDbParameter("@status", payment.Status ?? (payment.paid ? "Paid" : "Pending")),
                new OleDbParameter("@notes", payment.Notes ?? ""));
        }

        public void UpdatePayment(Payment payment)
        {
            SaveChanges("UPDATE [Payments] SET [paid] = ? WHERE [PaymentID] = ?",
                new OleDbParameter("@paid", payment.paid),
                new OleDbParameter("@paymentId", payment.PaymentID));
        }

        public bool CheckPaid(int id)
        {
            var list = Select("SELECT * FROM [Payments] WHERE PaymentID = ?",
                new OleDbParameter("@paymentId", id)).OfType<Payment>().ToList();
            return list.Count > 0 && list[0].paid;
        }

        public decimal GetTeacherIncome(int teacherId, DateTime fromDate, DateTime toDate)
        {
            object result = SelectScalar(@"SELECT SUM(Amount) FROM [Payments] 
                WHERE TeacherID = ? AND paid = ? AND PaymentDate >= ? AND PaymentDate <= ?",
                new OleDbParameter("@teacherId", teacherId),
                new OleDbParameter("@paid", true),
                new OleDbParameter("@fromDate", fromDate),
                new OleDbParameter("@toDate", toDate));

            return (result != null && result != DBNull.Value) ? Convert.ToDecimal(result) : 0;
        }

        public List<Payment> GetOutstandingPayments(int studentId)
        {
            return Select("SELECT * FROM [Payments] WHERE StudentID = ? AND paid = ? ORDER BY PaymentDate",
                new OleDbParameter("@studentId", studentId),
                new OleDbParameter("@paid", false)).OfType<Payment>().ToList();
        }

        public List<Payment> GetOverduePayments()
        {
            return Select("SELECT * FROM [Payments] WHERE paid = ? AND PaymentDate < ? ORDER BY PaymentDate",
                new OleDbParameter("@paid", false),
                new OleDbParameter("@today", DateTime.Today)).OfType<Payment>().ToList();
        }

        public void MarkAsOverdue(int paymentId)
        {
            SaveChanges("UPDATE [Payments] SET [Status] = ? WHERE [PaymentID] = ?",
                new OleDbParameter("@status", "Overdue"),
                new OleDbParameter("@paymentId", paymentId));
        }
    }
}