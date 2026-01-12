using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewDB
{
    public class CalendarDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new Calendars();
        }

        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    Calendars s = (Calendars)entity;

                    // Determine which table we're reading from based on columns
                    if (HasColumn(reader, "UnavailableDate") && HasColumn(reader, "AllDay")
                        && !HasColumn(reader, "availableDays"))
                    {
                        // ========================================
                        // TeacherUnavailableDate table
                        // ========================================
                        var u = new UnavailableDay();

                        DateTime dt;
                        if (DateTime.TryParse(reader["UnavailableDate"].ToString(), out dt))
                            u.Date = dt;
                        else
                            u.Date = DateTime.MinValue;

                        bool allday = false;
                        bool.TryParse(reader["AllDay"].ToString(), out allday);
                        u.AllDay = allday;

                        u.StartTime = reader["startTime"] != DBNull.Value ? reader["startTime"].ToString() : null;
                        u.EndTime = reader["endTime"] != DBNull.Value ? reader["endTime"].ToString() : null;

                        if (s.UnavailableDays == null)
                            s.UnavailableDays = new List<UnavailableDay>();
                        s.UnavailableDays.Add(u);

                        // Also set TeacherID if available
                        if (HasColumn(reader, "TeacherID"))
                            s.Teacherid = int.Parse(reader["TeacherID"].ToString());
                    }
                    else if (HasColumn(reader, "SelectedDate") && HasColumn(reader, "SelectedDay"))
                    {
                        // ========================================
                        // TeacherSpacialDays table
                        // ========================================
                        var sd = new SpecialDay();

                        DateTime dt;
                        if (DateTime.TryParse(reader["SelectedDate"].ToString(), out dt))
                            sd.Date = dt;
                        else
                            sd.Date = DateTime.MinValue;

                        sd.StartTime = reader["startTime"] != DBNull.Value ? reader["startTime"].ToString() : "08:00";
                        sd.EndTime = reader["endTime"] != DBNull.Value ? reader["endTime"].ToString() : "20:00";

                        if (s.SpecialDaysList == null)
                            s.SpecialDaysList = new List<SpecialDay>();
                        s.SpecialDaysList.Add(sd);

                        // Also set TeacherID if available
                        if (HasColumn(reader, "TeacherID"))
                            s.Teacherid = int.Parse(reader["TeacherID"].ToString());
                    }
                    else if (HasColumn(reader, "availableDays"))
                    {
                        // ========================================
                        // Availability table
                        // ========================================
                        s.StartTime = reader["startTime"] != DBNull.Value ? reader["startTime"].ToString() : "08:00";
                        s.EndTime = reader["endTime"] != DBNull.Value ? reader["endTime"].ToString() : "20:00";

                        string availDays = reader["availableDays"] != DBNull.Value ? reader["availableDays"].ToString() : "";
                        s.AvailableDays = string.IsNullOrEmpty(availDays)
                            ? new List<string>()
                            : availDays.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        // Optional fields that may not exist
                        if (HasColumn(reader, "UnavailableDate"))
                        {
                            string unavailDates = reader["UnavailableDate"] != DBNull.Value ? reader["UnavailableDate"].ToString() : "";
                            s.DatesUnavailable = string.IsNullOrEmpty(unavailDates)
                                ? new List<string>()
                                : unavailDates.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        }

                        if (HasColumn(reader, "WorkingHours"))
                        {
                            string workHours = reader["WorkingHours"] != DBNull.Value ? reader["WorkingHours"].ToString() : "";
                            s.WorkingHours = string.IsNullOrEmpty(workHours)
                                ? new List<string>()
                                : workHours.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        }

                        if (HasColumn(reader, "selectedDays"))
                            s.SelectedDay = reader["selectedDays"] != DBNull.Value ? reader["selectedDays"].ToString() : "";

                        if (HasColumn(reader, "AllDay"))
                        {
                            bool allDay = false;
                            bool.TryParse(reader["AllDay"].ToString(), out allDay);
                            s.AllDay = allDay;
                        }

                        if (HasColumn(reader, "TeacherID"))
                            s.Teacherid = int.Parse(reader["TeacherID"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("CreateModel mapping issue: " + ex.Message);
                }
            }
        }

        private bool HasColumn(IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public Calendars GetTeacherCalendar(int teacherId)
        {
            Calendars result = new Calendars();

            // ---------------------------
            // 1) Load WEEKLY availability
            // ---------------------------
            string sqlAvailability = $"SELECT * FROM Availability WHERE TeacherID = {teacherId}";
            List<Calendars> list = Select(sqlAvailability).OfType<Calendars>().ToList();

            if (list.Count > 0)
            {
                Calendars baseCal = list[0];
                result.Teacherid = teacherId;
                result.StartTime = baseCal.StartTime;
                result.EndTime = baseCal.EndTime;
                result.AvailableDays = baseCal.AvailableDays ?? new List<string>();
                result.DatesUnavailable = baseCal.DatesUnavailable ?? new List<string>();
            }
            else
            {
                // fallback defaults if Availability missing
                result.Teacherid = teacherId;
                result.AvailableDays = new List<string>();
                result.StartTime = "08:00";
                result.EndTime = "20:00";
            }

            // -----------------------------------
            // 2) Load TeacherUnavailableDate rows
            // -----------------------------------
            string sqlUnavailable = $"SELECT * FROM TeacherUnavailableDate WHERE TeacherID = {teacherId}";
            List<Calendars> unavailableRows = Select(sqlUnavailable).OfType<Calendars>().ToList();

            if (result.UnavailableDays == null)
                result.UnavailableDays = new List<UnavailableDay>();

            foreach (var row in unavailableRows)
            {
                if (row.UnavailableDays != null)
                {
                    foreach (var ud in row.UnavailableDays)
                    {
                        result.UnavailableDays.Add(new UnavailableDay
                        {
                            Date = ud.Date,
                            AllDay = ud.AllDay,
                            StartTime = ud.StartTime,
                            EndTime = ud.EndTime
                        });
                    }
                }
            }

            // -----------------------------------
            // 3) Load TeacherSpacialDays rows
            // -----------------------------------
            string sqlSpecial = $"SELECT * FROM TeacherSpacialDays WHERE TeacherID = {teacherId}";
            List<Calendars> specialRows = Select(sqlSpecial).OfType<Calendars>().ToList();

            if (result.SpecialDaysList == null)
                result.SpecialDaysList = new List<SpecialDay>();

            foreach (var row in specialRows)
            {
                if (row.SpecialDaysList != null)
                {
                    foreach (var sd in row.SpecialDaysList)
                    {
                        result.SpecialDaysList.Add(new SpecialDay
                        {
                            Date = sd.Date,
                            StartTime = sd.StartTime,
                            EndTime = sd.EndTime
                        });
                    }
                }
            }

            return result;
        }

        public List<Calendars> GetTeacherUnavailableDates(int teacherId)
        {
            string sqlStr = "SELECT * FROM TeacherUnavailableDate WHERE TeacherID = " + teacherId;
            List<Calendars> list = Select(sqlStr).OfType<Calendars>().ToList();
            return list;
        }

        public List<Calendars> TeacherSpacialDays(int teacherId)
        {
            string sqlStr = "SELECT * FROM TeacherSpacialDays WHERE TeacherID = " + teacherId;
            List<Calendars> list = Select(sqlStr).OfType<Calendars>().ToList();
            return list;
        }

        public bool SetTeacherCalendar(Calendars cal, int teacherId)
        {
            bool success = true;

            try
            {
                // --- 1️⃣ Check if Availability row exists
                string existsSql = $"SELECT * FROM [Availability] WHERE [TeacherID] = {teacherId}";
                bool exists = Select(existsSql).OfType<Calendars>().Any();

                string startTime = cal.StartTime ?? "08:00";
                string endTime = cal.EndTime ?? "20:00";
                string availableDays = (cal.AvailableDays != null)
                    ? string.Join(",", cal.AvailableDays)
                    : "";

                // --- 2️⃣ Insert or Update Availability
                string sql;
                if (exists)
                {
                    sql = $@"
                UPDATE [Availability]
                SET [startTime] = '{startTime}',
                    [endTime]   = '{endTime}',
                    [availableDays] = '{availableDays}'
                WHERE [TeacherID] = {teacherId}";
                }
                else
                {
                    sql = $@"
                INSERT INTO [Availability] 
                    ([TeacherID],[startTime],[endTime],[availableDays])
                VALUES
                    ({teacherId},'{startTime}','{endTime}','{availableDays}')";
                }
                SaveChanges(sql);

                // --- 3️⃣ Update TeacherUnavailableDate table
                string deleteUnavailable = $"DELETE FROM [TeacherUnavailableDate] WHERE [TeacherID] = {teacherId}";
                SaveChanges(deleteUnavailable);

                if (cal.UnavailableDays != null && cal.UnavailableDays.Count > 0)
                {
                    foreach (var u in cal.UnavailableDays)
                    {
                        if (u.Date != DateTime.MinValue)
                        {
                            string allDayStr = u.AllDay ? "True" : "False";
                            string st = string.IsNullOrEmpty(u.StartTime) ? "" : u.StartTime;
                            string et = string.IsNullOrEmpty(u.EndTime) ? "" : u.EndTime;

                            string insertUnavailable = $@"
            INSERT INTO [TeacherUnavailableDate] 
                ([TeacherID],[UnavailableDate],[AllDay],[startTime],[endTime])
            VALUES ({teacherId}, #{u.Date:MM/dd/yyyy}#, {allDayStr}, '{st}', '{et}')";
                            SaveChanges(insertUnavailable);
                        }
                    }
                }

                // --- 4️⃣ Update TeacherSpacialDays
                string deleteSpecial = $"DELETE FROM [TeacherSpacialDays] WHERE [TeacherID] = {teacherId}";
                SaveChanges(deleteSpecial);

                if (cal.SpecialDaysList != null && cal.SpecialDaysList.Count > 0)
                {
                    foreach (var sp in cal.SpecialDaysList)
                    {
                        if (sp.Date != DateTime.MinValue)
                        {
                            string st = string.IsNullOrEmpty(sp.StartTime) ? "08:00" : sp.StartTime;
                            string et = string.IsNullOrEmpty(sp.EndTime) ? "20:00" : sp.EndTime;

                            string insertSpecial = $@"
                    INSERT INTO [TeacherSpacialDays] 
                        ([TeacherID],[SelectedDate],[startTime],[endTime])
                    VALUES 
                        ({teacherId}, #{sp.Date:MM/dd/yyyy}#, '{st}', '{et}')";
                            SaveChanges(insertSpecial);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SetTeacherCalendar failed: " + ex.Message);
                success = false;
            }

            return success;
        }
    }
}