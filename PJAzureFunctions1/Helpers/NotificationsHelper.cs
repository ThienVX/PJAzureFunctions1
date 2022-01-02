using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;
using PJAzureFunctions1.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PJAzureFunctions1.Helpers
{
    public static class NotificationsHelper
    {

        //Notification Hub settings
        public static string ConnectionString = Settings.NotificationAccessSignature;
        public static string NotificationHubPath = Settings.NotificationHubName;

        // Initialize the Notification Hub
        static NotificationHubClient hub = NotificationHubClient.CreateClientFromConnectionString(ConnectionString, NotificationHubPath);

        /// <summary>
        /// Receive the information to register a new mobile device in the Azure Notification Hub
        /// </summary>
        /// <param name="deviceUpdate">The device information</param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task RegisterDevice(DeviceInstallation deviceUpdate, ILogger log)
        {
            Installation installation = new Installation();
            installation.InstallationId = deviceUpdate.InstallationId;
            installation.PushChannel = deviceUpdate.PushChannel;
            switch (deviceUpdate.Platform)
            {
                case "apns":
                    installation.Platform = NotificationPlatform.Apns;
                    break;
                case "fcm":
                    installation.Platform = NotificationPlatform.Fcm;
                    break;
                default:
                    throw new Exception("Invalid Channel");
            }
            installation.Tags = new List<string>();
            await hub.CreateOrUpdateInstallationAsync(installation);
            log.LogInformation("Device was registered");
        }


        /// <summary>
        /// Register a device to receive specific notifications related to a report
        /// </summary>
        /// <param name="installationId">Identifier of the device</param>
        /// <param name="requestId">The report identifier</param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task AddToRequest(string installationId, string requestId, ILogger log)
        {
            await AddTag(installationId, $"requestId:{requestId}", log);
            log.LogInformation($"Device was registered in the request {requestId}");
        }


        /// <summary>
        /// Remove a device from the notificartions related to a report
        /// </summary>
        /// <param name="installationId">Identifier of the device</param>
        /// <param name="requestId">The report identifier</param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task RemoveFromRequest(string installationId, string requestId, ILogger log)
        {
            await RemoveTag(installationId, $"requestId:{requestId}", log);
            log.LogInformation($"Device was removed from the request {requestId}");
        }



        /// <summary>
        /// Remove a device from an specific tag in the Azure Notification Hub
        /// </summary>
        /// <param name="installationId">The device identifier</param>
        /// <param name="tag">The Azure Notification Hub Tag</param>
        /// <param name="log"></param>
        /// <returns></returns>
        private static async Task RemoveTag(string installationId, string tag, ILogger log)
        {
            try
            {
                Installation installation = await hub.GetInstallationAsync(installationId);
                if (installation.Tags == null)
                {
                    if (installation.Tags.Contains(tag))
                        installation.Tags.Remove(tag);
                    await hub.CreateOrUpdateInstallationAsync(installation);
                }
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
            }
        }


        /// <summary>
        /// Add a device to an specific tag in the Azure Notification Hub
        /// </summary>
        /// <param name="installationId">The device identifier</param>
        /// <param name="tag">The Azure Notification Hub Tag</param>
        /// <param name="log"></param>
        /// <returns></returns>
        private static async Task AddTag(string installationId, string newTag, ILogger log)
        {
            try
            {
                Installation installation = await hub.GetInstallationAsync(installationId);
                if (installation.Tags == null)
                    installation.Tags = new List<string>();
                installation.Tags.Add(newTag);
                await hub.CreateOrUpdateInstallationAsync(installation);
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
            }
        }


        /// <summary>
        /// Remove a device from the Azure Notification Hub
        /// </summary>
        /// <param name="installationId">The device identifier</param>
        /// <returns></returns>
        public static async Task RemoveDevice(string installationId)
        {
            await hub.DeleteInstallationAsync(installationId);
        }


        /// <summary>
        /// Send a push notification about an specific report id. At this moment is only supported 1 device per report
        /// </summary>
        /// <param name="text">The notification message that will be displayed by the mobile device</param>
        /// <param name="requestId">the report identifier</param>
        /// <param name="installationId">The device identifier</param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task SendNotification(string text, string requestId, string installationId, ILogger log)
        {
            try
            {
                Installation installation = await hub.GetInstallationAsync(installationId);

                if (installation.Platform == NotificationPlatform.Fcm)
                {
                    var json = string.Format("{{\"data\":{{\"message\":\"{0}\"}}}}", text);
                    await hub.SendFcmNativeNotificationAsync(json, $"requestid:{requestId}");
                    log.LogInformation($"FCM notification was sent");
                }
                else
                {
                    var json = string.Format("{{\"aps\":{{\"alert\":\" {0}\"}}}}", text);
                    await hub.SendAppleNativeNotificationAsync(json, $"requestid:{requestId}");
                    log.LogInformation($"Apple notification was sent");
                }
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
            }
        }


        /// <summary>
        /// Send a notification to all Apple and Firebase devices in the notification hub
        /// </summary>
        /// <param name="text">The notification message that will be displayed by the mobile device </param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task SendBroadcastNotification(string text, ILogger log)
        {
            try
            {
                var json = string.Format("{{\"data\":{{\"message\":\"{0}\"}}}}", text);
                await hub.SendFcmNativeNotificationAsync(json);
                log.LogInformation($"FCM notification was sent");

            }
            catch (Exception ex) //If there aren't FCM devices registered in the hub, it throws an error
            {
                log.LogInformation(ex.Message);
            }


            try
            {
                var json = string.Format("{{\"aps\":{{\"alert\":\" {0}\"}}}}", text);
                await hub.SendAppleNativeNotificationAsync(json);
                log.LogInformation($"Apple notification was sent");
            }
            catch (Exception ex) //If there aren't Apple devices registered in the hub, it throws an error
            {
                log.LogInformation(ex.Message);
            }
        }
    }
}
