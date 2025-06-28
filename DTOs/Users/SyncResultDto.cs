namespace iskxpress_api.DTOs.Users;

/// <summary>
/// Data transfer object for Firebase sync operation results
/// </summary>
public class SyncResultDto
{
    /// <summary>
    /// Total number of users processed during sync
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Number of new users created during sync
    /// </summary>
    public int NewUsers { get; set; }

    /// <summary>
    /// Number of existing users updated during sync
    /// </summary>
    public int UpdatedUsers { get; set; }

    /// <summary>
    /// Number of errors encountered during sync
    /// </summary>
    public int ErrorsCount { get; set; }

    /// <summary>
    /// List of error messages encountered during sync
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Indicates whether the sync operation was successful (no errors)
    /// </summary>
    public bool IsSuccessful => ErrorsCount == 0;

    /// <summary>
    /// Summary message of the sync operation
    /// </summary>
    public string Summary => $"Processed {TotalProcessed} users: {NewUsers} created, {UpdatedUsers} updated, {ErrorsCount} errors";
} 