using GSPSSO.Options;
using GSPSSO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GSPSSO.Controllers
{
    public class SSOController : Controller
    {
        private readonly GspSsoService _gspSsoService;
        private readonly GspSsoOptions _gspOptions;
        private readonly HttpClient _httpClient;

        // 支援 DI 的建構子
        public SSOController(GspSsoService gspSsoService, GspSsoOptions gspOptions, HttpClient httpClient = null)
        {
            _gspSsoService = gspSsoService ?? throw new ArgumentNullException(nameof(gspSsoService));
            _gspOptions = gspOptions ?? throw new ArgumentNullException(nameof(gspOptions));
            _httpClient = httpClient ?? new HttpClient();
        }

        // 無 DI 時可用的預設建構子（會從 appSettings 讀設定並建立 HttpClient / Service）
        public SSOController()
        {
            _gspOptions = new GspSsoOptions
            {
                Authority = ConfigurationManager.AppSettings["GspSso:Authority"] ?? string.Empty,
                ClientId = ConfigurationManager.AppSettings["GspSso:ClientId"] ?? string.Empty,
                ClientSecret = ConfigurationManager.AppSettings["GspSso:ClientSecret"] ?? string.Empty,
                RedirectUri = ConfigurationManager.AppSettings["GspSso:RedirectUri"] ?? string.Empty
            };

            _httpClient = new HttpClient();
            _gspSsoService = new GspSsoService(_httpClient);
        }

        [HttpGet]
        [Route("config")]
        public async Task<ActionResult> GetConfig()
        {
            var config = await _gspSsoService.GetOidcConfigurationAsync();
            return Json(config, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("login")]
        public ActionResult Login()
        {
            if (string.IsNullOrWhiteSpace(_gspOptions.Authority))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "GspSso:Authority 沒有讀到");
            }

            string state = GenerateBase64Url(32);
            string nonce = GenerateBase64Url(32);

            Session["oidc_state"] = state;
            Session["oidc_nonce"] = nonce;

            var authEndpoint = $"{_gspOptions.Authority.TrimEnd('/')}/protocol/openid-connect/auth";

            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["response_type"] = "code";
            qs["client_id"] = _gspOptions.ClientId;
            qs["scope"] = "openid";
            qs["state"] = state;
            qs["redirect_uri"] = _gspOptions.RedirectUri;
            qs["nonce"] = nonce;

            var loginUrl = authEndpoint + "?" + qs.ToString();

            return Redirect(loginUrl);
        }
        
        private static string GenerateBase64Url(int bytesLength)
        {
            var bytes = new byte[bytesLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            var base64 = Convert.ToBase64String(bytes);
            return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        private class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; } = "";

            [JsonProperty("id_token")]
            public string IdToken { get; set; } = "";

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; } = "";

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
        }
    }
}