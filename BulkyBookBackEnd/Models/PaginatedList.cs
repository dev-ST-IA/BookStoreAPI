using Microsoft.EntityFrameworkCore;

namespace BulkyBookBackEnd.Models
{
    public class PaginatedList<T>:List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, Paging paging)
        {
            PageIndex = paging!=null? paging.Page:1;
            TotalPages = (int)Math.Ceiling(count / (double)paging.Size);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, Paging paging)
        {
            var page = paging!=null ? paging.Page : 1;
            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * paging.Size).Take(paging.Size).ToListAsync();
            return new PaginatedList<T>(items, count, paging);
        }
    }
}
