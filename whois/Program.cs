using Google.Protobuf.WellKnownTypes;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using whois;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

IDatabaseManager databaseManager = new DatabaseManager();
WhoisServer whoisServer = new WhoisServer(databaseManager);

if (args.Length == 0)
{
    whoisServer.RunServer();
}
else
{
    for (int i = 0; i < args.Length; i++)
    {
       whoisServer.ProcessCommand(args[i]);
    }
}

