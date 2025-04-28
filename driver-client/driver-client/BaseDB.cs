using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb; // ACCESS זה של Ole
using Model;

namespace ViewDB
{
    public class BaseDB
    {        
        static private string connectionString = null;
        protected OleDbConnection connection;
        protected OleDbCommand command;
        protected OleDbDataReader reader;

        //is used to get IDs only from DB. for example in SignIn we want to get ID.
        protected virtual Base NewEntity()
        {
            return new Base();
        }

        public BaseDB()
        {
            if (connectionString == null)
            {
                string applicationBaseFolder = AppDomain.CurrentDomain.BaseDirectory;  // directory of EXE file, at bin/debug directory
                connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + applicationBaseFolder + "\\..\\..\\..\\ViewDB\\ArchiveData3.accdb;Persist Security Info=True";
            }
            connection = new OleDbConnection(connectionString);
            command = new OleDbCommand();
            command.Connection = connection;
        }

        protected List<Base> CreateModel()
        {
            return null;
        }
        //  protected abstract List<BaseEntity> Select();

        protected virtual Base CreateModel(Base entity)
        {

            if (entity != null) //האם הצלחתי בהמרה, האם הטיפוס מתאים?
            {
                entity.Id = (int)reader["ID"]; //int.Parse(reader["id"].ToString());
            }
            return entity;
        }

        protected virtual int Count(string sqlString)
        {
            int count = -1;
            try
            {
                command.CommandText = sqlString;
                connection.Open();
                //reader = command.ExecuteReader();
                count = System.Convert.ToInt32(command.ExecuteScalar());
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return count;
        }

        protected virtual string Max(string sqlString)
        {
            string ret =null;
            try
            {
                command.CommandText = sqlString;
                connection.Open();
                ret = (command.ExecuteScalar().ToString());
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return ret;
        }


        protected virtual List<Base> Select(string sqlString)
        {
            /*
            אם זה מעניין אותך:
תיקונים קטנטנים במצגת(מה שמצאתי היום) :
שקף 68
            */
            List<Base> list = new List<Base>();
            command.CommandText = sqlString;
            try
            {
                connection.Open(); 
                reader = command.ExecuteReader(); 
                //NULLבנתיים לא בודקים האם אחד השדות הוא 
                while (reader.Read())
                {
                    Base entity = NewEntity(); //יוצר אובייקט מטיפוס המתאים
                    list.Add(CreateModel(entity));
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
                

        //returns number of changed rows in the table
        protected int SaveChanges(string command_text)
        {
            //OleDbCommand command = new OleDbCommand();
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

        //returns number of changed rows in the table
        public virtual int AddToDB(Base entity)
        {
            string command_text = GetSQLInsertCommand(entity);
            return SaveChanges(command_text);
        }

        protected virtual string GetSQLInsertCommand(Base entity)
        {
            return null;
        }

        //returns ID of the according to the sqlString or -1 if not found.
        public static int GetIdByString    (OleDbConnection connection,
                                            OleDbCommand command,
                                            OleDbDataReader reader,
                                            string sqlString)
        {
            int foundId = -1;
            
            command.CommandText = sqlString;
            try
            {
                connection.Open();
                reader = command.ExecuteReader();
                //NULLבנתיים לא בודקים האם אחד השדות הוא 
                while (reader.Read())
                {
                    foundId = int.Parse(reader["id"].ToString());
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
            return foundId;
        }

        protected int InsertAndGetID(string sqlStr)
        {
            int id = -1;

            string sqlStr2 = "SELECT @@Identity";

            command.CommandText = sqlStr;
            OleDbCommand command2 = new OleDbCommand(sqlStr2, connection);
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                id = (int)command2.ExecuteScalar();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\nSQL:" + command.CommandText);
            }
            finally
            {
                //connection.Dispose(); //this closes the connection without any option to reOpen it.
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return id;
        }
    }
}
