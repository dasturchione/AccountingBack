namespace Application.Abstractions.Authentication
{
    public interface IUserContext
    {
        int? Id { get; }

        int? RoleId { get; }

        short? LanguageId { get; }
    }
}
