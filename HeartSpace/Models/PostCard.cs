using System;

namespace HeartSpace.Models
{
    public class PostCard
    {
        public int Id { get; set; } // 貼文 ID
        public string Title { get; set; } // 貼文標題
        public string PostContent { get; set; } // 貼文內容
        public DateTime PublishTime { get; set; } // 發佈時間
        public string PostImg { get; set; } // 圖片的 Base64 編碼
        public string MemberNickName { get; set; } // 發文者暱稱
        public string MemberImageUrl { get; set; } // 發文者頭像 URL
        public string CategoryName { get; set; } // 類別名稱

        public PostCard() { }



        // 初始化建構函式
        public PostCard(int id, string title, string postContent, DateTime publishTime, string postImg,string memberNickName, string categoryName)
        {
            Id = id;
            Title = title;
            PostContent = postContent;
            PublishTime = publishTime;
            PostImg = postImg;
            MemberNickName = memberNickName;
            //MemberImageUrl = memberImageUrl;
            CategoryName = categoryName;

        }

        // RenderHTML 方法：生成 HTML 標籤
        public string RenderHtml()
        {
            // 限制貼文內容字數（例如：80 字）
            string truncatedPostContent = PostContent.Length > 80
                ? PostContent.Substring(0, 80) + "..."
                : PostContent;

            return $@"
                <div style='width: 250px; height: 266px; background-color: #d9d9d9; border-radius: 7px; padding: 7px; font-family: Arial, sans-serif; position: relative; font-size: 0.6rem;'>

                    <!-- 貼文圖片 -->
                    <div style='height: 80px; background-color: #e7f3e8; border: 1px solid #c0c0c0; display: flex; justify-content: center; align-items: center;'>
                        <img src='{PostImg}' alt='活動圖片' style='width: 90%; height: 90%; object-fit: cover;' />
                    </div>

                    <!-- 貼文內容 -->
                    <div style='margin-top: 5px; text-align: left;'>
                        <p style='margin: 0;'><strong style='font-size:16px'>{Title}</strong></p>
                        <p style='margin: -2px'>
                            <img src='{MemberImageUrl}' style='width: 9px; height: 9px; object-fit: cover;' />
                            <strong style='font-size:9px'>{MemberNickName}</strong>
                        </p>
                        <p style='margin: 6px 0 0 0; font-size:12px;margin-top:15px;'>{truncatedPostContent}</p>
                    </div>

                    <!-- 發佈時間 -->
                    <p style='position: absolute; bottom: 7px; right: 7px; margin: 0; font-size: 0.8rem; color: #888;'>
                        貼文時間：{PublishTime:yyyy/MM/dd tt h:mm}
                    </p>

                </div>";
        }
    }
}
