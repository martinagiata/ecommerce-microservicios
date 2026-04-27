namespace MiniApi.Models;

// ── Entidad principal ─────────────────────────
public record Item
{
    public long Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public double Price { get; init; }

    public long Stock { get; init; }

    public string CreatedAt { get; init; } = string.Empty;

    public string? UpdatedAt { get; init; }
}