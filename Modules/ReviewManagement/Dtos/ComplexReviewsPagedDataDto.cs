using FootballField.API.Shared.Dtos;

namespace FootballField.API.Modules.ReviewManagement.Dtos
{
    public class ComplexReviewsPagedDataDto
    {
        public List<ReviewDto> Reviews { get; set; } = new();
        public ReviewStatisticsDto Statistics { get; set; } = new();
    }

    public class ComplexReviewsPagedResponse : ApiResponse<ComplexReviewsPagedDataDto>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public ComplexReviewsPagedResponse(
            ComplexReviewsPagedDataDto data,
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
        }

        public static ComplexReviewsPagedResponse Ok(
            ComplexReviewsPagedDataDto data,
            int pageIndex,
            int pageSize,
            int totalRecords,
            string message = "")
            => new ComplexReviewsPagedResponse(data, pageIndex, pageSize, totalRecords, message, 200);
    }
}
