using System.Web.Mvc;
using HeartSpace.BLL;

namespace HeartSpace.Controllers
{
	public class HomeController : Controller
	{
		private readonly EventService _eventService;

		public HomeController()
		{
			_eventService = new EventService();
		}

		public ActionResult Index()
		{
			var events = _eventService.GetAllEvents();
			return View(events);
		}

		public ActionResult Details(int id)
		{
			var eventDetails = _eventService.GetEventById(id);
			if (eventDetails == null)
				return HttpNotFound();

			return View(eventDetails);
		}
	}
}
