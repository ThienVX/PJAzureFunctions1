using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using PJAzureFunctions1.Helpers;

namespace PJAzureFunctions1
{
    public static class PersonRegistration
    {
        private static FaceClientHelper client_face = new FaceClientHelper();

        [FunctionName("PersonRegistration")]
        public static async Task Run([BlobTrigger("images/{name}.{extension}")] CloudBlockBlob blobImage, string name, string extension, ILogger log)
        {

            log.LogInformation($"Image: {name}.{extension}");
            string notificationMessage = "Error";
            string json = string.Empty;
            var deviceId = blobImage.Metadata["deviceid"];


            try
            {

                await NotificationsHelper.AddToRequest(deviceId, name, log);
                log.LogInformation($"uri {blobImage.Uri.AbsoluteUri}");
                log.LogInformation($"Zone {Settings.Zone}");

                //determine if image has a face
                List<JObject> list = await client_face.DetectFaces(blobImage.Uri.AbsoluteUri);

                //validate image extension 
                if (blobImage.Properties.ContentType != "image/jpeg")
                {
                    log.LogInformation($"no valid content type for: {name}.{extension}");
                    await blobImage.DeleteAsync();

                    notificationMessage = "Incorrect Image Format";
                    await NotificationsHelper.SendNotification(notificationMessage, name, deviceId, log);
                    return;
                }

                //if image has no faces
                if (list.Count == 0)
                {
                    log.LogInformation($"there are no faces in the image: {name}.{extension}");
                    await blobImage.DeleteAsync();
                    notificationMessage = "The are not faces in the photo";
                    await NotificationsHelper.SendNotification(notificationMessage, name, deviceId, log);
                    return;
                }

                //if image has more than one face
                if (list.Count > 1)
                {
                    log.LogInformation($"multiple faces detected in the image: {name}.{extension}");
                    await blobImage.DeleteAsync();
                    notificationMessage = "Multiple faces detected in the image";
                    await NotificationsHelper.SendNotification(notificationMessage, name, deviceId, log);
                    return;
                }

            }
            catch (Exception ex)
            {
                // await blobImage.DeleteAsync();

                log.LogInformation($"Error in file: {name}.{extension} - {ex.Message}");
                notificationMessage = "Error in file registration";
                await NotificationsHelper.SendNotification(notificationMessage, name, deviceId, log);
                return;
            }

            log.LogInformation("person registered successfully");
            notificationMessage = "Person registered successfully";
            await NotificationsHelper.SendNotification(notificationMessage, name, deviceId, log);
        }
    }
}
