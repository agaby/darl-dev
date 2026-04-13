/// </summary>

﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ZeroBounce.Models;

namespace ZeroBounce
{

    public class ZeroBounceAPI
    {
        // "Your API Key";
        private string m_api_key = "";
        public string api_key
        {
            get
            {
                return m_api_key;
            }

            set
            {
                m_api_key = value;
            }
        }
        // "Your IP address";
        private string m_ip_address = "";
        public string ip_address
        {
            get
            {
                return m_ip_address;
            }

            set
            {
                m_ip_address = value;
            }
        }
        private string m_emailToValidate = "";
        public string EmailToValidate
        {
            get
            {
                return m_emailToValidate;
            }

            set
            {
                m_emailToValidate = value;
            }
        }

        private int m_requestTimeOut = 100000;
        public int RequestTimeOut
        {
            get
            {
                return m_requestTimeOut;
            }

            set
            {
                m_requestTimeOut = value;
            }
        }
        private int m_readTimeOut = 100000;
        public int ReadTimeOut
        {
            get
            {
                return m_readTimeOut;
            }

            set
            {
                m_readTimeOut = value;
            }
        }
        public async Task<ZeroBounceResultsModel> ValidateEmailAsync()
        {
            var apiUrl = "";
            var responseString = "";
            var oResults = new ZeroBounceResultsModel();

            try
            {
                apiUrl = "https://api.zerobounce.net/v2/validate?api_key=" + api_key + "&email=" + System.Net.WebUtility.UrlEncode(EmailToValidate) + "&ip_address=" + System.Net.WebUtility.UrlEncode(ip_address);

                // secure SSL / TLS channel for different .Net versions           
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11;
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;


                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)WebRequest.Create(apiUrl);
                request.Timeout = RequestTimeOut;
                request.Method = "GET";
                Console.WriteLine("Input APIKey: " + api_key);


                using (WebResponse response = await request.GetResponseAsync())
                {
                    //                    response.GetResponseStream().ReadTimeout = ReadTimeOut;
                    using (StreamReader ostream = new StreamReader(response.GetResponseStream()))
                    {
                        responseString = ostream.ReadToEnd();
                        oResults = JsonConvert.DeserializeObject<ZeroBounceResultsModel>(responseString);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The operation has timed out")) oResults.sub_status = "timeout_exceeded";
                else oResults.sub_status = "exception_occurred";
                oResults.status = "unknown";
                oResults.domain = EmailToValidate.Substring(EmailToValidate.IndexOf("@") + 1).ToLower();
                oResults.account = EmailToValidate.Substring(0, EmailToValidate.IndexOf("@")).ToLower();
                oResults.address = EmailToValidate.ToLower();
                oResults.error = ex.Message;
            }
            return oResults;
        }
        public async Task<ZeroBounceCreditsModel> GetCreditsAsync()
        {
            var apiUrl = "https://api.zerobounce.net/v2/getcredits?api_key=" + api_key;
            var responseString = "";
            var oResults = new ZeroBounceCreditsModel();
            try
            {
                // secure SSL / TLS channel for different .Net versions            
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11;
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)WebRequest.Create(apiUrl);
                request.Timeout = RequestTimeOut;
                request.Method = "GET";
                Console.WriteLine("APIKey: " + api_key);


                using (WebResponse response = await request.GetResponseAsync())
                {
                    response.GetResponseStream().ReadTimeout = ReadTimeOut;
                    using (StreamReader ostream = new StreamReader(response.GetResponseStream()))
                    {
                        responseString = ostream.ReadToEnd();
                        oResults = JsonConvert.DeserializeObject<ZeroBounceCreditsModel>(responseString);
                    }
                }
            }
            catch (Exception)
            {
                // Add Your Error Logging
            }
            return oResults;
        }
    }
}
