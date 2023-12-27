using Microsoft.EntityFrameworkCore;

namespace Blog.Utils
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; } // tổng của danh sách ban đầu (không phải của danh sách phân trang)
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public PaginatedList(List<T> items, int pageIndex, int totalPages, int totalItems)
        {
            this.AddRange(items); // Thêm items vào List
            PageIndex = pageIndex;
            TotalPages = totalPages;
            TotalItems = totalItems;
        }

        // Static method gọi mà k cần tạo instance đối tượng
        // Vì truy vấn bất đồng bộ nên phải tạo ra một method bất đồng bộ, Contructor k có async đc
        public static async Task<PaginatedList<T>> CreateSync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var totalItems = source.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PaginatedList<T>(items, pageIndex, totalPages, totalItems);
        }
    }
}
