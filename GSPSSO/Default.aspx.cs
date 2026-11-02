using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace GSPSSO
{
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string accessToken = Session["Gsp_AccessToken"] as string;
            string idToken = Session["Gsp_IdToken"] as string;

            // 尚未登入：Default.aspx 作為登入入口，直接導向 SSO 登入
            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(idToken))
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(() => SignInPermissionApiAsync(idToken)));
            }
        }

        private async Task SignInPermissionApiAsync(string idToken)
        {
            string account = GetAccountFromIdToken(idToken, out string name, out string email);

            if (string.IsNullOrWhiteSpace(account))
            {
                litResult.Text = "SSO 登入成功，但 idToken 中找不到 account";
                return;
            }

            // 同一個 Session 已登入過就不重複呼叫
            if (Session["Perm_Token"] == null || !string.Equals(Session["Perm_Account"] as string, account, StringComparison.Ordinal))
            {
                string baseUrl = ConfigurationManager.AppSettings["PermissionApi.BaseUrl"];
                string apiKey = ConfigurationManager.AppSettings["PermissionApi.ApiKey"];

                var payload = JsonConvert.SerializeObject(new { Account = account, UserName = name, Email = email });

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Post, baseUrl.TrimEnd('/') + "/Auth/sso-login"))
                {
                    request.Headers.Add("X-Api-Key", apiKey);
                    request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                    using (var response = await client.SendAsync(request))
                    {
                        string body = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            litResult.Text = "PermissionApi 登入失敗：" + (int)response.StatusCode + " " +
                                             HttpUtility.HtmlEncode(body) + " " + apiKey + " " + payload;
                            return;
                        }

                        var json = JObject.Parse(body);
                        Session["Perm_Token"] = (string)json["token"];
                        Session["Perm_Account"] = account;
                        Session["Perm_IsNewUser"] = (bool?)json["isNewUser"] ?? false;
                    }
                }
            }

            string admUrl = ConfigurationManager.AppSettings["AdmUser.Url"];
            string token = Session["Perm_Token"] as string;
            Response.Redirect(admUrl + "#token=" + HttpUtility.UrlEncode(token), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private static string GetAccountFromIdToken(string idToken, out string name, out string email)
        {
            name = null;
            email = null;

            string[] parts = idToken.Split('.');
            if (parts.Length < 2) return null;

            string b64 = parts[1].Replace('-', '+').Replace('_', '/');
            b64 = b64.PadRight(b64.Length + (4 - b64.Length % 4) % 4, '=');

            var claims = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(b64)));

            name = (string)claims["name"];
            email = (string)claims["email"];

            return (string)claims["account"]
                ?? (string)claims["preferred_username"]
                ?? (string)claims["sub"];
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Logout.aspx");
        }
    }
}
