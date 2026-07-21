using Application.Abstractions.Authentication;

namespace Application.Features.Acc.OpeningBalances
{
    public class OpeningBalanceService : IOpeningBalanceService
    {
        private readonly IUserContext _userContext;
        public OpeningBalanceService(IUserContext userContext)
        {
            _userContext = userContext;
        }


    }
}
