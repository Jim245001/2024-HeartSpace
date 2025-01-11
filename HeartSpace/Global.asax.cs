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

			// �]�m AntiForgeryConfig ���ߤ@���Ѳ�
			AntiForgeryConfig.UniqueClaimTypeIdentifier = System.Security.Claims.ClaimTypes.NameIdentifier;

		}

		// �s�W����k�A�]�m SameSite �M Secure �ݩ�
		protected void Application_EndRequest()
		{
			foreach (string key in Response.Cookies.AllKeys)
			{
				HttpCookie cookie = Response.Cookies[key];

				// �]�m SameSite �ݩ�
				cookie.SameSite = SameSiteMode.None;

				// �T�O�ϥ� HTTPS �ɳ]�m Secure �ݩ�
				if (Request.IsSecureConnection)
				{
					cookie.Secure = true;
				}
			}
		}

		// �s�W����k�A�B�z�ϥ�_�y�J��
		protected void Application_BeginRequest()
		{
			if (HttpContext.Current.Request.IsAuthenticated)
			{
				AntiForgeryConfig.UniqueClaimTypeIdentifier = System.Security.Claims.ClaimTypes.NameIdentifier;
			}
		}
	}
}
