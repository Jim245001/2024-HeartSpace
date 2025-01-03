using System.Web.Mvc;

namespace HeartSpace.Controllers.Admin
{
    public class AdminControllerController : Controller
    {
        // GET: AdminController
        public ActionResult ManageGroup()
        {
            return View();
        }
    }
}