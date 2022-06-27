using System.Text.Json.Serialization;

namespace Doreto.Shared.Models;

public class Character
{
    /// <summary>
    ///     Unique id of the character
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    ///     Name of the character
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    ///     Id of the server of the character
    /// </summary>
    public string ServerId { get; set; }

    /// <summary>
    ///     The id of the cell where the character is.
    /// </summary>
    [JsonIgnore]
    public short CellId { get; set; } = -1;
}
