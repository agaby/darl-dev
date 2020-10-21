using Darl.GraphQL.Models.Models;
using Datl.Language;
using GraphQL;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class EmailProcessing : IEmailProcessing
    {

        private IConfiguration _config;
        IConnectivity _connectivity;
        private CloudQueue queue;
        private IDistributedCache _cache;
        private static TimeSpan cacheExpiration = new TimeSpan(1, 0, 0, 0);


        public EmailProcessing(IConfiguration config, IConnectivity connectivity, IDistributedCache cache, IConnectivity conn)
        {
            _config = config;
            _connectivity = connectivity;
            var csa = CloudStorageAccount.Parse(_config["AppSettings:StorageConnectionString"]);
            queue = csa.CreateCloudQueueClient().GetQueueReference("support-messages");
            _cache = cache;
        }

        public async Task<string> InviteUser(string userId, string email)
        {
            if (await _connectivity.CheckEmail(email))
            {
                var user = await _connectivity.GetUserById(userId);
                await _cache.SetStringAsync($"AddUserRequest_{email}", userId, new DistributedCacheEntryOptions { SlidingExpiration = cacheExpiration });
                return await SendEmail($"You are invited to join the account of {user.InvoiceOrganization ?? user.InvoiceName ?? user.InvoiceEmail}.\n Go to https://darl.dev and register using this email address: {email}.\nThe system will recognize you and add you to that account.\nThis invitation expires in 24 hours.\n\n" +
                    $"Darl.dev support.", "Invitation to join a Darl.dev account", "support@darl.ai",email);
            }
            throw new ExecutionError($"{email} is suspect or invalid");
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
