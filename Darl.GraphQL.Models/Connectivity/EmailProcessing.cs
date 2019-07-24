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


        public async Task<List<Contact>> Mailshot(string userId, string collateral, string subject, string sendfrom, string filter, bool test)
        {
            var contacts = await _connectivity.GetContacts();
            var coll = await _connectivity.GetCollateral(userId, collateral);
            var tp = new TextProcess();
            if (!string.IsNullOrEmpty(filter))
            {
                contacts = contacts.Where(a => a.Sector == filter).ToList();
            }
            foreach (var c in contacts)
            {
                if (!test)
                {
                    await SendEmail(tp.Parse(coll, new Dictionary<string, string> { { "FirstName", c.FirstName } }), subject, sendfrom, c.Email);
                }
            }
            return contacts;
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
