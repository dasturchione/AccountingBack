namespace Application.Features.Departments;

public class DepartmentBaseDto
{
    public int? BranchId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
}
