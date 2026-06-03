namespace SharedKernel.Results
{
    public record Error
    {
        public Error(string code, string description, ErrorType type)
        {
            Code = code;
            Description = description;
            Type = type;
        }
        
        public string Code { get; }

        public string Description { get; }

        public ErrorType Type { get; }

        public static readonly Error None = new(
            code: string.Empty,
            description: string.Empty,
            type: ErrorType.None);

        public static readonly Error NullValue = new(
            code: "General.Null",
            description: "Null value was provided.",
            type: ErrorType.Problem);

        public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

        public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);

        public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);

        public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);

        public static Error Business(string code, string description) => new(code, description, ErrorType.Business);

        public static Error Problem(string code, string description) => new(code, description, ErrorType.Problem);

        public static Error Timeout(string code, string description) => new(code, description, ErrorType.Timeout);
    }
}
