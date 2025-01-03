using HeartSpace.App_Start;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace HeartSpace
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
          
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            UnityConfig.RegisterComponents(); // ±Ò¥Î Unity ®e¾¹

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
