# Week 3 - Day 5
## Testing & Documenting the API with Postman

### Overview

Today we finalized the Car Management API by building a complete Postman collection, organizing all requests, testing both successful and failure scenarios, and documenting the API in a way that makes it easy to share and reuse. We also configured a Postman Environment using variables to make the collection portable across different environments.

---

# What We Completed

## 1. Organized the Postman Collection

Created a structured Postman Collection for the Cars API and grouped all endpoints in a single resource folder.

```
Cars
│
├── GET - Get All Cars
├── GET - Get Car By Id
├── GET - Get Car By Id (Not Found)
├── POST - Create Car
├── POST - Create Car (Invalid)
├── PUT - Update Car
├── PUT - Update Car (Invalid)
├── PUT - Update Car (Not Found)
├── DELETE - Delete Car
└── DELETE - Delete Car (Not Found)
```

This organization makes the API easier to test, maintain, and share with other developers.

---

## 2. Tested Success & Error Paths

Every endpoint was tested using both expected and unexpected inputs.

### Success Examples

| Endpoint | Expected Status |
|----------|-----------------|
| GET /api/cars | 200 OK |
| GET /api/cars/{id} | 200 OK |
| POST /api/cars | 201 Created |
| PUT /api/cars/{id} | 204 No Content |
| DELETE /api/cars/{id} | 204 No Content |

### Error Examples

| Scenario | Expected Status |
|----------|-----------------|
| Car not found | 404 Not Found |
| Invalid request body | 400 Bad Request |

Testing error responses is just as important as testing successful ones because real clients don't always send valid data.

---

## 3. Added Postman Test Scripts

Added automated Postman test scripts to verify responses after each request.

### Example - Create Car

```javascript
pm.test("Status code is 201", function () {
    pm.response.to.have.status(201);
});

pm.test("Response has an id", function () {
    const jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("id");
});
```

### Example - Not Found

```javascript
pm.test("Status code is 404", function () {
    pm.response.to.have.status(404);
});
```

### Example - Delete

```javascript
pm.test("Status code is 204", function () {
    pm.response.to.have.status(204);
});
```

These tests allow Postman to automatically verify that the API returns the expected status codes.

---

## 4. Created a Postman Environment

Instead of hardcoding the server URL in every request, a reusable environment variable was created.

Environment Variable

```
baseUrl = http://localhost:5220
```

Requests now use:

```
{{baseUrl}}/api/cars
```

instead of

```
http://localhost:5220/api/cars
```

This makes it easy to switch between Local, Staging, or Production environments without editing every request.

---

## Week 3 Deliverables

- REST API Design Document
- Database ER Diagram (ERD)
- Entity Framework Core Code-First Database
- CRUD API Endpoints
- Complete Postman Collection
- Automated Postman Test Scripts
- Postman Environment using Variables

---

## Resources

### GitHub Repository

> Add your GitHub repository link here.

---

### REST API Design (Notion)

> Add your Notion REST Design link here.

---

### Exported Postman Collection

> Add the exported Postman Collection (.json) file or its GitHub link here.

---

## Week 3 Reflection

This week focused on building a complete backend workflow, starting with REST API design, then database modeling using Entity Framework Core, implementing CRUD operations, and finally validating and documenting the API using Postman.

By the end of the week, the project evolved from an API design into a functional and testable backend application with organized documentation, automated endpoint verification, and reusable testing environments.