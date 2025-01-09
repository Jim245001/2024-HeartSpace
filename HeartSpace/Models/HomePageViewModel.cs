using HeartSpace.Models.EFModels;
using System.Collections.Generic;
using HeartSpace.Helpers; // 替換為 PaginatedList 所在的命名空間
using HeartSpace.Models.ViewModels; // 替換為 PostViewModel 和 EventViewModel 所在的命名空間
using HeartSpace.Models;

namespace HeartSpace.Models
{
	public class HomePageViewModel
	{
		public PaginatedList<PostViewModel> Posts { get; set; } // 貼文分頁資料
		public PaginatedList<EventViewModel> Events { get; set; } // 活動分頁資料
		public List<Category> Categories { get; set; } // 新增：分類資料
		public string SelectedCategory { get; set; } // 新增：當前選中的分類名稱
		public int? SelectedCategoryId { get; set; } // 新增：當前選中的分類 ID
	}
}
