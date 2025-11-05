namespace backend.Models.ApiResponses;

public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = Array.Empty<T>();
    public PaginationMetadata Pagination { get; set; } = new();
}
