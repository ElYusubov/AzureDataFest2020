using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Cosmos.Utility
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Azure Data Fest 2020 Attendees!");
            string endpoint = "'your-cosmosdb-host'";
            string authKey = "'your-unique-key-from-cosmosdb'";
            
            try
            {
                using (CosmosClient client = new CosmosClient(endpoint, authKey))
                {
                    await client.ReadAccountAsync();
                }
                Console.WriteLine();
                Console.WriteLine("Read account operation succeeded.");
                 Console.WriteLine();
            }
            catch(CosmosException cosex)
            {
                Console.WriteLine("Cosmos Exception Message: {0}", cosex.ToString());
            }
            catch(Exception ex)
            {
                Exception baseException = ex.GetBaseException();
                Console.WriteLine("Base Exception: {0}, Message: {1}", ex.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }

        }
    }
}
