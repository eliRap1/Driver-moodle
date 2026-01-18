using System;
using System.Data.OleDb;
using WcfServiceLibrary1.Helpers;
using ViewDB;

namespace DatabaseMigration
{
    /// <summary>
    /// ONE-TIME MIGRATION: Converts plain text passwords to hashed passwords
    /// Run this ONCE before deploying the new security system
    /// </summary>
    public class MigratePasswordsToHash
    {
        public static void MigrateAllPasswords()
        {
            Console.WriteLine("=== PASSWORD MIGRATION TOOL ===");
            Console.WriteLine("This will convert all plain text passwords to secure hashes.");
            Console.WriteLine("WARNING: This operation cannot be reversed!");
            Console.WriteLine();
            Console.Write("Type 'YES' to continue: ");

            string confirm = Console.ReadLine();
            if (confirm != "YES")
            {
                Console.WriteLine("Migration cancelled.");
                return;
            }

            int studentsUpdated = 0;
            int teachersUpdated = 0;

            try
            {
                // Migrate Student passwords
                studentsUpdated = MigrateTable("Student");
                Console.WriteLine($"Migrated {studentsUpdated} student passwords");

                // Migrate Teacher passwords
                teachersUpdated = MigrateTable("Teacher");
                Console.WriteLine($"Migrated {teachersUpdated} teacher passwords");

                Console.WriteLine();
                Console.WriteLine("✓ Migration completed successfully!");
                Console.WriteLine($"Total passwords migrated: {studentsUpdated + teachersUpdated}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("✗ Migration failed: " + ex.Message);
                Console.WriteLine("Stack trace: " + ex.StackTrace);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static int MigrateTable(string tableName)
        {
            int count = 0;
            OleDbConnection connection = BaseDB.GetConnection();

            try
            {
                connection.Open();

                // Get all users with their current passwords
                string selectSql = $"SELECT id, username, password FROM [{tableName}]";
                OleDbCommand selectCmd = new OleDbCommand(selectSql, connection);
                OleDbDataReader reader = selectCmd.ExecuteReader();

                var usersToUpdate = new System.Collections.Generic.List<Tuple<int, string, string>>();

                while (reader.Read())
                {
                    int id = (int)reader["id"];
                    string username = reader["username"].ToString();
                    string currentPassword = reader["password"].ToString();

                    usersToUpdate.Add(new Tuple<int, string, string>(id, username, currentPassword));
                }

                reader.Close();

                // Update each password
                foreach (var user in usersToUpdate)
                {
                    int id = user.Item1;
                    string username = user.Item2;
                    string plainPassword = user.Item3;

                    // Check if password is already hashed (base64 encoded, length ~48+)
                    if (plainPassword.Length > 40 && IsBase64String(plainPassword))
                    {
                        Console.WriteLine($"  Skipping {username} - already hashed");
                        continue;
                    }

                    // Hash the plain text password
                    string hashedPassword = SecurityHelper.HashPassword(plainPassword);

                    // Update database
                    string updateSql = $"UPDATE [{tableName}] SET password = ? WHERE id = ?";
                    OleDbCommand updateCmd = new OleDbCommand(updateSql, connection);
                    updateCmd.Parameters.AddWithValue("@password", hashedPassword);
                    updateCmd.Parameters.AddWithValue("@id", id);

                    int rows = updateCmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        Console.WriteLine($"  ✓ Migrated: {username}");
                        count++;
                    }
                    else
                    {
                        Console.WriteLine($"  ✗ Failed: {username}");
                    }
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            return count;
        }

        private static bool IsBase64String(string value)
        {
            try
            {
                Convert.FromBase64String(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// For testing: Create a test user with hashed password
        /// </summary>
        public static void CreateTestUser()
        {
            string username = "testuser";
            string password = "Test123!";
            string hashedPassword = SecurityHelper.HashPassword(password);

            Console.WriteLine($"Test User: {username}");
            Console.WriteLine($"Plain Password: {password}");
            Console.WriteLine($"Hashed Password: {hashedPassword}");
            Console.WriteLine();
            Console.WriteLine("Verification Test:");
            bool isValid = SecurityHelper.VerifyPassword(password, hashedPassword);
            Console.WriteLine($"  Correct password: {isValid}");
            bool isInvalid = SecurityHelper.VerifyPassword("WrongPassword", hashedPassword);
            Console.WriteLine($"  Wrong password: {isInvalid}");
        }
    }
}