using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
} 