using System;
using System.Collections.Generic;
using System.Linq;
using HeartSpace.Models;
using HeartSpace.Models.DTOs;


namespace HeartSpace.Helpers
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;

            // 確保資料已經載入記憶體
            var data = source.ToList(); // 此時 source 是 IEnumerable，已經載入至記憶體

            // 計算總頁數
            TotalPages = (int)Math.Ceiling(data.Count() / (double)pageSize);

            // 處理圖片的 Base64 邏輯，假設 T 是 CreatePostDto
            var processedData = data.Select(item =>
            {
                if (item is CreatePostDto model)
                {
                    // 處理圖片的 Base64 轉換
                    model.MemberImgBase64 = model.MemberImgBase64 != null
                        ? $"data:image/png;base64,{model.MemberImgBase64}" // 包裹成完整的 Base64 字串
                        : null;
                }
                return item;
            }).ToList();

            // 分頁邏輯
            this.AddRange(processedData.Skip((PageIndex - 1) * pageSize).Take(pageSize));
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            return new PaginatedList<T>(source, pageIndex, pageSize);
        }
    }

}
