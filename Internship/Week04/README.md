# Week 4 - Security & Validation Summary

## Overview

This week I continued building on the same **Car Dealership API** from the previous weeks.

The main focus of Week 4 was security.

At the beginning of the week, the API could already work with the database and perform CRUD operations, but it did not have a complete way to identify users, control their permissions, validate their input, or protect the API from some common types of abuse.

Instead of trying to add everything at once, I added a new security layer each day.

The week progressed like this:

```text
Identity
   ↓
JWT Authentication
   ↓
Authorization & Roles
   ↓
Input Validation
   ↓
API Hardening
```

By the end of the week, these layers were working together inside the same API.

---

# Day 1 - ASP.NET Core Identity

I started the week by adding **ASP.NET Core Identity**.

The main goal was to stop treating users as simple records and use a proper system for managing accounts and passwords.

I connected Identity to the existing Entity Framework Core database:

```csharp
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();
```

I also created a registration endpoint:

```text
POST /api/auth/register
```

When a user registers, Identity handles creating the account and storing the password securely instead of saving the password directly.

The basic flow became:

```text
Register Request
      ↓
ASP.NET Core Identity
      ↓
Create User
      ↓
Hash Password
      ↓
Store User in Database
```

This gave me the user system that I needed for the rest of the week's authentication work.

---

# Day 2 - JWT Authentication

After users could register, the next step was allowing them to log in.

I added a Login endpoint that checks the user's email and password using Identity.

If the credentials are correct, the API generates a **JWT**.

```text
Login
  ↓
Check Email & Password
  ↓
Credentials Valid?
  ↓
Generate JWT
  ↓
Return Token
```

I configured JWT Bearer Authentication in `Program.cs` so the API can validate tokens sent with later requests.

The API checks:

```text
Issuer
Audience
Expiration
Signing Key
```

The JWT is then sent with protected requests using:

```text
Authorization: Bearer <token>
```

This helped me understand that logging in and accessing a protected endpoint are actually two separate steps.

The Login endpoint creates the token, while JWT Bearer Authentication validates that token when it comes back with another request.

---

# Day 3 - Authorization & Roles

Once authentication was working, I moved to **authorization**.

The difference became much clearer during this part:

```text
Authentication
→ Who are you?

Authorization
→ What are you allowed to do?
```

I created two roles:

```text
Admin
User
```

Some routes only require the user to be authenticated:

```csharp
[Authorize]
```

Other operations are restricted to Admin users:

```csharp
[Authorize(Roles = "Admin")]
```

I also added a custom permission:

```text
Permission = ManageCars
```

and created a policy for it:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageCars", policy =>
    {
        policy.RequireClaim("Permission", "ManageCars");
    });
});
```

This allowed me to protect an endpoint using:

```csharp
[Authorize(Policy = "CanManageCars")]
```

So instead of only checking whether someone is logged in, the API can also check what that specific user is allowed to do.

---

# Day 4 - Input Validation with FluentValidation

After authentication and authorization were working, I focused on the data being sent to the API.

Before this, some validation was done manually inside the controller using conditions such as:

```csharp
if (string.IsNullOrWhiteSpace(...))
```

This worked, but as more validation rules were added, the controller started becoming responsible for too many things.

I introduced **FluentValidation** and moved those rules into separate validator classes.

I created request models such as:

```text
CreateCarRequest
UpdateCarRequest
```

and validators such as:

```text
CreateCarValidator
UpdateCarValidator
```

For example:

```csharp
RuleFor(x => x.Price)
    .GreaterThan(0)
    .WithMessage("Car price must be greater than 0.");
```

and:

```csharp
RuleFor(x => x.VIN)
    .NotEmpty()
    .Length(17);
```

This made an important difference.

A value can have the correct C# data type and still not make sense for the application.

For example:

```text
Price = -500
```

is a valid decimal value, but it is not a valid car price.

The request flow became:

```text
Request
   ↓
Request Model
   ↓
FluentValidation
   ↓
Valid?
 /     \
No      Yes
↓        ↓
400   Controller
```

The API also started returning structured validation errors, which would make it easier for a frontend to know exactly which input field is invalid.

---

# Day 5 - API Hardening

On the final day, I added some additional security around the API itself.

The main areas were:

```text
Rate Limiting
CORS
HTTPS / HSTS
SQL Injection Review
```

These protections are different from JWT and authorization because they protect other parts of the request flow.

---

## Rate Limiting

I added ASP.NET Core's built-in Rate Limiting.

I created two policies:

```text
General API
→ 20 requests per minute

Login
→ 5 requests per minute
```

I intentionally made Login stricter because an authentication endpoint should not accept unlimited repeated attempts.

For example:

```csharp
options.AddFixedWindowLimiter("login", limiterOptions =>
{
    limiterOptions.PermitLimit = 5;
    limiterOptions.Window = TimeSpan.FromMinutes(1);
    limiterOptions.QueueLimit = 0;
});
```

The Login endpoint uses:

```csharp
[EnableRateLimiting("login")]
```

I also configured rejected requests to return:

```text
429 Too Many Requests
```

I tested this in Postman by repeatedly calling Login.

After exceeding the configured limit, the API returned `429`, confirming that the policy was working.

---

## CORS

I also configured a named CORS policy.

For this project, I allowed:

```text
http://localhost:3000
```

as the frontend origin.

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

I tested two cases in Postman.

The first request used:

```text
Origin: http://localhost:3000
```

and the response contained:

```text
Access-Control-Allow-Origin: http://localhost:3000
```

Then I tested:

```text
Origin: http://evil-site.com
```

The request still showed `204 No Content` in Postman, but the important difference was that the response did **not** contain:

```text
Access-Control-Allow-Origin
```

This test helped me understand CORS better.

Postman does not enforce CORS like a browser does. A browser checks these headers and would prevent frontend JavaScript from reading the response when the origin is not allowed.

---

## HTTPS & HSTS

The project already had HTTPS redirection:

```csharp
app.UseHttpsRedirection();
```

I also added HSTS for non-development environments:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
```

I kept HSTS outside Development because it is mainly intended for production use.

Together, these provide another security layer around the connection itself:

```text
HTTP
 ↓
HTTPS Redirection
 ↓
HTTPS
```

with HSTS helping browsers continue using HTTPS in production.

---

## SQL Injection Review

The last security check was reviewing the project for raw SQL.

I searched the codebase for:

```text
FromSqlRaw
ExecuteSqlRaw
FromSqlInterpolated
```

and found no occurrences.

The project currently uses Entity Framework Core and LINQ for normal database access.

For example:

```csharp
var car = await _context.Cars
    .FirstOrDefaultAsync(c => c.Id == id);
```

This means I am not manually building SQL strings using request values.

It also helped me understand why parameterized database access matters: user input should be treated as data, not as part of the SQL command itself.

---

# Putting the Whole Week Together

The most useful part of this week was seeing how all these concepts connect.

They are not separate replacements for each other.

Each one solves a different problem.

The final request flow looks roughly like this:

```text
Client
  ↓
HTTPS / HSTS
  ↓
CORS
  ↓
Rate Limiting
  ↓
JWT Authentication
  ↓
Authorization
  ↓
FluentValidation
  ↓
Controller
  ↓
Entity Framework Core
  ↓
SQL Server
```

For example, when someone tries to create a car, several things may need to happen before the car reaches the database:

```text
Request arrives
      ↓
Is the origin allowed?
      ↓
Is the request within the rate limit?
      ↓
Is the JWT valid?
      ↓
Does the user have permission?
      ↓
Is the car data valid?
      ↓
Run controller logic
      ↓
Save to database
```

This made the API feel much more like one complete application instead of a collection of separate exercises.

---

# What I Learned This Week

The biggest thing I learned this week is that saying an API is "secure" does not refer to one feature.

JWT alone does not secure everything.

Each layer answers a different question:

```text
Identity
→ How do we manage users?

JWT
→ How does the user prove their identity?

Authorization
→ What is this user allowed to do?

FluentValidation
→ Is the data acceptable?

Rate Limiting
→ Is the client sending too many requests?

CORS
→ Is this browser origin allowed?

HTTPS / HSTS
→ Is the connection protected?

EF Core
→ Are we interacting with the database safely?
```

I also became more comfortable with the idea of separating responsibilities.

Instead of putting everything inside a controller, different parts of the project now have specific jobs:

```text
Identity       → Users

JWT            → Authentication

Policies       → Authorization

DTOs           → Incoming request data

Validators     → Validation rules

Controllers    → API operations

EF Core        → Database access
```

That made the code easier for me to understand and made the project more organized.

---

# Mentor Check-In

For the Week 4 check-in, the main flow I can explain is:

```text
1. A user registers through ASP.NET Core Identity.

2. The user logs in using their email and password.

3. If the credentials are correct, the API generates a JWT.

4. The client sends that JWT as a Bearer Token.

5. ASP.NET Core validates the token.

6. Authorization checks the user's role or permission.

7. FluentValidation checks the request data.

8. Rate Limiting controls excessive requests.

9. CORS controls which browser origins are allowed.

10. HTTPS and HSTS improve transport security.

11. EF Core handles the database operations without manually
    building SQL strings from user input.
```

So if I had to summarize Week 4 in one sentence:

> I started the week with a Car Dealership API that could access its database, and gradually added the layers needed to identify users, control access, validate requests, and harden the API.

---

# Week 4 Final Result

By the end of Week 4, the Car Dealership API includes:

```text
ASP.NET Core Identity
JWT Authentication
Bearer Token Validation
Admin & User Roles
Role-Based Authorization
Permission-Based Authorization
FluentValidation
Create & Update Request DTOs
Structured Validation Errors
Rate Limiting
Stricter Login Rate Limit
429 Too Many Requests
Named CORS Policy
HTTPS Redirection
HSTS
Entity Framework Core
SQL Injection Review
Postman Testing
```

Week 4 connected several concepts that I had previously seen separately and showed me how they work together inside the same backend application.

The final API now has a much clearer security flow:

```text
Identify the user
       ↓
Authenticate the user
       ↓
Authorize the action
       ↓
Validate the input
       ↓
Protect the API
       ↓
Execute the operation safely
```

This completed my Week 4 work on **Identity, JWT Authentication, Authorization, Input Validation, and API Security**.