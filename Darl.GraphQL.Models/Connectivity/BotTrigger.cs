using Darl.GraphQL.Models.Models;
using Darl.Lineage.Bot;
using DarlCommon;
using Datl.Language;
using GraphQL.Client;
using GraphQL.Common.Request;
using Microsoft.ApplicationInsights;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class BotTrigger : ITrigger
    {

        private IConnectivity _connectivity;

        public BotTrigger(IConnectivity connectivity)
        {
            _connectivity = connectivity;
        }

        private TelemetryClient telemetry = new TelemetryClient();


        public async Task TriggerEvent(List<DarlVar> values, RuleForm rf, string user, ServiceConnectivity service)
        {
            if (rf.trigger != null)
            {
                try
                {
                    var sendEmail = GetBoolValue(rf.trigger.sendEmailSource, rf.trigger.sendEmail, values);
                    var sendBug = GetBoolValue(rf.trigger.sendBugSource, rf.trigger.sendBug, values);
                    var sendGraphQL = GetBoolValue(rf.trigger.graphqlDataSource, rf.trigger.graphqlData, values);
                    var graphQLUrl = GetStringValue(rf.trigger.graphqlDataSource, rf.trigger.graphqlName, values);
                    var graphQLData = GetStringValue(rf.trigger.graphqlDataSource, rf.trigger.graphqlData, values);
                    var emailAddress = GetStringValue(rf.trigger.addressSource, rf.trigger.addressText, values);
                    var emailSubject = GetStringValue(rf.trigger.subjectSource, rf.trigger.subjectText, values);
                    var emailBody = GetStringValue(rf.trigger.bodySource, rf.trigger.bodyText, values);
                    var attachment = GetBoolValue(rf.trigger.sendAttachmentSource, rf.trigger.sendAttachment, values);
                    var postData = GetBoolValue(rf.trigger.postDataSource, rf.trigger.postData, values);
                    if (sendEmail && !string.IsNullOrEmpty(emailAddress))
                    {
                        var sg = service.sendgridcred;
                        if (sg != null) //user has set up account
                        {
                            var client = new SendGridClient(sg.SendGridAPIKey);
                            var from = new EmailAddress(rf.trigger.emailFrom);
                            var to = new EmailAddress(emailAddress);
                            var msg = MailHelper.CreateSingleEmail(from, to, emailSubject, emailBody, "");
                            if (attachment && rf.trigger.attachmentName != null)
                            {
                                try
                                {
                                    var d = await _connectivity.GetDocument(user, rf.trigger.attachmentUri);
                                    using (MemoryStream ms = new MemoryStream(d.content)) //possibly we should leave msg to delete stream?
                                    {

                                        ms.Position = 0;
                                        //pipe it 
                                        var word = new WordProcessGem();
                                        var res = new Dictionary<string, string>();
                                        foreach (var k in values)
                                        {
                                            res.Add(k.name, k.Value);
                                        }
                                        msg.AddAttachment(rf.trigger.attachmentName, ConvertToBase64(word.Parse(ms, res)), "docx", "attachment");
                                    }

                                }
                                catch (Exception ex)
                                {
                                    telemetry.TrackException(ex);
                                    return;
                                }
                            }
                            await client.SendEmailAsync(msg);
                        }


                    }
                    if (postData)
                    {

                    }
                    if (sendBug)
                    {
                        await _connectivity.CreateSupportRequest(GetName(values, emailAddress), emailAddress, $"{emailSubject}: {emailBody}", "GraphQL trial site");
                    }
                    if(sendGraphQL)
                    {
                        GraphQLClient client = new GraphQLClient(graphQLUrl);
                        var req = new GraphQLRequest() {Query = graphQLData };
                        var resp = await client.PostAsync(req);
                    }
                }
                catch (Exception ex)
                {
                    telemetry.TrackException(ex);
                }
            }
        }

        public static string ConvertToBase64(Stream stream)
        {
            byte[] inArray = new Byte[(int)stream.Length];

            long arrayLength = (long)((4.0d / 3.0d) * stream.Length);

            // If array length is not divisible by 4, go up to the next
            // multiple of 4.
            if (arrayLength % 4 != 0)
            {
                arrayLength += 4 - arrayLength % 4;
            }

            char[] outArray = new Char[arrayLength];
            stream.Read(inArray, 0, (int)stream.Length);
            Convert.ToBase64CharArray(inArray, 0, inArray.Length, outArray, 0);
            return new string(outArray);
        }

        private static string GetStringValue(SourceType source, string text, List<DarlVar> dict)
        {
            if (source == SourceType.results)
            {
                if (dict.Where(a => a.name == text).Any())
                {
                    return dict.Where(a => a.name == text).First().Value;
                }
            }
            else
            {
                return text;
            }
            return string.Empty;
        }

        private static bool GetBoolValue(SourceType source, string text, List<DarlVar> dict)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            if (source == SourceType.results)
            {
                if (text.Contains("."))//look for category
                {
                    var comp = text.Trim().Split('.');
                    if (dict.Where(a => a.name == comp[0]).Any())
                    {
                        return dict.Where(a => a.name == comp[0]).First().Value == comp[1];
                    }
                }
                else //look for presence
                {
                    if (dict.Where(a => a.name == text.Trim()).Any())
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (text.Trim().ToLower() == "true")
                {
                    return true;
                }
            }
            return false;
        }

        private static string GetName(List<DarlVar> dict, string emailAddress)
        {
            if (dict.Where(a => a.name == "name").Any())
                return dict.Where(a => a.name == "name").First().Value;
            return emailAddress.Substring(0, emailAddress.IndexOf('@'));
        }

    }


}
