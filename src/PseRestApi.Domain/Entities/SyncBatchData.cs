namespace PseRestApi.Domain.Entities;

public class SyncBatchData : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public string Data { get; set; } = string.Empty;
}
