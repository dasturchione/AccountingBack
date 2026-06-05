namespace SharedKernel.Query.Options
{
    public class GetByIdOptions<T>
    {
        public T Id { get; set; } = default!;

        public GetByIdOptions()
        {
        }

        public GetByIdOptions(T id)
        {
            Id = id;
        }
    }
}
