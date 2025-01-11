using HeartSpace.App_Start;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web;
using System.Web.Helpers;

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

			// 設置 AntiForgeryConfig 的唯一標識符
			AntiForgeryConfig.UniqueClaimTypeIdentifier = System.Security.Claims.ClaimTypes.NameIdentifier;

		}

		// 新增此方法，設置 SameSite 和 Secure 屬性
		protected void Application_EndRequest()
		{
			foreach (string key in Response.Cookies.AllKeys)
			{
				HttpCookie cookie = Response.Cookies[key];

				// 設置 SameSite 屬性
				cookie.SameSite = SameSiteMode.None;

				// 確保使用 HTTPS 時設置 Secure 屬性
				if (Request.IsSecureConnection)
				{
					cookie.Secure = true;
				}
			}
		}

		// 新增此方法，處理反仿冒語彙基元
		protected void Application_BeginRequest()
		{
			if (HttpContext.Current.Request.IsAuthenticated)
			{
				AntiForgeryConfig.UniqueClaimTypeIdentifier = System.Security.Claims.ClaimTypes.NameIdentifier;
			}
		}
	}
}
