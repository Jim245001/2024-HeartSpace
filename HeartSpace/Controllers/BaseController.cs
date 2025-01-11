using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace HeartSpace.Controllers
{
	public class BaseController : Controller
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!User.Identity.IsAuthenticated)
			{
				// 設置訪客角色
				HttpContext.User = new GenericPrincipal(
					new GenericIdentity("Guest"), new[] { "Guest" });
			}

			base.OnActionExecuting(filterContext);
		}
	}
}