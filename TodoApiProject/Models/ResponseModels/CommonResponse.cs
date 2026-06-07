namespace TodoApiProject.Models.ResponseModels
{
    public class CommonResponse<T>
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static CommonResponse<T> Success(string message, T data)
        {
            return new CommonResponse<T> { Status = "Success", Message = message, Data = data };
        }

        public static CommonResponse<T> Unauthorized(string message)
        {
            return new CommonResponse<T> { Status = "Unauthorized", Message = message };
        }

        public static CommonResponse<T> NotFound(string message)
        {
            return new CommonResponse<T> { Status = "NotFound", Message = message };
        }

        public static CommonResponse<T> Failure(string message)
        {
            return new CommonResponse<T> { Status = "Failure", Message = message };
        }
    }
}
