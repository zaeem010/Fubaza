namespace Fubaza.Application.Core.Common
{
    public class PaginationInfo
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class PaginatedResponse<T>
    {
        public PaginationInfo? Pagination { get; set; }
        public List<T>? Items { get; set; }

        public T? Item { get; set; }
    }
}
