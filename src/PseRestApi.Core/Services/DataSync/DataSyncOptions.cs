namespace PseRestApi.Core.Services.DataSync;

public class DataSyncOptions
{
    public bool SkipStaging { get; set; }
    public Guid BatchId { get; set; }
    public string? MergeCommand { get; set; }
    public string? CleanupCommand { get; internal set; }
    public string? TargetTable { get; set; }
    public Dictionary<string, string> ColumnMappings { get; set; } = new Dictionary<string, string>();
}
