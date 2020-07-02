using System;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using System.Globalization;

namespace Cosmos.Utility.Crud
{
    public class Item
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Message { get; set; }
        public string Category { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public bool IsChecked { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class Program
    {
        private string EndpointUrl = "https://localhost:8081";

        private string PrimaryKey = "your-unique-key-from-cosmosdb";

        private CosmosClient cosmosClient;

        private Database database;

        private Container container;

        private string databaseId = "TodoDatabase";
        private string containerId = "MyItems";

        public static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Beginning operations...\n");
                Program p = new Program();
                await p.GetStartedDemoAsync();

            }
            catch (CosmosException ce)
            {
                Exception baseException = ce.GetBaseException();
                Console.WriteLine("Cosmos Exception {0} occurred: {1}", ce.StatusCode, ce);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Base Exception: {0}", ex);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }



        private async Task CreateDatabaseAsync()
        {
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }

        private async Task CreateContainerAsync()
        {
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/Category");
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }


        private async Task AddItemsToContainerAsync(string categoryName)
        {
            // Create a Item object for the category with stamp suffix
            var stamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            Item categoryItemTodo = new Item
            {
                Id = $"message-{stamp}",
                Message = $"Check daily calendar and meeting agenda-{stamp}",
                Category = categoryName,
                CreateDate = DateTime.UtcNow,
                IsChecked = false
            };

            try
            {
                // Create an item in the container. 
                // We provide the value of the partition key for this item, which is "Category".
                ItemResponse<Item> itemResponse = await this.container.CreateItemAsync<Item>(categoryItemTodo, new PartitionKey(categoryItemTodo.Category));
                // Note that after creating the item, we can access the body of the item with the Resource property of the ItemResponse. 
                // We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", itemResponse.Resource.Id, itemResponse.RequestCharge);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                Console.WriteLine("Item in database with id: {0} already exists\n", categoryItemTodo.Id);
            }
        }


        private async Task QueryItemsAsync(string category)
        {
            var sqlQueryText = $"SELECT * FROM c WHERE c.Category = '{category}'";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Item> queryResultSetIterator = this.container.GetItemQueryIterator<Item>(queryDefinition);

            List<Item> itemList = new List<Item>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Item> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Item singleItem in currentResultSet)
                {
                    itemList.Add(singleItem);
                    Console.WriteLine("\tRead {0}\n", singleItem);
                }
            }
        }


        private async Task DeleteDatabaseAndCleanupAsync()
        {
            DatabaseResponse databaseResourceResponse = await this.database.DeleteAsync();
            // wait this.cosmosClient.Databases["Actual-Database-Name"].DeleteAsync();

            Console.WriteLine("Deleted Database: {0}\n", this.databaseId);
            this.cosmosClient.Dispose();
        }


        public async Task GetStartedDemoAsync()
        {
            int number = GetTaskNumbers();

            this.cosmosClient = new CosmosClient(EndpointUrl, PrimaryKey);
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            string category = "Yearly-Task";

            for (int i = 0; i < number; i++)
            {
                await this.AddItemsToContainerAsync(category);
            }

            await this.QueryItemsAsync(category);
        }

        private static int GetTaskNumbers()
        {
            Console.Write("Enter a valid integer task number:");
            var taskNumber = Console.ReadLine();
            int number;
            if (!Int32.TryParse(taskNumber, out number))
            {
                Console.Write("Non valid integere task number was eneterd, using default 5.");
                number = 5;
            }

            return number;
        }
    }
}
