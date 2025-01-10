using HeartSpace.Models.DTOs;
using HeartSpace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeartSpace.Models.ViewModels;

namespace HeartSpace.DTOs.Services.Interfaces
{
	public interface IEventService
	{
		EventStatusViewModel GetEventStatus(int eventId); // 獲取活動報名狀況

		List<EventCard> FindEventsByKeyword(string keyword, int pageIndex, int pageSize); // 依關鍵字搜尋活動

		IEnumerable<EventCard> GetRandomEvents(int count); // 取得隨機活動


	}
}