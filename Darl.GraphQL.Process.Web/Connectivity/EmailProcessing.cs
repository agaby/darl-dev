/// <summary>
/// EmailProcessing.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Azure.Storage.Queues;
using Darl.GraphQL.Models.Models;
using Datl.Language;
using GraphQL;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class EmailProcessing : IEmailProcessing
    {

        private readonly IConfiguration _config;
        private readonly IKGTranslation _connectivity;
        private readonly ICheckEmail _checkEmail;
        private readonly QueueClient queue;
        private readonly IDistributedCache _cache;
        private static readonly TimeSpan cacheExpiration = new TimeSpan(1, 0, 0, 0);


        public EmailProcessing(IConfiguration config, IKGTranslation connectivity, IDistributedCache cache, IConnectivity conn)
        {
            _config = config;
            _connectivity = connectivity;
            queue = new QueueClient(_config["AppSettings:StorageConnectionString"], "support-messages", new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            queue.CreateIfNotExists();
            _cache = cache;
        }

        public async Task<string> InviteUser(string userId, string email)
        {
            if (await _checkEmail.CheckEmail(email))
            {
                var user = await _connectivity.GetUserById(userId);
                var accountName = user.InvoiceOrganization;
                if (string.IsNullOrEmpty(accountName))
                    accountName = user.InvoiceName;
                if (string.IsNullOrEmpty(accountName))
                    accountName = user.InvoiceEmail;
                await _cache.SetStringAsync($"AddUserRequest_{email}", userId, new DistributedCacheEntryOptions { SlidingExpiration = cacheExpiration });
                return await SendEmail($"You are invited to join the account of {accountName}.\r\n Go to https://darl.dev and register using this email address: {email}.\r\nThe system will recognize you and add you to that account.\r\nThis invitation expires in 24 hours.\r\n\r\n" +
                    $"Darl.dev support.", "Invitation to join a Darl.dev account", "support@darl.ai", email);
            }
            throw new ExecutionError($"{email} is suspect or invalid");
        }

        public async Task<int> Mailshot(string content, string subject, string sendfrom, bool test)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ExecutionError($"Collateral is not present or empty");
            }
            var tp = new TextProcess();
            int count = 1;
            if (test) //send one email back with the generated text
            {
                var body = tp.Parse(content, new Dictionary<string, string> { { "InvoiceName", "Andy" } }); //make the insertion dictionary programmable
                await SendEmail(body, subject, sendfrom, sendfrom);
            }
            else
            {
                var collection = await _connectivity.GetContacts();
                count = collection.Count;
                foreach (var c in collection)
                {
                    var body = tp.Parse(content, new Dictionary<string, string> { { "InvoiceName", c.FirstName } }); //make the insertion dictionary programmable
                    await SendEmail(body, subject, sendfrom, c.Email);
                }
            }
            return count;
        }

        public async Task<string> SendEmail(string body, string subject, string sendfrom, string emailAddress)
        {
            var smm = new SupportMailMessage { from = sendfrom, subject = subject, to = emailAddress, content = body };
            //add to queue
            await queue.SendMessageAsync(JsonConvert.SerializeObject(smm));
            return smm.content;
        }
    }
}
