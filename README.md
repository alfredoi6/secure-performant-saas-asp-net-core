# Secure Performant SaaS ASP.NET Core

This repository demonstrates how to build a **highly secure, multi-tenant SaaS application** in **ASP.NET Core**. Key areas of focus include:

1. **Tenant or Company Data in Authentication Cookies**: Embedding multi-tenant details (e.g., Company ID, subscription status) as claims.
2. **Single Sign-On (SSO)**: Managing extended user information for multi-tenant routing and subscription checks during SSO logins.
3. **Masking ASP.NET Core Footprint**: Techniques to rename cookies and headers so attackers cannot easily identify your technology stack.
4. **Understanding Cookie Encryption**: How ASP.NET Core Identity encrypts and signs authentication cookies to protect sensitive data.

---

## Overview

Modern multi-tenant SaaS applications often need to:
- Keep tenant or company data isolated and secure.
- Offer smooth SSO integration for users across multiple services.
- Improve performance by reducing repetitive database lookups for tenant information.
- Avoid exposing framework details through default cookie names or HTTP headers.
- Understand how to handle updated user data mid-session and refresh claims when necessary.

---

## Features

- **Multi-Tenant Setup**: Strategies for storing tenant or company-specific information within claims to differentiate user sessions.
- **SSO Support**: Extends the sign-in process to gather and store user-tenant data after an SSO login handshake.
- **Performance Boost via Claims**: Reading tenant data from claims (instead of querying the database on each request) can significantly reduce overhead.
- **Refreshing Claims**: Guidance on ensuring user data changes (e.g., subscription updates) are reflected in the user’s session even if they remain logged in.
- **Security Hardening**:
  - Renaming cookies to mask `.AspNetCore` defaults.
  - Modifying or removing server headers to hide framework signatures.
- **Encryption & Integrity**: Explains how ASP.NET Core Identity uses the Data Protection system (AES encryption and HMAC signing) to keep your authentication cookies secure.

---

## Getting Started

1. **Clone the repository** to your local machine.
2. **Restore and build** the solution using the .NET CLI.
3. **Configure your database settings** if necessary (e.g., `appsettings.json`).
4. **Run the application** and navigate to the provided localhost URL.

---

## Tenant-Aware Authentication & SSO

This project includes a **Custom Claims Principal Factory** that enriches the user identity with tenant or company-specific data. By placing critical information directly into claims, you can avoid repeated database lookups and quickly perform multi-tenant checks on every request or in SSO scenarios.

---

### Performance Benefits of Claims

Storing often-used data (such as subscription status, company roles, or user profile details) in claims can improve performance by eliminating frequent queries. This approach is especially beneficial when a large number of requests target tenant or user information that rarely changes.

---

### Refreshing Claims When Data Changes

When user or tenant data does change while a user is logged in (e.g., if a subscription expires mid-session), the application must refresh claims. ASP.NET Core Identity provides mechanisms to invalidate or refresh cookies to ensure users have accurate session details.

---

## Hiding ASP.NET Core Indicators

### Renaming Cookie Names

By default, ASP.NET Core uses cookie names like `.AspNetCore.Cookies`. Changing these to a generic name helps you avoid giving away framework details.

### Customizing Headers

ASP.NET Core can expose server info in headers by default. Removing or modifying these headers makes it harder for attackers to detect your environment or potential vulnerabilities.

---

## Encryption of ASP.NET Core Identity Cookies

ASP.NET Core Identity cookies are protected by the **Data Protection** system, which provides:

- **Encryption**: Cookie contents are encrypted (typically AES) to prevent direct reading of claims or user data.
- **Signing**: Each cookie is signed (HMAC) to prevent tampering. Any modification by a malicious party invalidates the cookie.
- **Key Rotation**: Keys are regularly rotated, enhancing security by reducing the lifetime of any single encryption key.

This means even though you store user or tenant data in claims within the cookie, it remains secure and unreadable to third parties.

---

## License

This project is licensed under the MIT License. Feel free to use and modify this code base for your own SaaS project.

---

**Stay secure and happy coding!**
