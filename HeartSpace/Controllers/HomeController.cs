using HeartSpace.Models;
using HeartSpace.Models.EFModels;
using System.Linq;
using System.Web.Mvc;
using HeartSpace.BLL;

namespace HeartSpace.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController()
        {
            _context = new AppDbContext();
        }

        public ActionResult Index()
        {
            // 從資料庫中讀取所有活動
            var events = _context.Events.ToList();

            // 傳遞資料給視圖
            return View(events);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
