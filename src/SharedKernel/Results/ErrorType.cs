namespace SharedKernel.Results
{
    public enum ErrorType
    {
        /// <summary>
        /// No error occurred. Used for successful results.
        /// </summary>
        None = 0,

        /// <summary>
        /// Validation error caused by invalid input data (missing fields, incorrect format, etc.)
        /// </summary>
        /// 400
        Validation = 1,

        /// <summary>
        /// The user is not authenticated or the authentication token is invalid or missing
        /// </summary>
        /// 401
        Unauthorized = 2,

        /// <summary>
        /// The user is authenticated but does not have permission to perform the requested action
        /// </summary>
        /// 403
        Forbidden = 3,

        /// <summary>
        /// The requested resource was not found
        /// </summary>
        /// 404
        NotFound = 4,

        /// <summary>
        /// A conflict occurred with the current state of the resource (duplicates, version mismatch, etc.)
        /// </summary>
        /// 409
        Conflict = 5,

        /// <summary>
        /// Business rule violation where the request is valid but not allowed by domain logic
        /// </summary>
        /// 422
        Business = 6,

        /// <summary>
        /// An unexpected internal system error occurred or an unhandled exception was thrown
        /// </summary>
        /// 500
        Problem = 7,

        /// <summary>
        /// The operation timed out while waiting for a response from a dependency or service
        /// </summary>
        /// 504
        Timeout = 8
    }
}
