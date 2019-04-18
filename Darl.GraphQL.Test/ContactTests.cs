using Darl.Connectivity.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class ContactTests
    {
        string connectionString = "DefaultEndpointsProtocol=https;AccountName=darlai;AccountKey=errnwefiVeXcDr0aKbHDxXjblOQhwFwHkeG4qR4caChkABnzp9MNeBBX0FP1jc4DnXPGztI67pbEBXDqA1dPCw==";
        string userId = "8c663676-a7dc-4561-af3d-89b38555837d";

        [TestMethod]
        public async Task CopyDarlContacts()
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var client = storageAccount.CreateCloudTableClient();
            var contacts = client.GetTableReference("contacts");
            var list = new List<TableContacts>();
            TableQuery<TableContacts> defQuery = new TableQuery<TableContacts>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));
            TableContinuationToken continuationToken = null;
            do
            {
                var defs = await contacts.ExecuteQuerySegmentedAsync(defQuery, continuationToken);
                list.AddRange(defs);
                continuationToken = defs.ContinuationToken;
            } while (continuationToken != null);

        }
    }
}
