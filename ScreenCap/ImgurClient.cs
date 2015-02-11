using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCap
{    
    class ImgurClient
    {
        
        public const string APIUrl = "https://api.imgur.com/3/";

        private HttpClient httpClient = new HttpClient();

        static ImgurClient()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 20;
        }

        public ImgurClient()
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Client-ID " + ImgurClient.ClientId);
        }

        public async Task<ImgurImageResponse> UploadImage(MemoryStream image)
        {
            ImgurImageResponse imgurResponse = null;
            try
            {
                using (var formContent = new MultipartFormDataContent())
                {
                    formContent.Add(new StringContent(Convert.ToBase64String(image.ToArray())), "image");
                    using (var result = await this.httpClient.PostAsync(ImgurClient.APIUrl + "image", formContent))
                    {
                        imgurResponse = await result.Content.ReadAsAsync<ImgurImageResponse>();                        
                    }
                }
            }
            catch(Exception e)
            {
                Trace.TraceError("Error uploading image: " + e.ToString());
            }

            return imgurResponse;
        }
    }
}
