# ExtendedExtension
An Arma 3 extension that incorporates Discord and MySql async functionality 

Author: Slep. | Licence: GNU GPL v3
-----------------------------------------------
    ___     ______ _  __
   /   |   / ____/| |/ /
  / /| |  / __/   |   / 
 / ___ | / /___  /   |  
/_/  |_|/_____/ /_/|_| 


ExtendedExtension Readme
-----------------------------------------------

Setup:
    Configure the config.json file in /cfg - (Note: Make sure to enable the modules you want to use. I.e if Mysql is enabled, the mysql settings must be configured)

Usage:
    Predefined:
        (Function Init) (SERVER)
        SessionKey = call AEX_LOAD

        (MySQL Request) (SERVER)
        [SessionKey, Command, Read] call AEX_MYSQL_ASYNC

        (DiscordAPI) (SERVER)
        [SessionKey, Message] call AEX_DISCORD_SEND

    Manual:
        (No args)
        "aex" callExtension "<Function>"

        (Args)
        "aex" callExtension ["<Function>", [SessionKey, Arg1, Arg2, ...]]

Functions:
    Startup:
        load
          - Arguments: None
          - Return Value: SessionKey (string)
          - Summary: Initialises all the modules and returns the active session key (Store that in a variable, any other function will not work without this)

    Mysql:
        mysql:async
          - Arguments: [string SessionKey, string Command, bool Read]
          - Return Value: If Read is true, Returns a value fetched from a cell, Else returns a mysql:async status code (Read below)
          - Summary: An asynchronus function that handles reading and writing to and from a MySql Database

    Discord:
        discord:send
          - Arguments: [string SessionKey, string Message]
          - Return Value: Boolean - true (Message Sent) or false (Message failed to send)
          - Summary: Makes a request to the specified webhook in config.json to send a Discord message

Status Codes:
    MYSQL_NQ_SUCCESS
        - Successfully (Indicates tables have been altered) issued a command to the database without a return value

    MYSQL_NO_ROWS
        - Command did not alter any of the specified cells or rows

    MYSQL_REQUEST_FAILED
        - Failed to send the command/query to the server. Check config or connection for issues

    INIT_RL_REJECT
        - Attempted to reinitialise the extension (Blocked for security reasons regarding the SessionKey)


When in doubt if something is working, check the latest log file generated in /logs, will display any exceptions or errors caught.





