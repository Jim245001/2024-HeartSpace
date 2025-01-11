using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeartSpace.Models
{
	public class SearchEventViewModel
	{
		public List<EventCard> Events { get; set; }
		public List<EventCard> RecommendedEvents { get; set; }
	}
}