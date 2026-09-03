## Demo — Sprint 2

The Sprint 2 demo focuses on the authentication, role-based authorization, ownership checks, and middleware behavior implemented in the Cardiac Patient Monitoring System.

### Authentication Flow

The authentication flow was tested through Postman:

1. Register a new Patient account.
2. Login using the registered credentials.
3. Receive a JWT containing the user identity, role, and `PatientId`.
4. Use the JWT to access a protected Patient endpoint successfully.

This demonstrates the complete flow from Patient registration to authenticated API access.

### Role-Based Authorization

The API was tested using the three main roles:

* **Admin** — full administrative access to protected resources.
* **Doctor** — access to permitted resources according to the Doctor role and patient relationships.
* **Patient** — access limited to the Patient's own data and permitted operations.

Admin-only operations were also tested with a Patient token and correctly returned **403 Forbidden**.

### Ownership Checks

Ownership authorization was tested using two different Patient accounts.

A Patient was prevented from accessing another Patient's medication record. The request correctly returned **403 Forbidden**, demonstrating that authentication alone is not sufficient and that data ownership is also enforced.

### Middleware

The request logging middleware was tested through a protected API request.

The application successfully logged:

* HTTP method
* Request path
* Response status code

The global exception middleware was also reviewed to ensure that unhandled exceptions are converted into a standardized **500 Internal Server Error** response using `ProblemDetails` without exposing internal exception details.

### Demo Result

The Sprint 2 demo confirms that:

* Patient registration and login work correctly.
* JWT authentication is functioning.
* Roles are correctly enforced.
* Admin-only endpoints reject unauthorized roles.
* Patient ownership restrictions are enforced.
* Middleware handles logging and unexpected exceptions centrally.
* The complete authentication and authorization flow works together as expected.
