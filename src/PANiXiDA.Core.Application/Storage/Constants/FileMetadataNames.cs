namespace PANiXiDA.Core.Application.Storage.Constants;

/// <summary>
/// Well-known names for caller-defined file metadata, without provider-specific header prefixes.
/// </summary>
public static class FileMetadataNames
{
    /// <summary>
    /// Identifies the content update attempt that created a stored file version.
    /// </summary>
    public const string ContentUpdateAttemptId = "content-update-attempt-id";

    /// <summary>
    /// Identifies the relocation attempt that created a stored file version.
    /// </summary>
    public const string RelocationAttemptId = "relocation-attempt-id";
}
