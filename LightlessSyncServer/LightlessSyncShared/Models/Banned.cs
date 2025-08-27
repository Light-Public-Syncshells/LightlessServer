using System.ComponentModel.DataAnnotations;

namespace LightlessSyncShared.Models;

public class Banned
{
    [Key]
    [MaxLength(100)]
    public string CharacterIdentification { get; set; }
    public string Reason { get; set; }
    [Timestamp]
    public byte[] Timestamp { get; set; }
}
