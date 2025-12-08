using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class UserInfo : Base
    {
        private string username;
        private string password;
        private string email;
        private string phone;
        private bool isAdmin;
        private int teacherId;
        private double rating;
        private string ratingT;
        private int lessonPrice;
        public bool IsAdmin { get => isAdmin; set => isAdmin = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public string Email { get => email; set => email = value; }
        public string Phone { get => phone; set => phone = value; }
        public int TeacherId { get => teacherId; set => teacherId = value; }
        public int StudentId { get; set; }
        public double Rating { get => rating; set => rating = value; }
        public string RatingText { get => ratingT; set => ratingT = value; }
        public string Rewiew { get; set; }
        public bool Confirmed { get; set; }
        public string Lessons { get; set; }
        public int LessonPrice { get => lessonPrice; set => lessonPrice = value; }
    }
}
