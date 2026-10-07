namespace ApplicationCore.Entities.Catalog;

public class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string IconKey { get; private set; } = "generic";

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Category() { }

    public Category(string name, string? description = null, string? iconKey = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        IconKey = NormalizeIconKey(iconKey);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        Name = name.Trim();
    }

    public void SetActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim() ?? string.Empty;
    }

    public void UpdateIconKey(string? iconKey)
    {
        IconKey = NormalizeIconKey(iconKey);
    }

    private static string NormalizeIconKey(string? iconKey) =>
        string.IsNullOrWhiteSpace(iconKey) ? "generic" : iconKey.Trim().ToLowerInvariant();
}
