using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace DOCUMENTSERVICE.Connections
{
    public class SharePointConnectionConfig
    {
        public static ClientContext SPClientContext { get; set; }
        public static Web SPWeb { get; set; }
        public static string SPErrorMsg { get; set; }

        public static bool Connect(string SPURL, string SPUserName, string SPPassWord, string SPDomainName)
        {

            bool bConnected = false;

            try
            {

                SPClientContext = new ClientContext(SPURL);
                SPClientContext.RequestTimeout = 36000000;
                var passWord = new SecureString();
                foreach (char c in SPPassWord.ToCharArray())
                {
                    passWord.AppendChar(c);
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                SPClientContext.Credentials = new SharePointOnlineCredentials(SPUserName, passWord);
                SPClientContext.AuthenticationMode = ClientAuthenticationMode.Default;
                SPWeb = SPClientContext.Web;
                SPClientContext.Load(SPWeb);
                SPClientContext.ExecuteQuery();
                bConnected = true;

            }

            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                bConnected = false;
                SPErrorMsg = ex.Message;
            }

            return bConnected;

        }

    }
}