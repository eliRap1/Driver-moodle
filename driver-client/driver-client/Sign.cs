using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace driver_client
{
    public class Sign
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int TeacherId { get; set; }
        public bool IsTeacher { get; set; }
        public int Id { get; set; }
        public string RatingText { get; set; }
        public string Role { get; set; }
        public int LessonPrice { get; set; } = 200; // Default lesson price
    }
}