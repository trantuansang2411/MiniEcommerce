namespace ApplicationCore.Entities.Warehousing;

public class Warehouse
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public WarehouseStatus Status { get; private set; } = WarehouseStatus.Active;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public ICollection<Inventory> Inventories { get; private set; } = new List<Inventory>();

    private Warehouse() { }

    public Warehouse(string code, string name, string address)
    {
        Code = NormalizeCode(code);
        Name = NormalizeRequired(name, nameof(name));
        Address = NormalizeRequired(address, nameof(address));
    }

    public void Update(string? name, string? address, WarehouseStatus? status)
    {
        if (name is null && address is null && !status.HasValue)
            throw new ArgumentException("At least one field must be provided for update.");

        if (name is not null)
            Name = NormalizeRequired(name, nameof(name));

        if (address is not null)
            Address = NormalizeRequired(address, nameof(address));

        if (status.HasValue)
            Status = status.Value;

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeCode(string code)
    {
        var normalized = NormalizeRequired(code, nameof(code)).ToUpperInvariant();
        return normalized;
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} is required.", parameterName);

        return value.Trim();
    }
}

public enum WarehouseStatus
{
    Active = 1,
    Inactive = 2,
    Closed = 3
}
