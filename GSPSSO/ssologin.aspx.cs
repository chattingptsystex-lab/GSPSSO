using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI;

namespace GSPSSO
{
    public partial class ssologin : Page
    {
        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await HandleCallbackAsync();
            }
        }

        private async Task HandleCallbackAsync()
        {
            string code = Request.QueryString["code"];
            string state = Request.QueryString["state"];
            string sessionState = Request.QueryString["session_state"];
            string iss = Request.QueryString["iss"];

            if (string.IsNullOrWhiteSpace(code))
            {
                Response.Write("缺少授權碼 code");
                return;
            }

            string expectedState = Session["oidc_state"] as string;

            if (!string.IsNullOrWhiteSpace(expectedState) &&
                !string.Equals(expectedState, state, StringComparison.Ordinal))
            {
                Response.Write("state 驗證失敗");
                return;
            }

            TokenResponse tokenResponse = await ExchangeCodeForTokenAsync(code);

            if (tokenResponse == null)
            {
                Response.Write("呼叫 Token 端點失敗，請重新登入");
                return;
            }

            if (!string.IsNullOrWhiteSpace(tokenResponse.IdToken))
            {
                Session["Gsp_IdToken"] = tokenResponse.IdToken;
            }

            if (!string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                Session["Gsp_AccessToken"] = tokenResponse.AccessToken;
            }

            Session["Gsp_SessionState"] = sessionState;
            Session["Gsp_Issuer"] = iss;

            Response.Redirect("~/Default.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private async Task<TokenResponse> ExchangeCodeForTokenAsync(string code)
        {
            string authority = ConfigurationManager.AppSettings["GspSso.Authority"];
            string clientId = ConfigurationManager.AppSettings["GspSso.ClientId"];
            string clientSecret = ConfigurationManager.AppSettings["GspSso.ClientSecret"];
            string redirectUri = ConfigurationManager.AppSettings["GspSso.RedirectUri"];

            string tokenEndpoint = authority.TrimEnd('/') + "/protocol/openid-connect/token";

            using (var client = new HttpClient())
            {
                var form = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri },
                    { "client_id", clientId },
                    { "client_secret", clientSecret }
                };

                using (var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form)))
                {
                    string body = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Response.Write("Token error: " + response.StatusCode + "<br/>");
                        Response.Write(Server.HtmlEncode(body));
                        return null;
                    }

                    return JsonConvert.DeserializeObject<TokenResponse>(body);
                }
            }
        }
    }

    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }
    }
}