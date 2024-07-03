using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace kpaste_cli.Logic
{
    public class Paste
    {

        private HttpClientHandler httpClientHandler;
        private HttpClient httpClient;
        private CookieContainer cookieContainer;

        public Paste()
        {
            this.cookieContainer = new CookieContainer();
            this.httpClientHandler = new HttpClientHandler() { CookieContainer = this.cookieContainer };
            this.httpClient = new HttpClient(this.httpClientHandler);
        }

        public class NewPasteRequestDto
        {
            [JsonProperty("data")]
            public string Data { get; set; }

            [JsonProperty("vector")]
            public string Vector { get; set; }

            [JsonProperty("salt")]
            public string Salt { get; set; }

            [JsonProperty("burn")]
            public bool Burn { get; set; }

            [JsonProperty("validity")]
            public string Validity { get; set; }

            [JsonProperty("password")]
            public bool Password { get; set; }
        }

        public class NewPasteResponseDto
        {
            [JsonProperty("result")]
            public string Result { get; set; }

            [JsonProperty("data")]
            public string Data { get; set; }
        }

        public NewPasteResponseDto sendPaste(NewPasteRequestDto dto)
        {
            var response = httpClient.Send(new HttpRequestMessage()
            {
                RequestUri = new Uri("https://welcome.infomaniak.com/api/components/profile/me?with=current_group,user,groups,products"),
                Method = HttpMethod.Get
            });

            response = null;

            response = httpClient.Send(new HttpRequestMessage()
            {
                RequestUri = new Uri("https://welcome.infomaniak.com/api/web-components/1/init?project=kpaste"),
                Method = HttpMethod.Get
            });

            response = null;

            response = httpClient.Send(new HttpRequestMessage()
            {
                RequestUri = new Uri("https://welcome.infomaniak.com/api/components/paste"),
                Method = HttpMethod.Post,
                Headers = { {"X-XSRF-TOKEN", [cookieContainer.GetCookies(new Uri("https://welcome.infomaniak.com/api/components/paste"))["SHOP-XSRF-TOKEN"].Value] } },
                Content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json"),
            });

            Span<byte> contentBytes = new byte[8*1024];
            var read = response.Content.ReadAsStream().Read(contentBytes);

            var result = JsonConvert.DeserializeObject<NewPasteResponseDto>(Encoding.UTF8.GetString(contentBytes));

            return result;
        }
    }
}
