using Google.Protobuf.WellKnownTypes;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using whois;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

DatabaseManager databaseManager = new DatabaseManager();

if (args.Length == 0)
{
    RunServer();
}
else
{
    for (int i = 0; i < args.Length; i++)
    {
        ProcessCommand(args[i]);
    }
}


void RunServer()
{

    TcpListener listener;
    Socket connection;
    NetworkStream socketStream;
    try
    {
        listener = new TcpListener(IPAddress.Any, 43);
        listener.Start();
        Console.WriteLine("Server Has Started Listening");

        while (true)
        {
            connection = listener.AcceptSocket();
            socketStream = new NetworkStream(connection);
            connection.SendTimeout = 10000;
            connection.ReceiveTimeout = 10000;
            Console.WriteLine("Connection Received");
            DoRequest(socketStream);
            socketStream.Close();
            connection.Close();
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e.ToString()); // An example
    }
}

void DoRequest(NetworkStream socketStream)
{

    StreamWriter sw = new StreamWriter(socketStream);
    StreamReader sr = new StreamReader(socketStream);

    try
    {

        string line = sr.ReadLine();


        if (line == null)
        {
            Console.WriteLine("Ignoring null command");
            return;
        }

        else if (line == "POST / HTTP/1.1")
        {
            int contentLength = 0;

            while (line != "")
            {
                line = sr.ReadLine(); // Skip to blank line

                if (line.StartsWith("Content-Length: "))
                {
                    contentLength = Int32.Parse(line.Substring(16));
                }
            }

            line = "";

            for (int i = 0; i < contentLength; i++) line += (char)sr.Read();

            String[] slices = line.Split(new char[] { '&' }, 2);
            String ID = slices[0].Substring(5);
            String value = slices[1].Substring(9);

            string result = databaseManager.UpdateExistingUser(ID, "location", value);

            if (result.Contains("could not be found in database"))
            {
                sw.WriteLine("HTTP/1.1 404 Not Found");
                sw.WriteLine("Content-Type: text/plain");
                sw.WriteLine();
                sw.WriteLine(result);
                sw.Flush();
                Console.WriteLine($"Received an update request for '{ID}' to '{value}");
                Console.WriteLine(result);
            }
            else
            {

                sw.WriteLine("HTTP/1.1 200 OK");
                sw.WriteLine("Content-Type: text/plain");
                sw.WriteLine();
                sw.WriteLine(result);
                sw.Flush();

                Console.WriteLine($"Received an update request for '{ID}' to '{value}");
                Console.WriteLine(result);
            }

        }

        else if (line.StartsWith("GET /?name=") && line.EndsWith(" HTTP/1.1"))
        {
            string[] slices = line.Split(" ");  // Split into 3 pieces
            string ID = slices[1].Substring(7);  //
            string result = (databaseManager.GetLookup(ID, "location"));

            if (result is not null)
            {

                sw.WriteLine("HTTP/1.1 200 OK");
                sw.WriteLine("Content-Type: text/plain");
                sw.WriteLine();
                sw.WriteLine(result);
                sw.Flush();

                Console.WriteLine($"Performed Lookup on '{ID}' returning '{result}'");
            }

            else
            {
                sw.WriteLine("HTTP/1.1 404 Not Found");
                sw.WriteLine("Content-Type: text/plain");
                sw.WriteLine();
                sw.Flush();
                Console.WriteLine($"Performed Lookup on '{ID}' returning '404 Not Found'");

            }

        }
        else
        {

            sw.WriteLine("HTTP/1.1 400 Bad Request");
            sw.WriteLine("Content-Type: text/plain");
            sw.WriteLine();
            sw.Flush();

            Console.WriteLine($"Unrecognised command: '{line}'");

        }

    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fault in Command Processing:{ex.ToString()} ");
        sw.Close();
        sr.Close();
    }
    finally
    {
        sw.Close();
        sr.Close();

    }
}

void ProcessCommand(string command)
{
    DatabaseManager databaseManager = new DatabaseManager();

    try
    {
        Console.WriteLine($"\nCommand: {command}");
        String[] slice = command.Split(new char[] { '?' }, 2);
        String ID = slice[0];
        String operation = null;
        String update = null;
        String field = null;

        if (slice.Length == 2)
        {
            operation = slice[1];
            String[] pieces = operation.Split(new char[] { '=' }, 2);
            field = pieces[0];
            if (pieces.Length == 2) update = pieces[1];
        }

        if (operation == null || operation == string.Empty)
        {

            if (databaseManager.GetLookup(ID, "loginId") is not null)
            {
                Dump(ID);
            }
            else
            {
                Console.WriteLine("User does not exist");
            }
        }

        else if (update == null)
        {
            Lookup(ID, field);
        }

        else
        {
            if (databaseManager.GetLookup(ID, "loginId") is not null)
            {
                string newID = command.Split("?")[0];

                update = command.Split("=")[1];

                databaseManager.UpdateNewUser(newID, field, update);

            }
            else
            {
                update = command.Split("=")[1];
                Update(ID, field, update);
            }

        }
    }

    catch (Exception e)
    {
        Console.WriteLine($"Fault in Command Processing: {e.ToString()}");
    }

}

void Dump(String ID)
{

    databaseManager.GetDump(ID);


}

void Lookup(String ID, String field)
{
    if (databaseManager.GetLookup(ID, "loginId") is not null)
    {
        Console.WriteLine($"lookup field: {field}");
        Console.WriteLine(databaseManager.GetLookup(ID, field));
    }
    else
    {
        Console.WriteLine($"lookup field: {field}");
        Console.WriteLine($"User '{ID}' does not exist");
    }

}

void Update(String ID, String field, String update)
{

    if (databaseManager.GetLookup(ID, "loginId") is not null)
    {

        databaseManager.UpdateExistingUser(ID, field, update);

        Console.WriteLine(databaseManager.GetLookup(ID, field));
    }
    else
    {
        databaseManager.UpdateNewUser(ID, field, update);
    }


    Console.WriteLine("OK");
}

