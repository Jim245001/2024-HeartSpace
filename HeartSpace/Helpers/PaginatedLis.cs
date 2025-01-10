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
        public int TotalItemCount { get; private set; }

        public PaginatedList(IEnumerable<T> items, int totalItemCount, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalItemCount = totalItemCount;
            TotalPages = (int)Math.Ceiling(totalItemCount / (double)pageSize);

            // 加入資料（不需要轉換 List）
            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static PaginatedList<T> Create(IEnumerable<T> items, int totalItemCount, int pageIndex, int pageSize)
        {
            return new PaginatedList<T>(items, totalItemCount, pageIndex, pageSize);
        }
    }
}