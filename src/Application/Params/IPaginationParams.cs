namespace Application.Params
{
    public interface IPaginationParams
    {
        int Skip { get; }
        int? Take { get; }
    }
}
