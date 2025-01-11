using HeartSpace.Models.EFModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeartSpace.Models
{
	public class EventCard
	{
		public int Id { get; set; }
		public string Title { get; set; } //活動標題
		public string EventContent { get; set; } //活動內容
		public DateTime EventTime { get; set; } //活動時間		
		public string EventImg { get; set; }
		public string MemberNickName { get; set; } // 發起人暱稱
		public string MemberImg { get; set; }
		public string CategoryName { get; set; } // 類別名稱

		// 無參數建構函數
		public EventCard() { }

		// Constructor 初始化
		public EventCard(int id, string title, string eventContent, DateTime eventTime, string postImg, string memberNickName, string memberImg, string categoryName)
		{
			Id = id;
			Title = title;
			EventContent = eventContent;
			EventTime = eventTime;
			EventImg = postImg;
			MemberNickName = memberNickName;
			MemberImg = memberImg;
			CategoryName = categoryName;
		}

		// Render HTML 方法
		public string RenderHtml()
		{
			// 限制活動內容字數（例如：80 字）
			string truncatedEventContent = EventContent.Length > 100
				? EventContent.Substring(0, 80) + "..."
				: EventContent;

			return $@"
                <div style='width: 250px; height: 266px; background-color: #d9d9d9; border-radius: 7px; padding: 7px; font-family: Arial, sans-serif; position: relative; font-size: 0.6rem;'>

                    <!-- 貼文圖片 -->
                    <div style='height: 80px; background-color: #e7f3e8; border: 1px solid #c0c0c0; display: flex; justify-content: center; align-items: center;'>
                        <img src='{EventImg}' alt='活動圖片' style='width: 90%; height: 90%; object-fit: cover;' />
                    </div>

                    <!-- 貼文內容 -->
                    <div style='margin-top: 5px; text-align: left;'>
                        <p style='margin: 0;'><strong style='font-size:16px'>{Title}</strong></p>
                        <p style='margin: -2px'>
                            <img src='{MemberImg}' style='width: 9px; height: 9px; object-fit: cover;' />
                            <strong style='font-size:9px'>{MemberNickName}</strong>
                        </p>
                        <p style='margin: 6px 0 0 0; font-size:12px;margin-top:15px;'>{truncatedEventContent}</p>
                    </div>

                    <!-- 發佈時間 -->
                    <p style='position: absolute; bottom: 7px; right: 7px; margin: 0; font-size: 0.8rem; color: #888;'>
                        貼文時間：{EventTime:yyyy/MM/dd tt h:mm}
                    </p>

                </div>";
		}


	}
}