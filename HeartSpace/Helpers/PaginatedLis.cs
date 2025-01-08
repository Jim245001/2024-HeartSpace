using System;
using System.Collections.Generic;
using System.Linq;

namespace HeartSpace.Helpers
{
	public class PaginatedList<T> : List<T>
	{
		public int PageIndex { get; private set; }
		public int PageSize { get; private set; }
		public int TotalCount { get; private set; }
		public int TotalPages { get; private set; }
		public List<T> Items { get; private set; }

		public bool HasPreviousPage => PageIndex > 1;
		public bool HasNextPage => PageIndex < TotalPages;

		public PaginatedList()
		{
			Items = new List<T>();
		}
		public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
		{
			if (orderBy == null)
			{
				throw new ArgumentException("The source must be ordered before calling Skip.", nameof(orderBy));
			}

			// 排序處理
			var orderedSource = orderBy(source);

			PageIndex = pageIndex;
			PageSize = pageSize;
			TotalCount = orderedSource.Count();
			TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

			// 分頁處理
			var items = orderedSource.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
			this.AddRange(items);
		}



		public static PaginatedList<T> Create(
			IQueryable<T> source,
			int pageIndex,
			int pageSize,
			Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
		{
			if (orderBy == null)
			{
				throw new ArgumentNullException(nameof(orderBy), "The orderBy function must be provided to sort the source before calling Create.");
			}

			// 應用排序邏輯
			var orderedSource = orderBy(source);

			// 計算總數量和分頁
			var totalCount = orderedSource.Count();
			var items = orderedSource
				.Skip((pageIndex - 1) * pageSize) // 跳過前面的資料
				.Take(pageSize) // 取得目前頁面的資料
				.ToList(); // 轉換為 List

			// 回傳分頁結果
			return new PaginatedList<T>
			{
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount,
				TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
				Items = items
			};
		}





	}
}
