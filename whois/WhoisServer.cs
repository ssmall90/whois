using Microsoft.VisualBasic;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace whois
{
    /// <summary>
    /// This is a server class for the database and networking coursework. 
    /// </summary>
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
            Handler requestHandler;

            try
            {
                //Create a TCP socket to listen for requests on port 43.
                #region Create and Start TCP Listener
                listener = new TcpListener(IPAddress.Any, 43);
                listener.Start();
                //Console.WriteLine("Server Has Started Listening");
                #endregion



                //Start a contiinous Loop to handle incoming requests.
                #region Handle Incoming Requests 
                while (true)
                {

                    //Upon receipt of a request create a socket to handle it.
                    Console.WriteLine("Server Has Started Listening.....");
                    connection = listener.AcceptSocket();

                    //connection.SendTimeout = 1000;
                    //connection.ReceiveTimeout = 1000;

                    requestHandler = new Handler(databaseManager);
                    Thread t = new Thread(() => requestHandler.DoRequest(connection));
                    t.Start();

                }
                #endregion
            }
            // Catch any errors and display exception to console. 
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public class Handler
        {

            IDatabaseManager _databaseManager;

            public Handler(IDatabaseManager databaseManager)
            {
                _databaseManager = databaseManager;
            }

            /// <summary>
            /// When the server receives a connection from on the TCP listener
            /// This method processes POST or GET requests and returns an appropriate reply
            /// </summary>
            /// <param name="socketStream"></param>
            public void DoRequest(Socket connection)
            {
                NetworkStream socketStream;
                socketStream = new NetworkStream(connection);
                Console.WriteLine("Connection Received");


                //Create Streamreader and Streamwriter to handle socket I/O
                StreamWriter sw = new StreamWriter(socketStream);
                StreamReader sr = new StreamReader(socketStream);

                try
                {
                    //Set timeout value to 1 second
                    //socketStream.ReadTimeout = 1000;
                    //socketStream.WriteTimeout = 1000;

                    //Read first line
                    string line = sr.ReadLine().Trim();


                    //Handle any null lines
                    if (line == null)
                    {
                        Console.WriteLine("Ignoring null command");
                        return;
                    }


                    #region Handling of Post Request 
                    else if (line == "POST / HTTP/1.1")
                    {
                        int contentLength = 0;

                        while (line != "")
                        {
                            line = sr.ReadLine(); // Skip to blank line

                            if (line.StartsWith("Content-Length: ")) //
                            {
                                contentLength = Int32.Parse(line.Substring(16)); // Retrieve length of the content
                            }
                        }

                        // Set line to empty string and append each character of the stream to line
                        line = "";
                        for (int i = 0; i < contentLength; i++) line += (char)sr.Read(); // 


                        //Split line into 2 sections and store the ID and the Value 
                        String[] slices = line.Split(new char[] { '&' }, 2);
                        String ID = slices[0].Substring(5);
                        String value = slices[1].Substring(9);


                        //Return result from update request to the database
                        string result;

                        if (_databaseManager.CheckUserExists(ID) is null)
                        {

                            _databaseManager.AddNewUser(ID);
                            result = _databaseManager.UpdateExistingUser(ID, "location", value);

                        }
                        else
                        {
                            result = _databaseManager.UpdateExistingUser(ID, "location", value);
                        }


                        //if request was unsuccessful send response and console log the result
                        if (result.Contains("could not be found in database"))
                        {
                            sw.WriteLine("HTTP/1.1 404 Not Found");
                            sw.WriteLine("Content-Type: text/plain");
                            sw.WriteLine();
                            sw.WriteLine(result);
                            sw.Flush();
                            Console.WriteLine($"Received an update request for '{ID}' to '{value}");
                            Console.WriteLine(result);
                            Console.WriteLine();

                        }

                        //If result was successful, send response and console log the result
                        else
                        {

                            sw.WriteLine("HTTP/1.1 200 OK");
                            sw.WriteLine("Content-Type: text/plain");
                            sw.WriteLine();
                            sw.WriteLine(result);
                            sw.Flush();

                            Console.WriteLine($"Received an update request for '{ID}' to '{value}");
                            Console.WriteLine();
                            Console.WriteLine(result);

                        }

                    }
                    #endregion



                    #region Handling of Get Request
                    else if (line.StartsWith("GET /?name=") && line.EndsWith(" HTTP/1.1"))
                    {
                        string[] slices = line.Split(" ");  // Split into 3 pieces
                        string ID = slices[1].Substring(7);  // Store ID


                        // Look up Location field of specified ID in database
                        string result = (_databaseManager.GetLookup(ID, "location"));


                        //If lookup was successful. Write and send response, then console log the result of lookup.
                        if (result is not null)
                        {

                            sw.WriteLine("HTTP/1.1 200 OK");
                            sw.WriteLine("Content-Type: text/plain");
                            sw.WriteLine();
                            sw.WriteLine(result);
                            sw.Flush();


                            Console.WriteLine($"Performed Lookup on '{ID}' returning '{result}'");
                            Console.WriteLine();
                        }


                        //If lookup was unsuccessful. Write and send response, then console log the result of lookup.
                        else
                        {
                            sw.WriteLine("HTTP/1.1 404 Not Found");
                            sw.WriteLine("Content-Type: text/plain");
                            sw.WriteLine();
                            sw.Flush();
                            Console.WriteLine($"Performed Lookup on '{ID}' returning '404 Not Found'");
                            Console.WriteLine();


                        }

                    }
                    #endregion


                    #region Handling Unrecognised Requests
                    // Write and send response. Console log the unrecognised command
                    else
                    {

                        sw.WriteLine("HTTP/1.1 400 Bad Request");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        sw.Flush();

                        Console.WriteLine($"Unrecognised command: '{line}'");
                        Console.WriteLine();


                    }
                    #endregion

                }


                // Catch and unexpected behaviour during command processing and display exceptions to console.
                catch (Exception ex)
                {
                    Console.WriteLine($"Respone Timeout");
                    Console.WriteLine();
                    sw.Close();
                    sr.Close();

                }

                // Close streamreader and streamwriter as request has now been processed.
                finally
                {

                    //Close network stream and socket once request is complete.

                    sw.Close();
                    sr.Close();
                    socketStream.Close();
                    connection.Close();

                }
            }

        }



        /// <summary>
        /// This method is used to handle command line inputs formatted according to coursework brief.
        /// It processes inputs and allocates any permissable commands to the database manager.
        /// </summary>
        /// <param name="command"></param>
        public void ProcessCommand(string command)
        {

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


                    if (operation == "") // checks for delete command
                    {

                        Delete(ID);
                        return;
                    }
                }


                if (operation == null) // checks for dump command
                {

                    if (databaseManager.CheckUserExists(ID) is not null)
                    {
                        Dump(ID);
                        return;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("User does not exist");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }


                else if (update == null) // checks for look up command
                {
                    Lookup(ID, field);
                }

                else // checks for update command 
                {
                    if (databaseManager.CheckUserExists(ID) is null) // checks if user exsits then adds new user and updates corresponding field 
                    {
                        string newID = command.Split("?")[0];

                        update = command.Split("=")[1];

                        databaseManager.AddNewUser(newID); 
                        Update(newID, field, update);


                    }
                    else // updates exsisting users corresponding field.
                    {
                        update = command.Split("=")[1];
                        Update(ID, field, update);
                    }

                }
            }

            catch (Exception e) // Handles any unrecognised commands
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
            Console.WriteLine(databaseManager.GetDump(ID));
        }

        /// <summary>
        /// Receives a user id and a field in the database, it returns the associated value for that field.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="field"></param>
        public string Lookup(String ID, String field)
        {

            if (databaseManager.CheckUserExists(ID) is not null)
            {
                string result = databaseManager.GetLookup(ID, field);
                Console.WriteLine($"lookup field: {field}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(result);
                Console.ForegroundColor = ConsoleColor.White;

                return result;
            }
            else
            {
                Console.WriteLine($"lookup field: {field}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"User '{ID}' does not exist");
                Console.ForegroundColor = ConsoleColor.White;

                return $"User '{ID}' does not exist";
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
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(databaseManager.UpdateExistingUser(ID, field, update));
                Console.ForegroundColor = ConsoleColor.White;

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(databaseManager.UpdateExistingUser(ID, field, update));
                Console.ForegroundColor = ConsoleColor.White;
            }

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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(databaseManager.DeleteUser(ID));
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"User '{ID}' was not deleted from database");
                Console.ForegroundColor = ConsoleColor.White;
            }

        }

    }
}
