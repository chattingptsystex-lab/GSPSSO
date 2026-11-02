using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;

namespace GSPSSO
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DoLogin();
            }
        }

        private void DoLogin()
        {
            string authority = ConfigurationManager.AppSettings["GspSso.Authority"];
            string clientId = ConfigurationManager.AppSettings["GspSso.ClientId"];
            string redirectUri = ConfigurationManager.AppSettings["GspSso.RedirectUri"];

            if (string.IsNullOrWhiteSpace(authority))
            {
                Response.Write("GspSso.Authority 沒有讀到");
                return;
            }

            string state = CreateBase64UrlRandomString(32);
            string nonce = CreateBase64UrlRandomString(32);

            Session["oidc_state"] = state;
            Session["oidc_nonce"] = nonce;

            string authEndpoint = authority.TrimEnd('/') + "/protocol/openid-connect/auth";

            string loginUrl =
                authEndpoint +
                "?response_type=code" +
                "&client_id=" + HttpUtility.UrlEncode(clientId) +
                "&scope=" + HttpUtility.UrlEncode("openid") +
                "&state=" + HttpUtility.UrlEncode(state) +
                "&redirect_uri=" + HttpUtility.UrlEncode(redirectUri) +
                "&nonce=" + HttpUtility.UrlEncode(nonce);

            Response.Redirect(loginUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private static string CreateBase64UrlRandomString(int byteLength)
        {
            byte[] bytes = new byte[byteLength];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}