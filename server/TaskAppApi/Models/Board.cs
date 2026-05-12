using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskAppApi.Models;

public class Board
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    [MaxLength(75)]
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [InverseProperty("Board")]
    public virtual ICollection<BoardTask> BoardTasks { get; set; } = new List<BoardTask>();
}