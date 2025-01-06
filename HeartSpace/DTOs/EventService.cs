using System.Collections.Generic;
using HeartSpace.Models.EFModel;
using HeartSpace.DAL;

namespace HeartSpace.BLL
{
	public class EventService
	{
		private readonly EventRepository _eventRepository;

		public EventService()
		{
			_eventRepository = new EventRepository();
		}

		public List<Event> GetAllEvents()
		{
			return _eventRepository.GetAllEvents();
		}

		public Event GetEventById(int id)
		{
			return _eventRepository.GetEventById(id);
		}

		public void AddEvent(Event newEvent)
		{
			_eventRepository.AddEvent(newEvent);
		}

		public void UpdateEvent(Event updatedEvent)
		{
			_eventRepository.UpdateEvent(updatedEvent);
		}

		public void DeleteEvent(int id)
		{
			_eventRepository.DeleteEvent(id);
		}
	}
}
 