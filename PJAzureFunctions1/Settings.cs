using System;
using System.Collections.Generic;
using System.Text;

namespace PJAzureFunctions1
{
    public class Settings
    {
        public static string AzureWebJobsStorage = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        public static string FaceAPIKey = Environment.GetEnvironmentVariable("Vision_API_Subscription_Key");
        public static string Zone = Environment.GetEnvironmentVariable("Vision_API_Zone");


        public static string NotificationAccessSignature = Environment.GetEnvironmentVariable("NotificationHub_Access_Signature");
        public static string NotificationHubName = Environment.GetEnvironmentVariable("NotificationHub_Name");


    }
}
