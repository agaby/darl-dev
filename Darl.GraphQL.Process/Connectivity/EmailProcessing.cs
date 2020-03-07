using Darl.GraphQL.Models.Models;
using Datl.Language;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class EmailProcessing : IEmailProcessing
    {

        private IConfiguration _config;
        IConnectivity _connectivity;
        private CloudQueue queue;
        public EmailProcessing(IConfiguration config, IConnectivity connectivity)
        {
            _config = config;
            _connectivity = connectivity;
            var csa = CloudStorageAccount.Parse(_config["AppSettings:StorageConnectionString"]);
            queue = csa.CreateCloudQueueClient().GetQueueReference("support-messages");
        }


        public async Task<int> Mailshot(string userId, string collateral, string subject, string sendfrom, string filter, bool test)
        {
            var coll = await _connectivity.GetCollateral(userId, collateral);
            var collection = _connectivity.db.GetCollection<Contact>(CosmosDBConnectivity.contactCollection);
            int count = 0;
            var tp = new TextProcess();
            using (IAsyncCursor<Contact> cursor = await collection.FindAsync(string.IsNullOrEmpty(filter) ? FilterDefinition<Contact>.Empty : Builders<Contact>.Filter.Where(x => x.Sector == filter)))
            {
                while (await cursor.MoveNextAsync())
                {
                    IEnumerable<Contact> batch = cursor.Current;
                    foreach (Contact c in batch)
                    {
                        if (!test)
                        {
                            var body = tp.Parse(coll, new Dictionary<string, string> { { "InvoiceName", c.FirstName } }); //make the insertion dictionary programmable
                            await SendEmail(body, subject, sendfrom, c.Email);
                        }
                        count++;
                    }
                }
            }
            if(test) //send one email back with the generated text
            {
                var body = tp.Parse(coll, new Dictionary<string, string> { { "InvoiceName", "Andy" } }); //make the insertion dictionary programmable
                await SendEmail(body, subject, sendfrom, sendfrom);
            }
            return count;
        }

        public async Task<string> SendEmail(string body, string subject, string sendfrom, string emailAddress)
        {
            var smm = new SupportMailMessage { from = sendfrom, subject = subject, to = emailAddress, content = body  };
            //add to queue
            await queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(smm)));
            return smm.content;
        }
    }
}
