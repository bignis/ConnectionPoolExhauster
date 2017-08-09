using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ConsoleAppWithQuery
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Object> listToPreventGarbageCollection = new List<object>();

            try
            {
                for (int i = 0; i < 1000; ++i)
                {
                    // If it's a multiple of 5, leak it
                    bool shouldLeakThisConnection = i % 5 == 0;

                    SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Initial Catalog=ConnectionPoolExhausterDatabase");
                    connection.Open();

                    // Put the loop counter into the query for investigation later
                    SqlCommand command = new SqlCommand(string.Format("SELECT * from TheOnlyTable WHERE id = {0}", i), connection);

                    command.ExecuteReader();

                    if (shouldLeakThisConnection)
                    {
                        listToPreventGarbageCollection.Add(connection);
                        listToPreventGarbageCollection.Add(command);
                        continue;
                    }

                    // Clean up everything manually
                    command.Dispose();
                    connection.Dispose();

                    Console.WriteLine("Opened new connection in iteration {0}.  Current time: {1}", i, DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred: {0}.  Current time: {1}", ex.ToString(), DateTime.Now);

                // Once this exception is hit, *before the program exits*, run this command to get the query text for the leaked connections
                /* SELECT
	                    recent.text AS 'Last SQL Statement',
	                    connection.connect_time AS 'connected since'
                    FROM
	                    sys.dm_exec_connections AS connection
                    CROSS APPLY
	                    sys.dm_exec_sql_text(connection.most_recent_sql_handle) AS recent
                    ORDER BY
	                    connection.connect_time ASC
                */
            }

            Console.Write("All done");
            Console.ReadLine();
        }
    }
}
 