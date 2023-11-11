﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace whois
{
    public class WhoisServer
    {
        IDatabaseManager databaseManager;

        public WhoisServer(IDatabaseManager pDatabaseManager)
        {
            databaseManager = pDatabaseManager;
        }

        /// <summary>
        /// When invoked this method will create and run a server.
        /// </summary>
        public void RunServer()
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


        /// <summary>
        /// When the server receives a connection from on the TCP listener
        /// This method processes POST or GET requests and returns an appropriate reply
        /// </summary>
        /// <param name="socketStream"></param>
        public void DoRequest(NetworkStream socketStream)
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


        /// <summary>
        /// This method is used to handle command line inputs formatted according to coursework brief.
        /// It processes inputs and allocates any permissable commands to the database manager.
        /// </summary>
        /// <param name="command"></param>
        public void ProcessCommand(string command)
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


                    if (operation == "")
                    {

                        Delete(ID);
                        return;
                    }
                }

                if (operation == null)
                {

                    if (databaseManager.GetLookup(ID, "loginId") is not null)
                    {
                        Dump(ID);
                        return;
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
                    if (databaseManager.GetLookup(ID, "loginId") is null)
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

        /// <summary>
        /// Returns a list of all column headers and their associated values from a specified user/row in databse
        /// </summary>
        /// <param name="ID"></param>
        public void Dump(String ID)
        {
            databaseManager.GetDump(ID);
        }

        /// <summary>
        /// Receives a user id and a field in the database, it returns the associated value for that field.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="field"></param>
        public void Lookup(String ID, String field)
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

        /// <summary>
        /// This method receives 3 arguments, a user id, a field in the database , and a value.
        /// It updates the received field with the received value, for the specified user. 
        /// If the user does not exist in the database , A new user is created and the value is added.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="field"></param>
        /// <param name="update"></param>
        void Update(String ID, String field, String update)
        {

            if (databaseManager.GetLookup(ID, "loginId") is not null)
            {

                Console.WriteLine(databaseManager.UpdateExistingUser(ID, field, update));

            }
            else
            {
                databaseManager.UpdateNewUser(ID, field, update);
            }


            Console.WriteLine("OK");
        }


        /// <summary>
        /// This method will delete a user from the database
        /// </summary>
        /// <param name="ID"></param>
        void Delete(String ID)
        {
            Console.WriteLine($"Are you sure you want to delete record '{ID}' from database? Y/N");

            string response = Console.ReadLine();

            if (response == "Y" || response == "y")
            {
                Console.WriteLine(databaseManager.DeleteUser(ID));
            }
            else
            {
                Console.WriteLine($"User '{ID}' was not deleted from database");
            }

        }

    }
}
