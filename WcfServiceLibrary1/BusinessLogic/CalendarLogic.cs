using Model;
using System.Collections.Generic;
using ViewDB;

namespace BusinessLogic
{
    /// <summary>
    /// Business logic for teacher availability calendars: working days,
    /// unavailable dates, and special days.
    /// </summary>
    public static class CalendarLogic
    {
        public static bool SetTeacherCalendar(Calendars cal, int teacherId)
        {
            return new CalendarDB().SetTeacherCalendar(cal, teacherId);
        }

        public static Calendars GetTeacherCalendar(int teacherId)
        {
            return new CalendarDB().GetTeacherCalendar(teacherId);
        }

        public static List<Calendars> GetTeacherUnavailableDates(int teacherId)
        {
            return new CalendarDB().GetTeacherUnavailableDates(teacherId);
        }

        public static List<Calendars> TeacherSpacialDays(int teacherId)
        {
            return new CalendarDB().TeacherSpacialDays(teacherId);
        }
    }
}
