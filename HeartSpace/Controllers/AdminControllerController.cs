using System.Web.Mvc;

namespace HeartSpace.Controllers.Admin
{
    public class AdminControllerController : BaseController
	{
        // GET: AdminController
        public ActionResult ManageGroup()
        {
            return View();
        }
    }
}