﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeartSpace.Models
{
	public class PostCard
	{
		public int Id { get; set; } // 貼文 ID
		public string Title { get; set; } // 貼文標題
		public string MemberName { get; set; } // 發起人 ID
		public string PostContent { get; set; } // 貼文內容
		public DateTime PublishTime { get; set; } // 發佈時間
		public string ImageUrl { get; set; }

		public string MemberImageUrl { get; set; } // 發起人 ID

		// 初始化建構函式
		public PostCard(string title, string memberName, string postContent, DateTime publishTime, string imageUrl, string memberImageUrl)
		{
			Title = title;
			MemberName = memberName;
			PostContent = postContent;
			PublishTime = publishTime;
			ImageUrl = imageUrl;
			MemberImageUrl = memberImageUrl;
		}

		// RenderHTML 方法：生成 HTML 標籤
		public string RenderHtml()
		{
			// 限制貼文內容字數（例如：100 字）
			string truncatedPostContent = PostContent.Length > 100
				? PostContent.Substring(0, 80) + "..."
				: PostContent;

			return $@"
				<div style='width: 250px; height: 266px; background-color: #d9d9d9; border-radius: 7px; padding: 7px; font-family: Arial, sans-serif; position: relative; font-size: 0.6rem;'>

					<!-- 左上角的圖片 -->
					<div style='height: 80px; background-color: #e7f3e8; border: 1px solid #c0c0c0; display: flex; justify-content: center; align-items: center;'>
						<img src='{ImageUrl}' alt='活動圖片' style='width: 90%; height: 90%; object-fit: cover;' />
					</div>

					<!-- 內容區域 -->
					<div style='margin-top: 5px; text-align: left;'>
						<p style='margin: 0;'><strong style='font-size:16px'>{Title}</strong></p>
						<p style='margin: -2px'>
							<img src='{MemberImageUrl}' style='width: 9px; height: 9px; object-fit: cover;' />
							<strong style='font-size:9px'>{MemberName}</strong>
						</p>
						<p style='margin: 6px 0 0 0; font-size:12px;margin-top:15px;'>{truncatedPostContent}</p>
					</div>

					<!-- 發佈時間，固定於卡片底部 -->
					<p style='position: absolute; bottom: 7px; right: 7px; margin: 0; font-size: 0.8rem; color: #888;'>
						貼文時間：{PublishTime:yyyy/MM/dd tt h:mm}
					</p>

				</div>";
		}

	}

}