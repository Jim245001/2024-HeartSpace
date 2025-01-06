<<<<<<< HEAD
﻿using HeartSpace.Models.EFModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
=======
﻿using System;
>>>>>>> origin/K

namespace HeartSpace.Models
{
    public class EventCard
    {
        public string Title { get; set; }
        public string Organizer { get; set; }
        public string Location { get; set; }
        public DateTime EventDate { get; set; }
        public string ImageUrl { get; set; }

<<<<<<< HEAD
		public string Description { get; set; }
		public string MemberImageUrl { get; set; } // 發起人 ID

		// Constructor 初始化
		public EventCard(string title, string organizer, string location, string description, DateTime eventDate, string imageUrl, string memberImageUrl)
		{
			Title = title;
			Organizer = organizer;
			Location = location;
			Description = description;
			EventDate = eventDate;
			ImageUrl = imageUrl;
			MemberImageUrl = memberImageUrl;
		}

		// Render HTML 方法
		public string RenderHtml()
		{

			string truncatedPostContent = Description.Length > 100
				? Description.Substring(0, 80) + "..."
				: Description;

			return $@"
			
			<div style='width: 250px; height: 266px; background-color: #d9d9d9; border-radius: 7px; padding: 7px; font-family: Arial, sans-serif; position: relative; font-size: 0.6rem;'>

				<!-- 左上角的圖片 -->
				<div style='height: 80px; background-color: #e7f3e8; border: 1px solid #c0c0c0; display: flex; justify-content: center; align-items: center;'>
					<img src='{ImageUrl}' alt='活動圖片' style='width: 90%; height: 90%; object-fit: cover;' />
				</div>

				<!-- 文字內容 -->
				<div style='margin-top: 5px; text-align: left;'>
					<!-- 標題 -->
					<p style='margin: 0;'><strong style='font-size:16px'>{Title}</strong></span>
				
					<!-- 主辦人 -->
					<p style='margin: -2px'>

						<img src='{MemberImageUrl}' style='width: 9px; height: 9px; object-fit: cover;' /><strong style='font-size:9px'>{Organizer}</strong>：<strong>{Location}</strong>

					</p>
					<!-- 描述 -->
					<p style='margin: 6px 0 0 0; font-size:12px;margin-top:15px'>{truncatedPostContent}</p>
				</div>

				<!-- 日期與時間 -->
				<p style='position: absolute; bottom: 7px; right: 7px; margin: 0; font-size: 0.8rem; color: #888;'>
						活動時間：{EventDate:yyyy/MM/dd tt h:mm}
				</p>

			</div>";
		}


	}
=======
        // Constructor 初始化
        public EventCard(string title, string organizer, string location, DateTime eventDate, string imageUrl)
        {
            Title = title;
            Organizer = organizer;
            Location = location;
            EventDate = eventDate;
            ImageUrl = imageUrl;
        }

        // Render HTML 方法
        public string RenderHtml()
        {
            return $@"
		<div style='width: 200px; height: 133px; background-color: #d9d9d9; border-radius: 7px; padding: 7px; font-family: Arial, sans-serif; position: relative; font-size: 0.6rem;'>
			<!-- 左上角的圖片 -->
			<div style='width: 40px; height: 40px; background-color: #e7f3e8; border: 1px solid #c0c0c0; display: flex; justify-content: center; align-items: center;'>
				<img src='{ImageUrl}' alt='活動圖片' style='width: 90%; height: 90%; object-fit: cover;' />
			</div>
			<!-- 文字內容 -->
			<div style='margin-top: 5px; text-align: left;'>
				<p style='margin: 5px 0 0 0;'><strong>標題：</strong>{Title}</p>
				<p style='margin: 5px 0 0 0;'><strong>主辦：</strong>{Organizer}</p>
				<p style='margin: 5px 0 0 0;'><strong>地點：</strong>{Location.Replace("\n", "<br />")}</p>
			</div>
			<!-- 日期與時間 -->
			<div style='position: absolute; bottom: 7px; right: 7px; text-align: center; background-color: white; padding: 3px; border-radius: 3px; font-size: 0.5rem;'>
				<p style='margin: 0;'>{EventDate:yyyy/MM/dd}</p>
				<p style='margin: 0;'>{EventDate:tt h:mm}</p>
			</div>
		</div>";
        }







    }
>>>>>>> origin/K
}