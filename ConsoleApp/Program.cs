using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Object> listToPreventGarbageCollection = new List<object>();

            try
            {
                while (true)
                {
                    SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Initial Catalog=ConnectionPoolExhausterDatabase");
                    connection.Open();
                    listToPreventGarbageCollection.Add(connection);

                    Console.WriteLine("Opened new connection, total connections {0}.  Current time: {1}", listToPreventGarbageCollection.Count, DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred: {0}.  Current time: {1}", ex.ToString(), DateTime.Now);
            }

            Console.Write("All done");
            Console.ReadLine();
        }
    }
}
