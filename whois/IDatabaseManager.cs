using MySql.Data.MySqlClient;

namespace whois
{
    public interface IDatabaseManager
    {
        MySqlConnection Connection { get; }

        void AddNewUser(string LoginId);
        string CheckLocationExists(string value);
        string CheckPositionExists(string value);
        string CheckUserExists(string LoginId);
        string DeleteUser(string LoginId);
        string GetDump(string LoginId);
        string GetLookup(string LoginId, string field);
        string HandleFieldInput(string sqlCmd);
        string HandleFieldInput(string sqlCmd, int numberOfFields);
        string UpdateConatctInfo(string field, string value, string LoginId);
        string UpdateExistingUser(string LoginId, string field, string valueToInsert);
        string UpdatePersonalInfo(string field, string value, string LoginId);
    }
}