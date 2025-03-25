using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb; // ACCESS זה של Ole

namespace ViewDB
{
    //כל המחלקות של התיקיה מיועדות לקבל נתונים מבסיס נתונים ולתרגם אותם לאובייקטים של
    //MyModel

    public abstract class BaseDB
    {
        private int id; // Identification number (of person, course etc.)

        private static string connectionString = null;
        protected OleDbConnection connection;
        protected OleDbCommand command;
        protected OleDbDataReader reader;

        protected abstract Base NewEntity();

        public BaseDB()
        {
            connection = GetConnection();
            command = new OleDbCommand();
            command.Connection = connection;
        }

        public static OleDbConnection GetConnection()
        {
            if (connectionString == null)
            {
                string ApplicationBaseFolder = AppDomain.CurrentDomain.BaseDirectory;  // directory of EXE file, at bin/debug directory
                connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + ApplicationBaseFolder + "\\..\\..\\..\\ViewDB\\StudentsDB.accdb;Persist Security Info=True";
            }
            return new OleDbConnection(connectionString);
        }

        protected List<Base> CreateModel()
        {
            return null;
        }
        //  protected abstract List<Base> Select();


        protected virtual void CreateModel(Base entity)
        {
            if (entity != null) //האם הצלחתי בהמרה, האם הטיפוס מתאים?
            {
                try 
                {
                    entity.Id = (int)reader["id"]; //int.Parse(reader["id"].ToString());
                }
                catch 
                {
                    Console.WriteLine("No ID in DB.");
                }
            }
        }


        protected virtual List<Base> Select(string sqlCommandTxt)
        {
            /*
            אם זה מעניין אותך:
תיקונים קטנטנים במצגת(מה שמצאתי היום) :
שקף 68
            */
            List<Base> list = new List<Base>();
            try
            {
                connection.Open(); //was missing
                command.CommandText = sqlCommandTxt;
                reader = command.ExecuteReader(); 
                //NULLבנתיים לא בודקים האם אחד השדות הוא 
                while (reader.Read())
                {
                    Base entity = NewEntity(); //יוצר אובייקט מטיפוס המתאים
                    CreateModel(entity);
                    list.Add(entity);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message); //will word is every world, not only in world of Console

                //the output - we'll see in the output window of VisualStudio
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return list;
        }


        protected int SaveChanges(string command_text)
        {
            OleDbCommand command = new OleDbCommand();
            int records = 0;
            try
            {
                command.Connection = this.connection;
                command.CommandText = command_text;
                connection.Open();
                records = command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\nSQL:" + command.CommandText);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return records;
        }
    }
}
