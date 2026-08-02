## Car Dealership Management System

### Project Overview

The Car Dealership Management API provides a structured way to manage vehicles, customers, and purchase orders. It follows REST principles by modeling the system around resources, using standard HTTP methods, returning appropriate status codes, and applying a clear versioning convention.

---

## 1. Domain and Core Resources

### Domain

**Car Dealership Management System**

The system manages the dealership’s vehicle inventory, customer records, and purchase orders.

### Core Resources

| Resource | Description |
| --- | --- |
| `Cars` | Vehicles available in the dealership inventory |
| `Customers` | Customers registered with the dealership |
| `Orders` | Vehicle purchase orders placed by customers |

### Base Resource URLs

```
/api/v1/cars
/api/v1/customers
/api/v1/orders
```

All resources use plural nouns to maintain consistent REST naming conventions.

---

## 2. Primary Resource Endpoints

The primary resource is `Cars`. It represents the vehicles available in the dealership’s inventory.

| Operation | HTTP Method | Endpoint | Description |
| --- | --- | --- | --- |
| List cars | `GET` | `/api/v1/cars` | Returns all cars in the inventory |
| Get a car | `GET` | `/api/v1/cars/{id}` | Returns a specific car by ID |
| Create a car | `POST` | `/api/v1/cars` | Adds a new car to the inventory |
| Update a car | `PUT` | `/api/v1/cars/{id}` | Updates an existing car |
| Delete a car | `DELETE` | `/api/v1/cars/{id}` | Removes a car from the inventory |

### Example Car Representation

```json
{
  "id": 15,
  "brand": "Kia",
  "model": "Sportage",
  "year": 2024,
  "color": "Black",
  "price": 32000,
  "status": "Available"
}
```

---

## 3. Nested Resource Endpoint

A customer can place multiple purchase orders. This relationship is represented using a nested resource endpoint:

```
GET /api/v1/customers/{customerId}/orders
```

This endpoint returns all purchase orders belonging to a specific customer.

### Example Request

```
GET /api/v1/customers/8/orders
```

### Example Response

```json
[
  {
    "id": 101,
    "customerId": 8,
    "carId": 15,
    "orderDate": "2026-08-02",
    "totalPrice": 32000,
    "status": "Pending"
  },
  {
    "id": 108,
    "customerId": 8,
    "carId": 24,
    "orderDate": "2026-07-15",
    "totalPrice": 28500,
    "status": "Completed"
  }
]
```

---

## 4. HTTP Status Codes

| Operation | Endpoint | Success Response | Error Response |
| --- | --- | --- | --- |
| List all cars | `GET /api/v1/cars` | `200 OK` | `500 Internal Server Error` |
| Get a specific car | `GET /api/v1/cars/{id}` | `200 OK` | `404 Not Found` |
| Create a new car | `POST /api/v1/cars` | `201 Created` | `400 Bad Request` |
| Update an existing car | `PUT /api/v1/cars/{id}` | `200 OK` | `400 Bad Request` or `404 Not Found` |
| Delete an existing car | `DELETE /api/v1/cars/{id}` | `204 No Content` | `404 Not Found` |
| Get a customer’s orders | `GET /api/v1/customers/{customerId}/orders` | `200 OK` | `404 Not Found` |

### Example: Successful Creation

#### Request

```
POST /api/v1/cars
Content-Type: application/json
```

```json
{
  "brand": "Hyundai",
  "model": "Tucson",
  "year": 2025,
  "color": "White",
  "price": 34000,
  "status": "Available"
}
```

#### Response

```
201 Created
Location: /api/v1/cars/16
```

```json
{
  "id": 16,
  "brand": "Hyundai",
  "model": "Tucson",
  "year": 2025,
  "color": "White",
  "price": 34000,
  "status": "Available"
}
```

### Example: Resource Not Found

#### Request

```
GET /api/v1/cars/999
```

#### Response

```
404 Not Found
```

```json
{
  "message": "Car with ID 999 was not found."
}
```

### Example: Invalid Input

#### Request

```
POST /api/v1/cars
Content-Type: application/json
```

```json
{
  "brand": "",
  "model": "Tucson",
  "year": 1990,
  "price": -500
}
```

#### Response

```
400 Bad Request
```

```json
{
  "message": "The submitted car data is invalid.",
  "errors": {
    "brand": ["Brand is required."],
    "year": ["Year must be valid."],
    "price": ["Price must be greater than zero."]
  }
}
```

If no cars are currently available, `GET /api/v1/cars` returns `200 OK` with an empty collection:

```json
[]
```

---

## 5. API Versioning Convention

The API uses URL-based versioning. The version is included as a segment in every endpoint:

```
/api/v1/cars
```

All endpoints in the current project use version 1:

```
GET    /api/v1/cars
GET    /api/v1/cars/{id}
POST   /api/v1/cars
PUT    /api/v1/cars/{id}
DELETE /api/v1/cars/{id}

GET    /api/v1/customers/{customerId}/orders
```

If future requirements introduce breaking changes, a new API version will be created:

```
/api/v2/cars
```

Existing clients can continue using `v1` until they are ready to migrate to `v2`.

---

## Final Resource Map

| Resource | HTTP Method | Endpoint |
| --- | --- | --- |
| Cars | `GET` | `/api/v1/cars` |
| Cars | `GET` | `/api/v1/cars/{id}` |
| Cars | `POST` | `/api/v1/cars` |
| Cars | `PUT` | `/api/v1/cars/{id}` |
| Cars | `DELETE` | `/api/v1/cars/{id}` |
| Customer Orders | `GET` | `/api/v1/customers/{customerId}/orders` |
- BinX tech internship —>Backend
- Name: Mohammad Abu Alhaija