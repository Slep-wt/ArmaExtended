# ArmaExtended
An Arma 3 extension that incorporates Discord and MySql async functionality 

Author: Slep. | Licence: GNU GPL v3
-----------------------------------

Setup:
-----------------------------------
    Configure the config.json file in /cfg - (Note: Make sure to enable the modules you want to use. I.e if Mysql is enabled, the mysql settings must be configured)

Usage:
-----------------------------------
        (No args)
        "aex" callExtension "<Function>"

        (Args)
        "aex" callExtension ["<Function>", [SessionKey, Arg1, Arg2, ...]]

Functions
-----------------------------------

    Startup:
        load
          - Arguments: None
          - Return Value: SessionKey (string)
          - Summary: Initialises all the modules and returns the active session key (Store that in a variable, any other function will not work without this and it is only broadcast once)

    Mysql:
        mysql:async
          - Arguments: [string SessionKey, string Command, bool Read]
          - Return Values: 
            - Case Read=false:            MYSQL_NQ_RESULT (NQ_NOROWS or NQ_SUCCESS)
            - Case Read=true:             string Data
            - Case Read=true, Data > 8kb: [string Data, [int BufferIndex, int DataIndex]]
          - Summary: An asynchronus function that handles reading and writing to and from a MySql Database
          
            

        mysql:buffer
           - Arguments: [int BufferIndex, int DataIndex]
           - Return Value: [string BufferData, [int BufferIndex, int NewDataIndex]]
           - Summary: Fetches data from the MySQL buffer at a BufferIndex, with data at the DataIndex.
           - Notes:
             - A data index of -1 indicates that there is no more data left to be read. Reading with a data index of -1 will result in a                MYSQL_BUFFER_BAD_INPUT error.

    Discord:
        discord:send
          - Arguments: [string SessionKey, string Message, int ChannelSelect]
          - Return Value: Boolean - true (Message Sent) or false (Message failed to send)
          - Summary: Makes a request to the specified webhook/s in config.json to send a Discord message

        discord:sendrich
          - Arguments: [string SessionKey, string Contents, int ChannelSelect, string Color]
          - Return Value: Boolean - true (Message Sent) or false (Message failed to send)
          - Summary: Makes a formatted request to the specified webhook/s in config.json to send a mbedded Discord message
          - Notes:
            - string Contents must be structured as "<Title>;<EmbedName>;<EmbedContents>;"
            - string Color must be of either "GREEN", "ORANGE", "YELLOW" or "RED"

Status Codes
-----------------------------------
    MYSQL_NQ_SUCCESS
        - Successfully (Indicates tables have been altered) issued a command to the database without a return value

    MYSQL_NO_ROWS
        - Command did not alter any of the specified cells or rows

    MYSQL_REQUEST_FAILED
        - Failed to send the command/query to the server. Check config or connection for issues

    MYSQL_BUFFER_BAD_INPUT
        - Data at the provided indexes for BufferIndex and DataIndex is null.

    INIT_RL_REJECT
        - Attempted to reinitialise the extension (Blocked for security reasons regarding the SessionKey)


When in doubt if something is working, check the latest log file generated in /Logs, will display any exceptions or errors caught.





