# Delivery System Implementation

This document describes the complete delivery functionality implementation for the ISK Express API.

## Overview

The delivery system allows for flexible order fulfillment through both pickup and delivery methods, with support for external delivery partners and automatic order confirmation.

## Key Features

### 1. Delivery Request System
- Orders with delivery method are automatically converted to delivery requests
- Delivery requests can be assigned to delivery partners
- Pending delivery requests are visible to all delivery partners

### 2. Stall Delivery Availability
- Stalls can set their delivery partner availability
- Orders are handled differently based on stall delivery availability
- Real-time updates of delivery availability status

### 3. Order Status Management
- Comprehensive order status flow with validation
- Different flows for pickup vs delivery orders
- Automatic order confirmation system

### 4. User Confirmation System
- 5-minute automatic confirmation for delivered orders
- Manual confirmation option for users
- Tracking of confirmation status

## Delivery Flow

### For Orders with Stall Delivery Partner Available:
1. **Checkout** → Order created with `Pending` status
2. **Vendor** → Sets status to `ToPrepare`
3. **Vendor** → Sets status to `ToReceive` (skips `ToDeliver`)
4. **Delivery Partner** → Delivers to user
5. **User** → Confirms delivery (or auto-confirms after 5 minutes)
6. **System** → Sets status to `Accomplished`

### For Orders without Stall Delivery Partner:
1. **Checkout** → Order created with `Pending` status + Delivery Request created
2. **Delivery Partner** → Assigns delivery request to themselves
3. **Vendor** → Order appears in stall orders list
4. **Vendor** → Sets status to `ToPrepare`
5. **Vendor** → Sets status to `ToDeliver`
6. **Delivery Partner** → Sets status to `ToReceive`
7. **User** → Confirms delivery (or auto-confirms after 5 minutes)
8. **System** → Sets status to `Accomplished`

### For Pickup Orders:
1. **Checkout** → Order created with `Pending` status
2. **Vendor** → Sets status to `ToPrepare`
3. **Vendor** → Sets status to `ToReceive` (skips `ToDeliver`)
4. **User** → Picks up order
5. **User** → Confirms pickup (or auto-confirms after 5 minutes)
6. **System** → Sets status to `Accomplished`

## API Endpoints

### Delivery Requests
```
GET /api/Delivery/requests                           # Get available delivery requests
GET /api/Delivery/partner/{partnerId}/requests       # Get partner's assigned requests
GET /api/Delivery/requests/{requestId}               # Get specific delivery request
POST /api/Delivery/requests/{requestId}/assign       # Assign request to partner
PUT /api/Delivery/requests/{requestId}/status        # Update request status
```

### Order Management
```
PUT /api/Order/{orderId}/status                      # Update order status
POST /api/Order/{orderId}/confirm                    # Confirm order delivery
```

### Stall Management
```
PUT /api/stalls/{stallId}/delivery-availability      # Update delivery availability
```

## Order Status Flow

### Status Values:
- `0` = Pending
- `1` = ToPrepare
- `2` = ToDeliver
- `3` = ToReceive
- `4` = Accomplished

### Valid Status Transitions:
- **Pending** → **ToPrepare** (Vendor starts preparation)
- **ToPrepare** → **ToDeliver** (Ready for delivery partner pickup)
- **ToPrepare** → **ToReceive** (Ready for pickup or direct delivery)
- **ToDeliver** → **ToReceive** (Delivery partner picked up)
- **ToReceive** → **Accomplished** (User confirms delivery)

## Database Models

### DeliveryRequest
- Links orders to delivery partners
- Tracks assignment and completion status
- Manages delivery request lifecycle

### OrderConfirmation
- Handles user confirmation for delivered orders
- Implements 5-minute auto-confirmation
- Tracks confirmation status and timing

### Stall (Updated)
- Added `HasDeliveryPartner` flag
- Added `DeliveryAvailable` flag
- Controls delivery request creation

## Business Rules

1. **Delivery Request Creation**: Only created for delivery orders when stall has no delivery partner
2. **Order Visibility**: Orders only appear in stall orders after delivery partner assignment
3. **Status Validation**: Strict validation of order status transitions
4. **Auto-Confirmation**: Orders auto-confirm after 5 minutes in ToReceive status
5. **Delivery Partner Assignment**: Only users with DeliveryPartner role can be assigned

## Error Handling

- **Invalid Status Transitions**: Returns 400 with validation message
- **Missing Delivery Partner**: Returns 400 if user is not a delivery partner
- **Order Not Found**: Returns 404 for non-existent orders
- **Already Confirmed**: Returns 400 if order already confirmed

## Testing

Use the `delivery-examples.http` file to test all delivery endpoints:

1. **Create delivery order** (use existing checkout endpoint)
2. **Get available delivery requests**
3. **Assign delivery request to partner**
4. **Update order status through the flow**
5. **Confirm order delivery**

## Future Enhancements

1. **Real-time Notifications**: WebSocket notifications for status changes
2. **Delivery Tracking**: GPS tracking for delivery partners
3. **Delivery Partner Management**: Partner registration and verification
4. **Delivery Fees**: Dynamic pricing based on distance and demand
5. **Batch Processing**: Process multiple auto-confirmations efficiently 