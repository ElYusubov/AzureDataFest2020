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
            
            CosmosClient client = new CosmosClient(endpoint, authKey);
            await client.ReadAccountAsync();


            Console.WriteLine("End of demo, press any key to exit.");
            Console.ReadKey();
        }
    }
}
