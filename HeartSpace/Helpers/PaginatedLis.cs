using System;
using System.Collections.Generic;
using System.Linq;
using HeartSpace.Models;


namespace HeartSpace.Helpers
{
	public class PaginatedList<T> : List<T>
	{
		public int PageIndex { get; private set; }
		public int TotalPages { get; private set; }

		public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize)
		{
			PageIndex = pageIndex;
			TotalPages = (int)Math.Ceiling(source.Count() / (double)pageSize);

			this.AddRange(source.Skip((PageIndex - 1) * pageSize).Take(pageSize));
		}

		public bool HasPreviousPage => PageIndex > 1;
		public bool HasNextPage => PageIndex < TotalPages;

		public static PaginatedList<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
		{
			return new PaginatedList<T>(source, pageIndex, pageSize);
		}
	}
}
