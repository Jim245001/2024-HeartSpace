using HeartSpace.Models.EFModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HeartSpace.Helpers; // 替換為 PaginatedList 所在的命名空間
using HeartSpace.Models.ViewModels; // 替換為 PostViewModel 所在的命名空間

namespace HeartSpace.Models
{
	public class HomePageViewModel
	{
		public PaginatedList<PostViewModel> Posts { get; set; }

		public PaginatedList<EventViewModel> Events { get; set; } // 活動分頁資料
	}

}