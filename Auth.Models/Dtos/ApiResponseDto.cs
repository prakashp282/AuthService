namespace Auth.Models.Dtos
{
    public class ApiResponseDto
    {
        public ApiResponseDto(int status = 200, string message = "", dynamic data = null, string error = "")
        {
            Status = status;
            Message = message;
            Data = data;
            Error = error;
        }

        /// <summary>
        /// Response status, true indicates success.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// The info message to be displayed based on status.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The Error message to be displayed.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// The actual response data on success.
        /// </summary>
        public dynamic Data { get; set; }
    }
}