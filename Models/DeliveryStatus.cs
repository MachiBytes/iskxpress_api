namespace iskxpress_api.Models;

/// <summary>
/// Enumeration for delivery status
/// </summary>
public enum DeliveryStatus
{
    /// <summary>
    /// Order has been picked up from the merchant
    /// </summary>
    PickedUp = 0,

    /// <summary>
    /// Order is out for delivery to the customer
    /// </summary>
    OutForDelivery = 1,

    /// <summary>
    /// Order has been delivered to the customer
    /// </summary>
    Delivered = 2,

    /// <summary>
    /// Delivery has been cancelled
    /// </summary>
    Cancelled = 3
} 