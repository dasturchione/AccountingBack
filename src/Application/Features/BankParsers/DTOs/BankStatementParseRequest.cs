using Microsoft.AspNetCore.Http;

namespace Application.Features.BankParsers
{
    public class BankStatementParseRequest
    {
        public IFormFile File { get; set; } = null!;
        public BankStatementBankType BankType { get; set; }
    }
}
