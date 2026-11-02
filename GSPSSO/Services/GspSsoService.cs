using GSPSSO.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace GSPSSO.Services
{
    public class GspSsoService
    {
        private readonly HttpClient _httpClient;

        public GspSsoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GspOidcConfiguration> GetOidcConfigurationAsync()
        {
            var response = await _httpClient.GetAsync(
                "https://login.cp.gov.tw/realms/gsp-sso/.well-known/openid-configuration");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<GspOidcConfiguration>(json);
        }
    }
}