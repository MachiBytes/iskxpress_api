using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iskxpress_api.Models;

public class StallSection
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [ForeignKey("Stall")]
    public int StallId { get; set; }

    // Navigation properties
    public virtual Stall Stall { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
} 