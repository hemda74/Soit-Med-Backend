namespace SoitMed.Common
{
    /// <summary>
    /// Represents the result of a service operation
    /// </summary>
    /// <typeparam name="T">The type of data returned</typeparam>
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? ErrorCode { get; private set; }

        private ServiceResult(bool isSuccess, T? data, string? errorMessage, string? errorCode)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T>(true, data, null, null);
        }

        /// <summary>
        /// Creates a successful result with no data
        /// </summary>
        public static ServiceResult<T> Success()
        {
            return new ServiceResult<T>(true, default(T), null, null);
        }

        /// <summary>
        /// Creates a failed result
        /// </summary>
        public static ServiceResult<T> Failure(string errorMessage, string? errorCode = null)
        {
            return new ServiceResult<T>(false, default(T), errorMessage, errorCode);
        }
    }
}



