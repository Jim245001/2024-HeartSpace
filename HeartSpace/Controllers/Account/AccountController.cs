using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeartSpace.Controllers.Account
{
	public class AccountController : Controller
	{
		// GET: Account
		public ActionResult Login()
		{
			return View();
		}

		public ActionResult Register()
		{
			return View();
		}

		public ActionResult ForgotPassword()
		{
			return View();
		}
	}
}