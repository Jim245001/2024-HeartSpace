using HeartSpace.App_Start;
using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace HeartSpace
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			UnityConfig.RegisterComponents();

			AreaRegistration.RegisterAllAreas();
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{
			if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
			{
				var formsIdentity = HttpContext.Current.User.Identity as FormsIdentity;
				if (formsIdentity != null)
				{
					// 取得 FormsAuthenticationTicket
					var ticket = formsIdentity.Ticket;

					// 分割 UserData，假設格式為 "Name|Role"
					var userDataParts = ticket.UserData.Split('|');
					if (userDataParts.Length == 2)
					{
						var role = userDataParts[1]; // 取得角色 (Role)

						// 設置角色到 HttpContext.User
						HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(formsIdentity, new[] { role });
					}
				}
			}
		}
	}
}
