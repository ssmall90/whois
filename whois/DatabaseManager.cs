using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace whois
{
    /// <summary>
    /// This is a databse managing class for the database and networking coursework. 
    /// </summary>
    /// 
    public class DatabaseManager : IDatabaseManager
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

            string getUserDump = $"SELECT \r\n    personalInfo.userId,\r\n    loginIdTable.loginId,\r\n    personalInfo.title,\r\n    personalInfo.forenames,\r\n    personalInfo.surname,\r\n    positions.positionTitle AS position,\r\n    contactInformation.email,\r\n    contactInformation.primaryPhone AS phone,\r\n    locations.locationName AS location_name\r\nFROM personalInfo \r\nJOIN loginIdTable ON personalInfo.userId = loginIdTable.userId\r\nJOIN userPositions userPositions ON personalInfo.userId = userPositions.userId\r\nJOIN positions ON userPositions.positionId = positions.positionId\r\nJOIN contactInformation ON personalInfo.userId = contactInformation.userId\r\nJOIN loginLocations ON loginIdTable.loginId = loginLocations.loginId\r\nJOIN locations ON loginLocations.locationId = locations.locationId\r\nWHERE loginidtable.loginId = '{LoginId}';\r\n";

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


        public string CheckUserExists(string LoginId)

        {
            string result = null;

            _connection.Open();

            string checkUser = $"SELECT loginId\r\nFROM loginIdTable\r\nWHERE loginId = '{LoginId}'";

            MySqlCommand cmd = new MySqlCommand(checkUser, _connection);

            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                result = $"{rdr[0]}";
            }

            _connection.Close();

            return result;

        }

        public string GetLookup(string LoginId, string field)
        {
            string result = null;

            if (field.ToLower() == "location" || field.ToLower() == "userid" || field.ToLower() == "loginid" || field.ToLower() == "forenames" || field.ToLower() == "surname" || field.ToLower() == "position" || field.ToLower() == "email" || field.ToLower() == "phone")
            {
                _connection.Open();

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
            else
            {

                return $"{field} is not a recognised field ";
            }



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

                if (GetLookup(LoginId, field) != null)
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
