# ConnectionPoolExhauster
Demonstrates the effects and diagnosis options for an exhausted .NET connection pool of Sql Server connections

## Demonstrating the effects of trying to create a new connection after exhausting the connection pool

The /ConsoleApp project demonstrates the behavior of leaking connections with all the defaults set

100 connections get opened, never closed, and the 101st connection will timeout with a specific exception after trying for 15 seconds

```
Opened new connection, total connections 1.  Current time: 8/8/2017 8:27:21 PM
Opened new connection, total connections 2.  Current time: 8/8/2017 8:27:21 PM
Opened new connection, total connections 3.  Current time: 8/8/2017 8:27:21 PM
... snip...
Opened new connection, total connections 96.  Current time: 8/8/2017 8:27:21 PM
Opened new connection, total connections 97.  Current time: 8/8/2017 8:27:21 PM
Opened new connection, total connections 98.  Current time: 8/8/2017 8:27:21 PM
Opened new connection, total connections 99.  Current time: 8/8/2017 8:27:21 PM
Opened new connection, total connections 100.  Current time: 8/8/2017 8:27:21 PM
Exception occurred: System.InvalidOperationException: Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool.  This may have occurred because all pooled connections were in use and max pool size was reached.
   at System.Data.ProviderBase.DbConnectionFactory.TryGetConnection(DbConnection owningConnection, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal oldConnection, DbConnectionInternal& connection)
   at System.Data.ProviderBase.DbConnectionInternal.TryOpenConnectionInternal(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at System.Data.ProviderBase.DbConnectionClosed.TryOpenConnection(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at System.Data.SqlClient.SqlConnection.TryOpenInner(TaskCompletionSource`1 retry)
   at System.Data.SqlClient.SqlConnection.TryOpen(TaskCompletionSource`1 retry)
   at System.Data.SqlClient.SqlConnection.Open()
   at ConsoleApp.Program.Main(String[] args) in d:\code\ConnectionPoolExhauster\ConsoleApp\Program.cs:line 21.  Current time: 8/8/2017 8:27:36 PM
All done
```

## Demonstrating how to find the query that is related to a leaked connection

The /ConsoleAppWithQuery project leaks only **some** (1 in every 5) connections by not cleaning them up after running a query.  What query is related to the leaked connections?

### The console output

This shows that many connections are created and reused successfully (since more than 100 iterations occurred) and the same exception as above is thrown

```
Opened new connection in iteration 1.  Current time: 8/8/2017 8:49:53 PM
Opened new connection in iteration 2.  Current time: 8/8/2017 8:49:53 PM
Opened new connection in iteration 3.  Current time: 8/8/2017 8:49:53 PM
... snip ...
Opened new connection in iteration 489.  Current time: 8/8/2017 8:49:53 PM
Opened new connection in iteration 491.  Current time: 8/8/2017 8:49:53 PM
Opened new connection in iteration 492.  Current time: 8/8/2017 8:49:53 PM
Opened new connection in iteration 493.  Current time: 8/8/2017 8:49:53 PM
Opened new connection in iteration 494.  Current time: 8/8/2017 8:49:53 PM
Exception occurred: System.InvalidOperationException: Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool.  This may have occurred because all pooled connections were in use and max pool size was reached.
   at System.Data.ProviderBase.DbConnectionFactory.TryGetConnection(DbConnection owningConnection, TaskCompletionSource`1 retry, DbConnectionOptions userOptions, DbConnectionInternal oldConnection, DbConnectionInternal& connection)
   at System.Data.ProviderBase.DbConnectionInternal.TryOpenConnectionInternal(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at System.Data.ProviderBase.DbConnectionClosed.TryOpenConnection(DbConnection outerConnection, DbConnectionFactory connectionFactory, TaskCompletionSource`1 retry, DbConnectionOptions userOptions)
   at System.Data.SqlClient.SqlConnection.TryOpenInner(TaskCompletionSource`1 retry)
   at System.Data.SqlClient.SqlConnection.TryOpen(TaskCompletionSource`1 retry)
   at System.Data.SqlClient.SqlConnection.Open()
   at ConsoleAppWithQuery.Program.Main(String[] args) in d:\code\ConnectionPoolExhauster\ConsoleAppWithQuery\Program.cs:line 24.  Current time: 8/8/2017 8:50:08 PM
All done
```

### Querying for the offending queries related to the leaked connections

After running the ConsoleAppWithQuery project but **not** exiting once the exception is thrown, this query can be run:

```
SELECT
	recent.text AS 'Last SQL Statement',
	connection.connect_time AS 'connected since'
FROM
	sys.dm_exec_connections AS connection
CROSS APPLY
	sys.dm_exec_sql_text(connection.most_recent_sql_handle) AS recent
ORDER BY
	connection.connect_time ASC
```

the query output does indicate that every 5th connection was leaked and shows the associated query

```
use [master]	2017-08-08 20:14:11.533
select Has_Perms_By_Name(N'dbo.TheOnlyTable', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TheOnlyTable', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TheOnlyTable', 'Object', 'CONTROL') as Contr_Per 	2017-08-08 20:24:26.617
SELECT * from TheOnlyTable WHERE id = 0	2017-08-08 20:37:48.053
SELECT * from TheOnlyTable WHERE id = 5	2017-08-08 20:37:48.070
SELECT * from TheOnlyTable WHERE id = 10	2017-08-08 20:37:48.077
SELECT * from TheOnlyTable WHERE id = 15	2017-08-08 20:37:48.080
SELECT * from TheOnlyTable WHERE id = 20	2017-08-08 20:37:48.083
SELECT * from TheOnlyTable WHERE id = 25	2017-08-08 20:37:48.087
SELECT * from TheOnlyTable WHERE id = 30	2017-08-08 20:37:48.090
SELECT * from TheOnlyTable WHERE id = 35	2017-08-08 20:37:48.093
SELECT * from TheOnlyTable WHERE id = 40	2017-08-08 20:37:48.097
SELECT * from TheOnlyTable WHERE id = 45	2017-08-08 20:37:48.100
... snip ...
SELECT * from TheOnlyTable WHERE id = 480	2017-08-08 20:37:48.517
SELECT * from TheOnlyTable WHERE id = 485	2017-08-08 20:37:48.520
SELECT * from TheOnlyTable WHERE id = 490	2017-08-08 20:37:48.523
SELECT * from TheOnlyTable WHERE id = 495	2017-08-08 20:37:48.530
SELECT   recent.text AS 'Last SQL Statement',   connection.connect_time AS 'connected since'  FROM   sys.dm_exec_connections AS connection  CROSS APPLY   sys.dm_exec_sql_text(connection.most_recent_sql_handle) AS recent  ORDER BY   connection.connect_time ASC	2017-08-08 20:38:54.677
ALTER EVENT SESSION [telemetry_xevents] ON SERVER STATE = stop           ALTER EVENT SESSION [telemetry_xevents] ON SERVER STATE = start;	2017-08-08 20:39:10.403
```

after exiting the program, and rerunning the same query results in a much shorter list:

```
use [master]	2017-08-08 20:14:11.533
select Has_Perms_By_Name(N'dbo.TheOnlyTable', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TheOnlyTable', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TheOnlyTable', 'Object', 'CONTROL') as Contr_Per 	2017-08-08 20:24:26.617
SELECT   recent.text AS 'Last SQL Statement',   connection.connect_time AS 'connected since'  FROM   sys.dm_exec_connections AS connection  CROSS APPLY   sys.dm_exec_sql_text(connection.most_recent_sql_handle) AS recent  ORDER BY   connection.connect_time ASC	2017-08-08 20:38:54.677
ALTER EVENT SESSION [telemetry_xevents] ON SERVER STATE = stop           ALTER EVENT SESSION [telemetry_xevents] ON SERVER STATE = start;	2017-08-08 20:39:10.403
SELECT   recent.text AS 'Last SQL Statement',   connection.connect_time AS 'connected since'  FROM   sys.dm_exec_connections AS connection  CROSS APPLY   sys.dm_exec_sql_text(connection.most_recent_sql_handle) AS recent  ORDER BY   connection.connect_time ASC	2017-08-08 20:40:32.557
```
