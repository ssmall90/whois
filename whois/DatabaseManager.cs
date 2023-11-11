using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace whois
{
    internal class DatabaseManager
    {
        static string connectionString = "server=localhost;user=root;database=acw_whois_database;port=3306;password=L3tM31n";
        private MySqlConnection _connection;


        public MySqlConnection Connection { get { return _connection; } }

        public DatabaseManager()
        {
            _connection = new MySqlConnection(connectionString);
        }

        public void GetDump(string LoginId)
        {

            _connection.Open();

            string getUserDump = $"SELECT * FROM acw_whois_database.users WHERE LoginId = '{LoginId}'";

            MySqlCommand cmd = new MySqlCommand(getUserDump, _connection);

            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Console.WriteLine($"User Id: {rdr[0]}");
                Console.WriteLine($"Login Id: {rdr[1]}");
                Console.WriteLine($"Title: {rdr[2]}");
                Console.WriteLine($"Fornames: {rdr[3]}");
                Console.WriteLine($"Surname: {rdr[4]}");
                Console.WriteLine($"Position: {rdr[5]}");
                Console.WriteLine($"Email: {rdr[6]}");
                Console.WriteLine($"Phone: {rdr[7]}");
                Console.WriteLine($"Location: {rdr[8]}");

            }
            rdr.Close();

            _connection.Close();

        }

        public string GetLookup(string LoginId, string field)
        {
            _connection.Open();

            string result = null;

            string getLookUp = $"SELECT {field} FROM acw_whois_database.users WHERE loginId = '{LoginId}';";

            MySqlCommand cmd = new MySqlCommand(getLookUp, _connection);

            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                result = ($"Look up of {field} for {LoginId} returned {rdr[0]}");
            }

            _connection.Close();

            return result;

        }

        public void UpdateNewUser(string LoginId, string field, string valueToInsert)
        {
            _connection.Open();

            int lastID = 0;

            string retrieveLastId = "SELECT MAX(userId) FROM users";

            MySqlCommand cmd = new MySqlCommand(retrieveLastId, _connection);

            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                lastID = int.Parse(rdr[0].ToString()!) + 1;

            }
            rdr.Close();

            string addNewUser = $"INSERT INTO acw_whois_database.users (`userId`,`loginId`,`{field}`) VALUES ({lastID},'{LoginId}','{valueToInsert}')";

            cmd.CommandText = addNewUser;

            cmd.ExecuteNonQuery();

            _connection.Close();


        }

        public string UpdateExistingUser(string LoginId, string field, string valueToInsert)
        {

            try
            {
                //_connection.Open();

                if (GetLookup(LoginId,field) != null)
                {
                    _connection.Open();

                    string UpdateUserInfo = $"UPDATE acw_whois_database.users SET {field} = '{valueToInsert}' WHERE loginId = '{LoginId}'";

                    MySqlCommand cmd = new MySqlCommand(UpdateUserInfo, _connection);

                    cmd.ExecuteNonQuery();

                    _connection.Close();

                    return $"Successfully updated {LoginId}'s {field} to {valueToInsert}";
                }

                else
                { 
                    return $"{LoginId} could not be found in database";
                }


            }
            catch (Exception ex)
            {
                return $"Database error {(ex.ToString())}";
            }

        }

        public string DeleteUser(string LoginId)
        {
            try
            {

                if (GetLookup(LoginId, "loginId") != null)
                {
                    _connection.Open();

                    string deleteUser = $"DELETE FROM `acw_whois_database`.`users` WHERE (`loginId` = '{LoginId}')";

                    MySqlCommand cmd = new MySqlCommand(deleteUser, _connection);

                    cmd.ExecuteNonQuery();

                    _connection.Close();

                    return $"User '{LoginId}' has been deleted from the database";
                }


                else
                {
                    return $"User '{LoginId}' could not be found in database";
                }

            }
            catch (Exception ex)
            {
                return $"Database error {(ex.ToString())}";
            }

        }

    }
}
