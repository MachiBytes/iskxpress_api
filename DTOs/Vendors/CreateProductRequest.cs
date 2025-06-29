using System.ComponentModel.DataAnnotations;

namespace iskxpress_api.DTOs.Vendors;

/// <summary>
/// Data transfer object for creating a new product
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// Product name
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Base price of the product (before markup)
    /// </summary>
    [Required(ErrorMessage = "Base price is required")]
    [Range(0.01, 999.99, ErrorMessage = "Base price must be between $0.01 and $999.99")]
    [DataType(DataType.Currency)]
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Category ID that this product belongs to
    /// </summary>
    [Required(ErrorMessage = "Category ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a positive number")]
    public int CategoryId { get; set; }

    /// <summary>
    /// Section ID that this product belongs to
    /// </summary>
    [Required(ErrorMessage = "Section ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Section ID must be a positive number")]
    public int SectionId { get; set; }
} 