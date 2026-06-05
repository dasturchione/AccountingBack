using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Users.Queries
{
    public record GetUserByUserNameOptions(string Username);

    public class UserByUserNameCriteriaBuilder : ICriteriaBuilder<User, GetUserByUserNameOptions>
    {
        public Expression<Func<User, bool>> Build(GetUserByUserNameOptions options)
        {
            return x => x.UserName == options.Username;
        }
    }
}
