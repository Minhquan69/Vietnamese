namespace Backend.Contracts
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();

        public static ApiResponse<T> Ok(T data, string message = "") => new()
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = Array.Empty<string>()
        };

        public static ApiResponse<T> Fail(string message, params string[] errors) => new()
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors?.Length > 0 ? errors : new[] { message }
        };
    }
}

