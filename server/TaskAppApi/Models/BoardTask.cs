using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TaskAppApi.Models;

public class BoardTask
{
    public Guid Id { get; set; }
    public required Guid BoardId { get; set; }
    [MaxLength(75)]
    public required string Summary { get; set; }
    [MaxLength(1000)]
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTime? DueAt { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BoardTaskStatus Status { get; set; } = BoardTaskStatus.ToDo;
    [ForeignKey("BoardId")]
    [InverseProperty("BoardTasks")]
    [JsonIgnore]
    public virtual Board Board { get; set; } = null!;
}