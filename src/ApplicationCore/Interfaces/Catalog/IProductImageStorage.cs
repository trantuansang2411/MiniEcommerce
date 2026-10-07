namespace ApplicationCore.Interfaces.Catalog;

public interface IProductImageStorage
{
    Task<string> SaveAsync(
        string productName,
        Stream content,
        string fileExtension,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default);
}
