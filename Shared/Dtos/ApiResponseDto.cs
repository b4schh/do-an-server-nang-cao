namespace FootballField.API.Shared.Dtos
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; } = 200;
        public T? Data { get; set; }
        public IEnumerable<string>? Errors { get; set; } // lỗi chi tiết (nếu có)

        public ApiResponse() { }

        public ApiResponse(bool success, string message, T? data, int statusCode = 200)
        {
            Success = success;
            Message = message;
            Data = data;
            StatusCode = statusCode;
        }

        // Helper methods
        public static ApiResponse<T> Ok(T data, string message = "", int statusCode = 200)
            => new ApiResponse<T>(true, message, data, statusCode);

        public static ApiResponse<T> Fail(string message, int statusCode = 400, IEnumerable<string>? errors = null)
            => new ApiResponse<T>(false, message, default, statusCode)
            {
                Errors = errors
            };
    }
}
