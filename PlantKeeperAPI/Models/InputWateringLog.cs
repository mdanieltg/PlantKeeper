using System.ComponentModel.DataAnnotations;

namespace PlantKeeperAPI.Models;

public class InputWateringLog
{
    public Guid PlantId { get; set; }
    public Guid WateringMethodId { get; set; }
    public Guid KeeperId { get; set; }
    public DateTime Date { get; set; }

    [StringLength(255)]
    public string? Comments { get; set; }
}
