using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
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

            string getUserDump = $"SELECT \r\n    personalInfo.userId,\r\n    loginIdTable.loginId,\r\n    personalInfo.title,\r\n    personalInfo.forenames,\r\n    personalInfo.surname,\r\n    positions.positionTitle AS position,\r\n    contactInformation.email,\r\n    contactInformation.primaryPhone AS phone,\r\n    locations.locationName AS location_name\r\nFROM personalInfo \r\nJOIN loginIdTable ON personalInfo.userId = loginIdTable.userId\r\nJOIN  userPositions ON personalInfo.userId = userPositions.userId\r\nJOIN positions ON userPositions.positionId = positions.positionId\r\nJOIN contactInformation ON personalInfo.userId = contactInformation.userId\r\nJOIN loginLocations ON loginIdTable.loginId = loginLocations.loginId\r\nJOIN locations ON loginLocations.locationId = locations.locationId\r\nWHERE loginidtable.loginId = '{LoginId}';\r\n";

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

        public string CheckLocationExists(string value)
        {
            string result = null;

            string checkValue = $"SELECT locationId\r\nFROM locations\r\nWHERE locationName = '{value}';";

            MySqlCommand cmd = new MySqlCommand(checkValue, _connection);

            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                result = $"{rdr[0]}";
            }

            rdr.Close();

            return result;
        }

        public string CheckPositionExists(string value)
        {
            string result = null;

            string checkValue = $"SELECT positionId\r\nFROM positions\r\nWHERE positionTitle = '{value}';";

            MySqlCommand cmd = new MySqlCommand(checkValue, _connection);

            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                result = $"{rdr[0]}";
            }

            rdr.Close();

            return result;
        }

        public string GetLookup(string LoginId, string field)
        {
            string result = null;
            field = field.ToLower();

            _connection.Open();

            switch (field)
            {
                case "location":

                    result = HandleFieldInput($"SELECT locations.locationName\r\nFROM loginLocations\r\nJOIN loginIdTable ON loginLocations.loginId = loginIdTable.loginId\r\nJOIN locations ON loginLocations.locationId = locations.locationId\r\nWHERE loginIdTable.loginId = '{LoginId}'");
                    break;

                case "position":

                    result = HandleFieldInput($"SELECT positionTitle\r\nFROM positions\r\nJOIN userPositions ON positions.positionId = userpositions.positionId\r\nJOIN loginidtable ON userpositions.userId = loginidtable.userId\r\nWhere loginId = '{LoginId}'");
                    break;

                case "userid":

                    result = HandleFieldInput($"SELECT userId\r\nFROM loginIdTable\r\nWHERE loginId = '{LoginId}'");
                    break;

                case "forenames":

                    result = HandleFieldInput($"SELECT forenames\r\nFROM personalinfo\r\nJOIN loginidtable ON personalinfo.userId = loginidtable.userId \r\nWHERE loginId = '{LoginId}'");
                    break;

                case "surname":

                    result = HandleFieldInput($"SELECT surname\r\nFROM personalinfo\r\nJOIN loginidtable ON personalinfo.userId = loginidtable.userId \r\nWHERE loginId = '{LoginId}'");
                    break;


                case "title":

                    result = HandleFieldInput($"SELECT title\r\nFROM personalinfo\r\nJOIN loginidtable ON personalinfo.userId = loginidtable.userId \r\nWHERE loginId = '{LoginId}'");
                    break;

                case "email":

                    result = HandleFieldInput($"SELECT email, adminEmail\r\nFROM contactinformation\r\nJOIN personalinfo ON contactinformation.userId = personalinfo.userId\r\nJOIN loginidtable ON personalinfo.userId = loginidtable.userId\r\nWHERE loginId = '{LoginId}'", 2);
                    break;

                case "phone":

                    result = HandleFieldInput($"SELECT primaryphone, secondaryphone\r\nFROM contactinformation\r\nJOIN personalinfo ON contactinformation.userId = personalinfo.userId\r\nJOIN loginidtable ON personalinfo.userId = loginidtable.userId\r\nWHERE loginId = '{LoginId}'", 2);
                    break;

                case "loginid":

                    result = HandleFieldInput($" SELECT *\r\nFROM loginIdTable\r\nWhere loginidtable.loginId = '{LoginId}'");
                    break;

                default:
                    return $"{field} is not a recognised field ";

            }

            return $"look up of '{field}' for loginid: '{LoginId}' returned:  \r\n{result}";


        }

        public void AddNewUser(string LoginId)
        {
            _connection.Open();

            // Insert new user into databe
            string addNewUser = $"INSERT INTO personalInfo (title, forenames, surname)\r\nVALUES (null, null, null)";

            MySqlCommand cmd = new MySqlCommand(addNewUser, _connection);

            cmd.ExecuteNonQuery();



            // Get New Users Id
            int lastID = 0;

            string retrieveLastId = "SELECT MAX(userId) FROM personalInfo";

            MySqlCommand cmd1 = new MySqlCommand(retrieveLastId, _connection);

            MySqlDataReader rdr = cmd1.ExecuteReader();

            while (rdr.Read())
            {
                lastID = int.Parse(rdr[0].ToString()!);

            }
            rdr.Close();


            //Add new user to all tables in database assign values as unknown or null.

            string updateLoginId = $"\r\nINSERT INTO loginIdTable (userId, loginId)\r\nVALUES ({lastID}, '{LoginId}'); \r\n\r\nINSERT INTO userPositions (userId, positionId)\r\nVALUES ({lastID}, (SELECT positionId FROM positions WHERE positionTitle = 'unknown')); \r\n\r\nINSERT INTO contactInformation (userId, email, adminEmail, primaryPhone, secondaryPhone)\r\nVALUES ({lastID}, NULL, NULL, NULL, NULL); \r\n\r\nINSERT INTO loginLocations (locationId, loginId)\r\nVALUES ((SELECT locationId FROM locations WHERE locationName = 'unknown'), '{LoginId}');";

            MySqlCommand cmd2 = new MySqlCommand(updateLoginId, _connection);

            cmd2.ExecuteNonQuery();

            _connection.Close();


        }

        public string UpdateExistingUser(string LoginId, string field, string valueToInsert)
        {

            try
            {
                _connection.Open();
                field = field.ToLower();

                switch (field)
                {
                    case "location":

                        if (CheckLocationExists(valueToInsert) == null)
                        {

                            string UpdateUserInfo = $"INSERT INTO locations (locationName)\r\nVALUES ('{valueToInsert}');\r\n\r\nUPDATE loginLocations\r\nSET locationId = (\r\n    SELECT locationId\r\n    FROM locations\r\n    WHERE locationName = '{valueToInsert}'\r\n)\r\nWHERE loginId = '{LoginId}';";

                            MySqlCommand cmd = new MySqlCommand(UpdateUserInfo, _connection);

                            cmd.ExecuteNonQuery();

                            _connection.Close();

                            return $"Successfully updated {LoginId}'s {field} to {valueToInsert}";

                        }
                        else
                        {
                            string UpdateUserInfo = $"UPDATE loginLocations\r\nSET locationId = (SELECT locationId FROM locations WHERE locationName = '{valueToInsert}')\r\nWHERE loginId = '{LoginId}';";

                            MySqlCommand cmd = new MySqlCommand(UpdateUserInfo, _connection);

                            cmd.ExecuteNonQuery();

                            _connection.Close();

                            return $"Successfully updated {LoginId}'s {field} to {valueToInsert}";
                        }

                    case "position":

                        if (CheckPositionExists(valueToInsert) == null)
                        {

                            string UpdateUserInfo = $"\r\nINSERT INTO positions (positionTitle)\r\nVALUES ('{valueToInsert}');\r\n\r\n  \r\nUPDATE userPositions\r\nSET positionId = (\r\n    SELECT positionId FROM positions WHERE positionTitle = '{valueToInsert}'\r\n)\r\nWHERE userId = (\r\n    SELECT userId FROM loginIdTable WHERE loginId = '{LoginId}'\r\n);";
                            MySqlCommand cmd = new MySqlCommand(UpdateUserInfo, _connection);

                            cmd.ExecuteNonQuery();

                            _connection.Close();

                            return $"Successfully updated {LoginId}'s {field} to {valueToInsert}";

                        }
                        else
                        {
                            string UpdateUserInfo = $"UPDATE userPositions\r\nSET positionId = (\r\n    SELECT positionId FROM positions WHERE positionTitle = '{valueToInsert}'\r\n)\r\nWHERE userId = (\r\n    SELECT userId FROM loginIdTable WHERE loginId = '{LoginId}'\r\n);";

                            MySqlCommand cmd = new MySqlCommand(UpdateUserInfo, _connection);

                            cmd.ExecuteNonQuery();

                            _connection.Close();

                            return $"Successfully updated {LoginId}'s {field} to {valueToInsert}";
                        }

                    case "forenames":
                        UpdatePersonalInfo(field, valueToInsert, LoginId);
                        return $"Successfully updated {LoginId}'s {field} to {valueToInsert}";

                    case "surname":
                        UpdatePersonalInfo(field, valueToInsert, LoginId);
                        return $"Successfully updated {LoginId}'s {field} to {valueToInsert}";

                    case "title":
                        UpdatePersonalInfo(field, valueToInsert, LoginId);
                        return $"Successfully updated {LoginId}'s {field} to {valueToInsert}";

                    case "email":
                        UpdateConatctInfo(field, valueToInsert, LoginId);
                        return $"Successfully updated {LoginId}'s {field} to {valueToInsert}";

                    case "phone":
                        UpdateConatctInfo("primaryphone", valueToInsert, LoginId);
                        return $"Successfully updated {LoginId}'s {field} to {valueToInsert}";

                    default:

                        return $"{field} is not a recognised field ";

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

                if (CheckUserExists(LoginId) != null)
                {
                    _connection.Open();

                    string deleteUser = $"DELETE FROM `acw_whois_database`.`loginlocations` WHERE (`loginId` = '{LoginId}')";

                    MySqlCommand cmd = new MySqlCommand(deleteUser, _connection);

                    cmd.ExecuteNonQuery();

                    string deleteUserLocation = $"DELETE FROM `acw_whois_database`.`loginIdTable` WHERE (`loginId` = '{LoginId}')";

                    MySqlCommand cmd1 = new MySqlCommand(deleteUserLocation, _connection);

                    cmd1.ExecuteNonQuery();

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

        public string HandleFieldInput(string sqlCmd, int numberOfFields)
        {
            string result = null;
            StringBuilder sb = new StringBuilder();

            string getLookUp = sqlCmd;
            MySqlCommand cmd = new MySqlCommand(getLookUp, _connection);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                for (int i = 0; i < numberOfFields; i++)
                {
                    sb.AppendLine(rdr[i].ToString());
                    sb.Append("");
                }
            }

            result = sb.ToString();

            _connection.Close();

            return result;
        }

        public string HandleFieldInput(string sqlCmd)
        {
            string result = null;

            string getLookUp = sqlCmd;
            MySqlCommand cmd = new MySqlCommand(getLookUp, _connection);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                result = $"{rdr[0]}";

            }

            _connection.Close();

            return result;
        }

        public string UpdatePersonalInfo(string field, string value, string LoginId)
        {

            string UpdateUserInfo = $"UPDATE personalInfo\r\nSET {field} = '{value}' \r\nWHERE userId = (\r\n    SELECT userId FROM loginIdTable WHERE loginId = '{LoginId}'\r\n);";

            MySqlCommand cmd = new MySqlCommand(UpdateUserInfo, _connection);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return $"Successfully updated {LoginId}'s {field} to {value}";
        }

        public string UpdateConatctInfo(string field, string value, string LoginId)
        {

            string UpdateUserInfo = $"UPDATE contactinformation\r\nSET {field} = '{value}'\r\nWHERE userid = (\r\nSELECT userId\r\nFROM loginidtable\r\nWhere loginId = '{LoginId}');";

            MySqlCommand cmd = new MySqlCommand(UpdateUserInfo, _connection);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return $"Successfully updated {LoginId}'s {field} to {value}";
        }


    }
}
