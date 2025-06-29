using System.ComponentModel.DataAnnotations;
using iskxpress_api.Models;

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
    /// Product description (optional)
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Product picture file ID reference (optional)
    /// </summary>
    public int? PictureId { get; set; }

    /// <summary>
    /// Base price of the product (before markup)
    /// </summary>
    [Required(ErrorMessage = "Base price is required")]
    [Range(0.01, 999.99, ErrorMessage = "Base price must be between $0.01 and $999.99")]
    [DataType(DataType.Currency)]
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Product availability status
    /// </summary>
    public ProductAvailability Availability { get; set; } = ProductAvailability.Available;

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