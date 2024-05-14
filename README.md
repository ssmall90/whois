# WHOIS Server  

## Project Structure
The project consists of three main files:
1. **DatabaseManager.cs**: This file contains the `DatabaseManager` class, which is responsible for managing interactions with the database. It handles operations such as retrieving user data, checking user existence, adding new users, updating user information, and deleting users.
   
2. **WhoisServer.cs**: This file contains the `WhoisServer` class, which represents the WHOIS server. It interacts with the `DatabaseManager` to handle WHOIS queries and commands. The server can be started and commands can be processed based on command-line arguments.

3. **Program.cs**: This is the entry point of the application. It imports necessary namespaces, instantiates the `DatabaseManager` and `WhoisServer` objects, and handles command-line arguments to either start the WHOIS server or process specific commands.

## Functionality
- **Database Interaction**: The `DatabaseManager` class handles interactions with the MySQL database. It provides methods for querying user information, checking user existence, adding new users, updating user data, and deleting users.
  
- **WHOIS Server**: The `WhoisServer` class represents the WHOIS server. It can be started to listen for incoming WHOIS queries. It also provides functionality to process WHOIS commands received via command-line arguments.
  
- **Command-Line Interface**: The application accepts command-line arguments to control the behavior of the WHOIS server. When no arguments are provided, the server is started. Otherwise, specific commands are processed.

## Technologies Used
- **C#**: The project is written in C#.
- **MySQL Database**: The application interacts with a MySQL database to store and retrieve user information.


## Project Execution
To run the project:
1. Compile the C# code.
2. Make sure a MySQL database is set up with the required schema.
3. Run the compiled executable, optionally providing command-line arguments to control the server's behavior.

## Future Improvements
Some potential improvements for the project include:
- Error Handling: Enhance error handling to provide more informative messages to users.
- Security: Implement security measures to prevent unauthorized access to the WHOIS server and database.
- Performance Optimization: Optimize database queries and server operations for better performance.
