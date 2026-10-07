using ApplicationCore.Interfaces.Catalog;
using System.Globalization;
using System.Text;

namespace Infrastructure.Storage.Catalog;

public sealed class LocalProductImageStorage : IProductImageStorage
{
    private const string PublicDirectory = "/uploads/products";
    private readonly string _directoryPath;

    public LocalProductImageStorage(string webRootPath)
    {
        if (string.IsNullOrWhiteSpace(webRootPath))
            throw new ArgumentException("Web root path is required.", nameof(webRootPath));

        _directoryPath = Path.Combine(webRootPath, "uploads", "products");
    }

    public async Task<string> SaveAsync(
        string productName,
        Stream content,
        string fileExtension,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileExtension);

        Directory.CreateDirectory(_directoryPath);

        var fileName = $"{ToSafeFileName(productName)}_{Guid.NewGuid():N}{fileExtension.ToLowerInvariant()}";
        var filePath = Path.Combine(_directoryPath, fileName);

        await using var destination = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await content.CopyToAsync(destination, cancellationToken);

        return $"{PublicDirectory}/{fileName}";
    }

    public Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) ||
            !imageUrl.StartsWith(PublicDirectory + "/", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var fileName = Path.GetFileName(imageUrl);
        var filePath = Path.Combine(_directoryPath, fileName);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    private static string ToSafeFileName(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var result = new StringBuilder();
        var previousWasSeparator = false;

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            var safeCharacter = character is 'đ' or 'Đ' ? 'd' : char.ToLowerInvariant(character);

            if (char.IsLetterOrDigit(safeCharacter))
            {
                result.Append(safeCharacter);
                previousWasSeparator = false;
            }
            else if (!previousWasSeparator)
            {
                result.Append('_');
                previousWasSeparator = true;
            }
        }

        var fileName = result.ToString().Trim('_');
        return string.IsNullOrWhiteSpace(fileName) ? "product" : fileName;
    }
}
