namespace WebApplication2.cls
{
   
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";
            public T? Data { get; set; }

            public static ApiResponse<T> Ok(T data, string msg = "OK") =>
                new ApiResponse<T> { Success = true, Message = msg, Data = data };

            public static ApiResponse<T> Fail(string msg) =>
                new ApiResponse<T> { Success = false, Message = msg, Data = default };
        }
     
}
