using System;

namespace FootballField.API.Shared.Dtos
{
    public class ApiPagedResponse<T> : ApiResponse<IEnumerable<T>>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public ApiPagedResponse(
            IEnumerable<T> data,
            int pageIndex,
            int pageSize,
            int totalRecords,
            string message = "",
            int statusCode = 200)
            : base(true, message, data, statusCode)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            // ✅ Xóa dòng này
            // Meta = new { TotalRecords, TotalPages, PageIndex, PageSize };
        }
    }
}