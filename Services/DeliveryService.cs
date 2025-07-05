using iskxpress_api.Data;
using iskxpress_api.DTOs.Delivery;
using iskxpress_api.Models;
using iskxpress_api.Repositories;

namespace iskxpress_api.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IskExpressDbContext _context;
    private readonly IDeliveryRequestRepository _deliveryRequestRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public DeliveryService(
        IskExpressDbContext context,
        IDeliveryRequestRepository deliveryRequestRepository,
        IOrderRepository orderRepository,
        IUserRepository userRepository)
    {
        _context = context;
        _deliveryRequestRepository = deliveryRequestRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public async Task<DeliveryRequestResponse> CreateDeliveryRequestAsync(int orderId)
    {
        // Validate order exists and needs delivery
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
        if (order == null)
        {
            throw new ArgumentException($"Order with ID {orderId} not found");
        }

        if (order.FulfillmentMethod != FulfillmentMethod.Delivery)
        {
            throw new ArgumentException("Order is not a delivery order");
        }

        // Check if delivery request already exists
        var existingRequest = await _deliveryRequestRepository.GetByOrderIdAsync(orderId);
        if (existingRequest != null)
        {
            throw new ArgumentException("Delivery request already exists for this order");
        }

        // Create delivery request
        var deliveryRequest = new DeliveryRequest
        {
            OrderId = orderId,
            Status = DeliveryRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _deliveryRequestRepository.AddAsync(deliveryRequest);
        await _context.SaveChangesAsync();

        // Return the created delivery request
        var createdRequest = await _deliveryRequestRepository.GetByIdWithDetailsAsync(deliveryRequest.Id);
        return MapToDeliveryRequestResponse(createdRequest!);
    }

    public async Task<DeliveryRequestResponse> AssignDeliveryRequestAsync(int requestId, int deliveryPartnerId)
    {
        // Validate delivery request exists
        var deliveryRequest = await _deliveryRequestRepository.GetByIdWithDetailsAsync(requestId);
        if (deliveryRequest == null)
        {
            throw new ArgumentException($"Delivery request with ID {requestId} not found");
        }

        if (deliveryRequest.Status != DeliveryRequestStatus.Pending)
        {
            throw new ArgumentException("Delivery request is not pending assignment");
        }

        // Validate delivery partner exists and has correct role
        var deliveryPartner = await _userRepository.GetByIdAsync(deliveryPartnerId);
        if (deliveryPartner == null)
        {
            throw new ArgumentException($"User with ID {deliveryPartnerId} not found");
        }

        if (deliveryPartner.Role != UserRole.DeliveryPartner)
        {
            throw new ArgumentException("User is not a delivery partner");
        }

        // Assign delivery request to partner
        deliveryRequest.AssignedDeliveryPartnerId = deliveryPartnerId;
        deliveryRequest.Status = DeliveryRequestStatus.Assigned;
        deliveryRequest.AssignedAt = DateTime.UtcNow;

        // Update the order to assign the delivery partner
        var order = await _orderRepository.GetOrderWithItemsAsync(deliveryRequest.OrderId);
        if (order != null)
        {
            order.DeliveryPartnerId = deliveryPartnerId;
            await _context.SaveChangesAsync();
        }

        await _context.SaveChangesAsync();

        // Return the updated delivery request
        var updatedRequest = await _deliveryRequestRepository.GetByIdWithDetailsAsync(requestId);
        return MapToDeliveryRequestResponse(updatedRequest!);
    }

    public async Task<IEnumerable<DeliveryRequestResponse>> GetAvailableDeliveryRequestsAsync()
    {
        var pendingRequests = await _deliveryRequestRepository.GetPendingRequestsAsync();
        return pendingRequests.Select(MapToDeliveryRequestResponse);
    }

    public async Task<IEnumerable<DeliveryRequestResponse>> GetDeliveryPartnerRequestsAsync(int deliveryPartnerId)
    {
        var assignedRequests = await _deliveryRequestRepository.GetByDeliveryPartnerIdAsync(deliveryPartnerId);
        return assignedRequests.Select(MapToDeliveryRequestResponse);
    }

    public async Task<DeliveryRequestResponse?> GetDeliveryRequestAsync(int requestId)
    {
        var deliveryRequest = await _deliveryRequestRepository.GetByIdWithDetailsAsync(requestId);
        return deliveryRequest != null ? MapToDeliveryRequestResponse(deliveryRequest) : null;
    }

    public async Task<DeliveryRequestResponse> UpdateDeliveryRequestStatusAsync(int requestId, DeliveryRequestStatus status)
    {
        var deliveryRequest = await _deliveryRequestRepository.GetByIdWithDetailsAsync(requestId);
        if (deliveryRequest == null)
        {
            throw new ArgumentException($"Delivery request with ID {requestId} not found");
        }

        // Validate status transition
        if (!IsValidStatusTransition(deliveryRequest.Status, status))
        {
            throw new ArgumentException($"Invalid status transition from {deliveryRequest.Status} to {status}");
        }

        deliveryRequest.Status = status;
        if (status == DeliveryRequestStatus.Completed)
        {
            deliveryRequest.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Return the updated delivery request
        var updatedRequest = await _deliveryRequestRepository.GetByIdWithDetailsAsync(requestId);
        return MapToDeliveryRequestResponse(updatedRequest!);
    }

    private static bool IsValidStatusTransition(DeliveryRequestStatus currentStatus, DeliveryRequestStatus newStatus)
    {
        return currentStatus switch
        {
            DeliveryRequestStatus.Pending => newStatus == DeliveryRequestStatus.Assigned || newStatus == DeliveryRequestStatus.Cancelled,
            DeliveryRequestStatus.Assigned => newStatus == DeliveryRequestStatus.Completed || newStatus == DeliveryRequestStatus.Cancelled,
            DeliveryRequestStatus.Completed => false, // Cannot change from completed
            DeliveryRequestStatus.Cancelled => false, // Cannot change from cancelled
            _ => false
        };
    }

    private static DeliveryRequestResponse MapToDeliveryRequestResponse(DeliveryRequest deliveryRequest)
    {
        return new DeliveryRequestResponse
        {
            Id = deliveryRequest.Id,
            OrderId = deliveryRequest.OrderId,
            Order = new OrderSummaryResponse
            {
                Id = deliveryRequest.Order.Id,
                UserId = deliveryRequest.Order.UserId,
                UserName = deliveryRequest.Order.User.Name,
                StallId = deliveryRequest.Order.StallId,
                StallName = deliveryRequest.Order.Stall.Name,
                DeliveryAddress = deliveryRequest.Order.DeliveryAddress ?? string.Empty,
                TotalPrice = deliveryRequest.Order.TotalPrice,
                CreatedAt = deliveryRequest.Order.CreatedAt
            },
            AssignedDeliveryPartnerId = deliveryRequest.AssignedDeliveryPartnerId,
            AssignedDeliveryPartner = deliveryRequest.AssignedDeliveryPartner != null ? new UserSummaryResponse
            {
                Id = deliveryRequest.AssignedDeliveryPartner.Id,
                Name = deliveryRequest.AssignedDeliveryPartner.Name,
                Email = deliveryRequest.AssignedDeliveryPartner.Email
            } : null,
            Status = deliveryRequest.Status,
            CreatedAt = deliveryRequest.CreatedAt,
            AssignedAt = deliveryRequest.AssignedAt,
            CompletedAt = deliveryRequest.CompletedAt
        };
    }
} 