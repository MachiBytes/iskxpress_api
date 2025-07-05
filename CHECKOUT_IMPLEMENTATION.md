# Checkout Implementation

This document describes the checkout functionality implementation for the ISK Express API.

## Overview

The checkout functionality allows users to convert their cart items into orders. The system creates orders with order items that capture the product prices at checkout time, ensuring that vendor price changes don't affect existing orders.

## Key Features

### 1. Cart to Order Conversion
- Users can checkout multiple cart items at once
- Cart items are grouped by stall (one order per stall)
- Cart items are removed after successful order creation

### 2. Price Snapshot
- Order items store the product price at checkout time (`PriceEach`)
- Uses `PriceWithMarkup` from the Product model
- Total price is calculated and stored in the order

### 3. Fulfillment Methods
- **Pickup**: User picks up the order from the stall
- **Delivery**: Order is delivered to the user's address
- Delivery address is required when delivery method is selected

### 4. Validation
- Validates that cart items belong to the user
- Checks that products still exist and are available
- Ensures delivery address is provided for delivery orders
- Prevents checkout from multiple stalls in a single order

## API Endpoints

### Checkout
```
POST /api/Order/user/{userId}/checkout
```

**Request Body:**
```json
{
  "cartItemIds": [1, 2, 3],
  "fulfillmentMethod": 1, // 0 = Pickup, 1 = Delivery
  "deliveryAddress": "123 Main St, City, Country", // Required for delivery
  "notes": "Please deliver to the front door"
}
```

**Response:**
```json
{
  "id": 1,
  "userId": 1,
  "stallId": 1,
  "stallName": "Test Stall",
  "status": 0, // Pending
  "fulfillmentMethod": 1, // Delivery
  "deliveryAddress": "123 Main St, City, Country",
  "notes": "Please deliver to the front door",
  "totalPrice": 45.00,
  "createdAt": "2024-01-01T12:00:00Z",
  "orderItems": [
    {
      "id": 1,
      "productId": 1,
      "productName": "Pizza",
      "productDescription": "",
      "productPictureUrl": "https://example.com/pizza.jpg",
      "quantity": 2,
      "priceEach": 15.00,
      "totalPrice": 30.00
    }
  ]
}
```

### Get User Orders
```
GET /api/Order/user/{userId}
GET /api/Order/user/{userId}?status={orderStatus}
```

**Query Parameters:**
- `status` (optional): Filter orders by status (0=Pending, 1=ToPrepare, 2=ToDeliver, 3=ToReceive, 4=Accomplished)

**Examples:**
```
GET /api/Order/user/1                    # Get all user orders
GET /api/Order/user/1?status=0           # Get only pending orders
GET /api/Order/user/1?status=4           # Get only accomplished orders
```

### Get Stall Orders
```
GET /api/Order/stall/{stallId}
GET /api/Order/stall/{stallId}?status={orderStatus}
```

**Query Parameters:**
- `status` (optional): Filter orders by status (0=Pending, 1=ToPrepare, 2=ToDeliver, 3=ToReceive, 4=Accomplished)

**Examples:**
```
GET /api/Order/stall/1                   # Get all stall orders
GET /api/Order/stall/1?status=1          # Get only orders to prepare
GET /api/Order/stall/1?status=2          # Get only orders ready for delivery
```

### Get Specific Order
```
GET /api/Order/{orderId}
```

## Order Status Flow

1. **Pending** - Order created, waiting for vendor to start preparation
2. **ToPrepare** - Vendor is preparing the order
3. **ToDeliver** - Order is ready for delivery/pickup
4. **ToReceive** - Order is being delivered or ready for pickup
5. **Accomplished** - Order completed

## Database Changes

### Order Model
- `UserId`: Links to the user who placed the order
- `StallId`: Links to the stall where the order was placed
- `Status`: Current order status (Pending, ToPrepare, etc.)
- `FulfillmentMethod`: Pickup or Delivery
- `DeliveryAddress`: Required for delivery orders
- `Notes`: Additional order notes
- `TotalPrice`: Total order amount
- `CreatedAt`: Order creation timestamp

### OrderItem Model
- `OrderId`: Links to the parent order
- `ProductId`: Links to the product
- `Quantity`: Number of items ordered
- `PriceEach`: Product price at checkout time (snapshot)

## Business Rules

1. **One Stall Per Order**: Orders can only contain items from a single stall
2. **Price Snapshot**: Product prices are captured at checkout time
3. **Cart Cleanup**: Cart items are removed after successful order creation
4. **Availability Check**: Products must be available at checkout time
5. **Delivery Address**: Required when fulfillment method is delivery

## Error Handling

- **Invalid Cart Items**: Returns 400 if cart items don't exist or don't belong to user
- **Unavailable Products**: Returns 400 if any products are no longer available
- **Missing Delivery Address**: Returns 400 if delivery method selected without address
- **Multiple Stalls**: Returns 400 if trying to checkout items from multiple stalls

## Testing

Unit tests are included in `Tests/OrderServiceTests.cs` covering:
- Successful order creation
- Delivery address validation
- Invalid cart item handling
- Unavailable product handling

## Future Enhancements

1. **Multi-Stall Orders**: Support for checking out items from multiple stalls
2. **Order Status Updates**: API endpoints for updating order status
3. **Delivery Integration**: Integration with delivery partner system
4. **Payment Processing**: Integration with payment gateways
5. **Order Notifications**: Real-time notifications for order status changes 