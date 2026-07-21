namespace Application.Abstractions.Authentication;

public interface IUserContext
{
    int? Id { get; }

    int? RoleId { get; }

    short? LanguageId { get; }

    int? TenantId { get; }

    // Header berilgan bo'lsa — o'sha org; berilmasa null
    int? OrganizationId { get; }

    // User ruxsat berilgan barcha org IDlar (middleware tomonidan to'ldiriladi)
    List<int> AllowedOrganizationIds { get; }

    int? BranchId { get; }

    bool HasGlobalAccess { get; }
}
