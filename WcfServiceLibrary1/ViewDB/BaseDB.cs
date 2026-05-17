using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace ViewDB
{
    public abstract class BaseDB
    {
        private static string connectionString = null;
        private static readonly object connectionStringLock = new object();
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
                lock (connectionStringLock)
                {
                    if (connectionString == null)
                    {
                        connectionString = ResolveConnectionString();
                    }
                }
            }
            return new OleDbConnection(connectionString);
        }

        private static string ResolveConnectionString()
        {
            string appBase = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = appBase + "\\..\\..\\..\\ViewDB\\UsersDataBase.accdb";
            string[] providers = { "Microsoft.ACE.OLEDB.16.0", "Microsoft.ACE.OLEDB.12.0" };
            var failures = new List<string>();

            foreach (string provider in providers)
            {
                string cs = $"Provider={provider};Data Source={dbPath};Persist Security Info=True";
                try
                {
                    using (var testConn = new OleDbConnection(cs))
                    {
                        testConn.Open();
                    }
                    System.Diagnostics.Debug.WriteLine($"BaseDB: using OleDb provider {provider}");
                    return cs;
                }
                catch (Exception ex) when (IsProviderMissingError(ex))
                {
                    failures.Add($"{provider}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"BaseDB: provider {provider} unavailable - {ex.Message}");
                }
            }

            string msg =
                "Access Database Engine is NOT installed on this machine. " +
                "Neither Microsoft.ACE.OLEDB.16.0 nor Microsoft.ACE.OLEDB.12.0 is registered. " +
                "FIX: open elevated PowerShell in the repo root and run: scripts\\install-access-engine.ps1 " +
                "-- or download manually from https://www.microsoft.com/en-us/download/details.aspx?id=54920 " +
                "(pick AccessDatabaseEngine_X64.exe for x64 builds, AccessDatabaseEngine.exe for x86 / 'Prefer 32-bit'). " +
                "After install, fully close and reopen Visual Studio.\n\nProvider probe details:\n" +
                string.Join("\n", failures);
            throw new InvalidOperationException(msg);
        }

        private static bool IsProviderMissingError(Exception ex)
        {
            if (ex == null) return false;
            string m = ex.Message ?? string.Empty;
            if (m.IndexOf("not registered", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (m.IndexOf("לא רשום", StringComparison.Ordinal) >= 0) return true; // "לא רשום" (Hebrew)
            if ((uint)ex.HResult == 0x80040154) return true; // REGDB_E_CLASSNOTREG
            return false;
        }

        protected virtual void CreateModel(Base entity)
        {
            if (entity != null)
            {
                try
                {
                    entity.Id = (int)reader["id"];
                }
                catch
                {
                    Console.WriteLine("No ID in DB.");
                }
            }
        }

        /// <summary>
        /// SECURE: Execute SELECT with parameterized query
        /// </summary>
        protected virtual List<Base> Select(string sqlCommandTxt, params OleDbParameter[] parameters)
        {
            List<Base> list = new List<Base>();
            try
            {
                connection.Open();
                command.CommandText = sqlCommandTxt;
                command.Parameters.Clear();

                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Base entity = NewEntity();
                    CreateModel(entity);
                    list.Add(entity);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Select Error: " + ex.Message);
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

        /// <summary>
        /// SECURE: Execute SELECT for reviews with parameterized query
        /// </summary>
        protected virtual List<string> SelectReview(string sqlCommandTxt, params OleDbParameter[] parameters)
        {
            List<string> list = new List<string>();
            try
            {
                connection.Open();
                command.CommandText = sqlCommandTxt;
                command.Parameters.Clear();

                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(reader["Rewiew"].ToString());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SelectReview Error: " + ex.Message);
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

        /// <summary>
        /// SECURE: Execute INSERT/UPDATE/DELETE with parameterized query
        /// </summary>
        protected int SaveChanges(string commandText, params OleDbParameter[] parameters)
        {
            int records = 0;
            OleDbCommand cmd = new OleDbCommand();
            try
            {
                cmd.Connection = connection;
                cmd.CommandText = commandText;
                cmd.Parameters.Clear();

                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                    System.Diagnostics.Debug.WriteLine($"SaveChanges: Executing with {parameters.Length} parameters");
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var p = parameters[i];
                        System.Diagnostics.Debug.WriteLine($"  Param[{i}]: {p.ParameterName} = {p.Value} (Type: {p.OleDbType})");
                    }
                }

                connection.Open();
                records = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine($"SaveChanges: Successfully affected {records} row(s)");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"SaveChanges Error: {e.Message}");
                System.Diagnostics.Debug.WriteLine($"SQL: {cmd.CommandText}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {e.StackTrace}");
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return records;
        }

        /// <summary>
        /// SECURE: Execute SELECT and return single scalar value
        /// </summary>
        protected object SelectScalar(string query, params OleDbParameter[] parameters)
        {
            try
            {
                command.CommandText = query;
                command.Parameters.Clear();

                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                object result = command.ExecuteScalar();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SelectScalar Error: " + ex.Message);
                return null;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
    }
}
