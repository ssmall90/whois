using MySql.Data.MySqlClient;

namespace whois
{
    public interface IDatabaseManager
    {
        MySqlConnection Connection { get; }

        string CheckUserExists(string LoginId);
        string DeleteUser(string LoginId);
        void GetDump(string LoginId);
        string GetLookup(string LoginId, string field);
        string UpdateExistingUser(string LoginId, string field, string valueToInsert);
        void UpdateNewUser(string LoginId, string field, string valueToInsert);
    }
}