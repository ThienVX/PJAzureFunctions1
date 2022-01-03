using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using PJAzureFunctions1.Domain;
using PJAzureFunctions1.Helpers;
using System.Net;

namespace PJAzureFunctions1
{
    public static class Function1
    {
        [FunctionName("DeviceNotificationsRegistration")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "devicenotificationsregistrations/")] HttpRequestMessage req, ILogger log)
        {
            try
            {
                log.LogInformation("New device registration incoming");
                var content = await req.Content.ReadAsStringAsync();
                DeviceInstallation deviceUpdate = await req.Content.ReadAsAsync<DeviceInstallation>();
                await NotificationsHelper.RegisterDevice(deviceUpdate, log);
                log.LogInformation("New device registered");
                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                log.LogInformation($"Error during device registration: {ex.Message}");
            }
            return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error during device registration");
        }
    }
}