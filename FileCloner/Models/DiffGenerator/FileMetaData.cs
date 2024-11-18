using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FileCloner.Models.DiffGenerator;

/// <summary>
/// Represents metadata for a file or folder, including properties for size, 
/// last modification date, path, and hierarchical relationships.
/// </summary>
public class FileMetadata
{
    /// <summary>
    /// The size of the file in bytes. Nullable to accommodate folders (which do not have a size).
    /// </summary>
    [JsonPropertyName("SIZE")]
    public int? Size { get; set; }

    /// <summary>
    /// The last modified timestamp for the file or folder.
    /// </summary>
    [JsonPropertyName("LAST_MODIFIED")]
    public DateTime LastModified { get; set; }

    /// <summary>
    /// The full path of the file or folder in the filesystem.
    /// </summary>
    [JsonPropertyName("FULL_PATH")]
    public string FullPath { get; set; }

    /// <summary>
    /// A collection of child files or folders for this metadata entry, supporting hierarchical structures.
    /// </summary>
    [JsonPropertyName("CHILDREN")]
    public Dictionary<string, FileMetadata> Children { get; set; } = new(); // Recursive children

    /// <summary>
    /// The network address or IP address associated with this file or folder.
    /// </summary>
    [JsonPropertyName("ADDRESS")]
    public string Address { get; set; }

    /// <summary>
    /// A color code or identifier associated with the file for UI representation purposes.
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// The name of the root directory where this file or folder resides.
    /// Required for relative path calculations.
    /// </summary>
    public required string InitDirectoryName { get; set; }
}
