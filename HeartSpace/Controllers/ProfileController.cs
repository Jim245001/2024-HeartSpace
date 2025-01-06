using HeartSpace.DAL;
using HeartSpace.Models.EFModel;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Mvc;

namespace HeartSpace.Controllers
{
	public class ProfileController : Controller
	{
		public ActionResult Profile()
		{
			return View();
		}


	}
}