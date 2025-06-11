using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace KpasteCli.Logic
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

        public class GetPasteResponseDto
        {
            [JsonProperty("result")]
            public string Result { get; set; }

            [JsonProperty("data")]
            public GetPasteResponseDataDto Data { get; set; }
        }

        public class GetPasteResponseDataDto
        {
            [JsonProperty("id")]
            public string Id { get; set; }
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
            [JsonProperty("created_at")]
            public Int64 CreatedAt { get; set; }
            [JsonProperty("updated_at")]
            public Int64 UpdatedAt { get; set; }
            [JsonProperty("expirated_at")]
            public Int64 ExpiredAt { get; set; }
            [JsonProperty("deleted_at")]
            public Int64? DeletedAt { get; set; }
        }

        public NewPasteResponseDto SendPaste(NewPasteRequestDto dto)
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

        public GetPasteResponseDto ReceivePaste(string pasteId)
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
                RequestUri = new Uri("https://welcome.infomaniak.com/api/components/paste/" + pasteId),
                Method = HttpMethod.Get,
                Headers = { { "X-XSRF-TOKEN", [cookieContainer.GetCookies(new Uri("https://welcome.infomaniak.com/api/components/paste"))["SHOP-XSRF-TOKEN"].Value] } }
            });

            Span<byte> contentBytes = new byte[8 * 1024];
            var read = response.Content.ReadAsStream().Read(contentBytes);

            var result = JsonConvert.DeserializeObject<GetPasteResponseDto>(Encoding.UTF8.GetString(contentBytes));

            return result;
        }
    }
}
