using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace GSPSSO
{
    public partial class Logout : Page
    {
        protected string LogoutEndpoint = "";
        protected string IdTokenHint = "";
        protected string PostLogoutRedirectUri = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            string authority = ConfigurationManager.AppSettings["GspSso.Authority"];

            // 要先取出 id_token，再清 Session
            IdTokenHint = Session["Gsp_IdToken"] as string ?? "";

            // 清除本地 Session
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();

            // 若有 FormsAuthentication 也一併清掉
            FormsAuthentication.SignOut();

            // 強制讓 ASP.NET_SessionId Cookie 過期
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                var cookie = new HttpCookie("ASP.NET_SessionId", "")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Path = "/"
                };
                Response.Cookies.Add(cookie);
            }

            LogoutEndpoint = authority.TrimEnd('/') + "/protocol/openid-connect/logout";
        }
    }
}