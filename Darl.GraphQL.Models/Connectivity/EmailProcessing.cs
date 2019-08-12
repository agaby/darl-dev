using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Models;
using Datl.Language;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Darl.GraphQL.Models.Connectivity
{
    public class EmailProcessing : IEmailProcessing
    {

        private IOptions<AppSettings> _opt;
        IConnectivity _connectivity;
        private CloudQueue queue;
        public EmailProcessing(IOptions<AppSettings> optionsAccessor, IConnectivity connectivity)
        {
            _opt = optionsAccessor;
            _connectivity = connectivity;
            var csa = CloudStorageAccount.Parse(_opt.Value.StorageConnectionString);
            queue = csa.CreateCloudQueueClient().GetQueueReference("support-messages");
        }


        public async Task<int> Mailshot(string userId, string collateral, string subject, string sendfrom, string filter, bool test)
        {
            var coll = await _connectivity.GetCollateral(userId, collateral);

            var collection = _connectivity.db.GetCollection<Contact>("contact");
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
            return count;
        }

        public async Task<string> SendEmail(string body, string subject, string sendfrom, string emailAddress)
        {
            var smm = new SupportMailMessage { from = sendfrom, subject = subject, to = emailAddress, content = body  };
            //add to queue
            queue.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(smm)));
            return smm.content;
        }
    }
}
