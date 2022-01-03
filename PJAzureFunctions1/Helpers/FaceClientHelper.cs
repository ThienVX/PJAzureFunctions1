using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PJAzureFunctions1.Helpers
{
    class FaceClientHelper
    {
        private string FaceAPIKey = Settings.FaceAPIKey;
        private string Zone = Settings.Zone;

        /// <summary>
        /// Analyze a photo using the Face API
        /// </summary>
        /// <param name="url">The image url</param>
        /// <returns></returns>
        public async Task<List<JObject>> DetectFaces(String url)
        {
            using (var client = new HttpClient())
            {
                var service = $"https://{Zone}.api.cognitive.microsoft.com/face/v1.0/detect";
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", FaceAPIKey);
                byte[] byteData = Encoding.UTF8.GetBytes("{'url':'" + url + "'}");
                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var httpResponse = await client.PostAsync(service, content);

                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        List<JObject> result = JsonConvert.DeserializeObject<List<JObject>>(await httpResponse.Content.ReadAsStringAsync());
                        return result;
                    }
                }
            }
            return null;
        }
    }
}
