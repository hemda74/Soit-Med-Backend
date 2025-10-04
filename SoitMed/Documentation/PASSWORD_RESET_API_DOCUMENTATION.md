# SoitMed Password Reset API Documentation

## Overview

This document provides comprehensive information about the password reset functionality in the SoitMed system, including email verification and password update services.

## Table of Contents

1. [API Endpoints](#api-endpoints)
2. [Request/Response Examples](#requestresponse-examples)
3. [Email Configuration](#email-configuration)
4. [Testing Guide](#testing-guide)
5. [Error Handling](#error-handling)
6. [Security Considerations](#security-considerations)

## API Endpoints

### Step 1: Forgot Password (Check Email & Send Code)

**Endpoint:** `POST /api/Account/forgot-password`

**Description:** Checks if email exists in database and sends verification code if found.

**Request Body:**

```json
{
	"email": "user@example.com"
}
```

**Response (Email Found):**

```json
{
	"success": true,
	"message": "Verification code sent to your email address.",
	"email": "user@example.com"
}
```

**Response (Email Not Found):**

```json
{
	"success": false,
	"message": "Email address not found in our system."
}
```

**Status Codes:**

- `200 OK` - Email found, code sent successfully
- `400 Bad Request` - Email not found or invalid request data
- `500 Internal Server Error` - Server error

### Step 2: Verify Code (Unlock Password Change)

**Endpoint:** `POST /api/Account/verify-code`

**Description:** Verifies the code and returns a reset token to allow password change.

**Request Body:**

```json
{
	"email": "user@example.com",
	"code": "123456"
}
```

**Response (Valid Code):**

```json
{
	"success": true,
	"message": "Verification code is valid. You can now change your password.",
	"resetToken": "CfDJ8...",
	"email": "user@example.com"
}
```

**Response (Invalid Code):**

```json
{
	"success": false,
	"message": "Invalid or expired verification code. Please request a new code."
}
```

**Status Codes:**

- `200 OK` - Code verified, reset token provided
- `400 Bad Request` - Invalid or expired code
- `500 Internal Server Error` - Server error

### Step 3: Reset Password (Change Password)

**Endpoint:** `POST /api/Account/reset-password`

**Description:** Changes the user's password using the reset token from step 2.

**Request Body:**

```json
{
	"email": "user@example.com",
	"resetToken": "CfDJ8...",
	"newPassword": "NewPassword123!",
	"confirmPassword": "NewPassword123!"
}
```

**Response:**

```json
{
	"success": true,
	"message": "Password has been reset successfully. You can now login with your new password."
}
```

**Status Codes:**

- `200 OK` - Password reset successfully
- `400 Bad Request` - Invalid token or password requirements not met
- `500 Internal Server Error` - Server error

### 4. Change Password (Existing)

**Endpoint:** `POST /api/Account/change-password`

**Description:** Changes password for authenticated users (requires current password).

**Headers:** `Authorization: Bearer <token>`

**Request Body:**

```json
{
	"currentPassword": "CurrentPassword123!",
	"newPassword": "NewPassword123!",
	"confirmPassword": "NewPassword123!"
}
```

**Response:**

```json
{
	"success": true,
	"message": "Password changed successfully."
}
```

## Request/Response Examples

### Complete Password Reset Flow

#### Step 1: Check Email & Send Code

```bash
curl -X POST "http://localhost:5117/api/Account/forgot-password" \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com"}'
```

**Expected Response:**

```json
{
	"success": true,
	"message": "Verification code sent to your email address.",
	"email": "user@example.com"
}
```

#### Step 2: Verify Code & Get Reset Token

```bash
curl -X POST "http://localhost:5117/api/Account/verify-code" \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com", "code": "123456"}'
```

**Expected Response:**

```json
{
	"success": true,
	"message": "Verification code is valid. You can now change your password.",
	"resetToken": "CfDJ8...",
	"email": "user@example.com"
}
```

#### Step 3: Reset Password with Token

```bash
curl -X POST "http://localhost:5117/api/Account/reset-password" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "resetToken": "CfDJ8...",
    "newPassword": "NewPassword123!",
    "confirmPassword": "NewPassword123!"
  }'
```

**Expected Response:**

```json
{
	"success": true,
	"message": "Password has been reset successfully. You can now login with your new password."
}
```

## Email Configuration

### Required Settings in appsettings.json

```json
{
	"EmailSettings": {
		"SmtpHost": "smtp.gmail.com",
		"SmtpPort": "587",
		"SmtpUsername": "your-email@gmail.com",
		"SmtpPassword": "your-app-password",
		"FromEmail": "your-email@gmail.com",
		"FromName": "SoitMed System"
	}
}
```

### Gmail Setup Instructions

1. **Enable 2-Factor Authentication** on your Gmail account
2. **Generate App Password:**
      - Go to Google Account settings
      - Security → 2-Step Verification → App passwords
      - Generate a new app password for "Mail"
      - Use this password in `SmtpPassword`

### Email Template

The system sends HTML emails with:

- Professional SoitMed branding
- 6-digit verification code
- 15-minute expiry notice
- Security warnings

## Testing Guide

### Prerequisites

1. **Start the Application:**

      ```bash
      cd SoitMed
      dotnet run
      ```

2. **Configure Email Settings:**

      - Update `appsettings.json` with your email credentials
      - Ensure SMTP settings are correct

3. **Have a Test User:**
      - Create a user account in the system
      - Note the email address for testing

### Manual Testing Steps

#### Test 1: Forgot Password Flow

1. **Send Forgot Password Request:**

      ```bash
      curl -X POST "http://localhost:5117/api/Account/forgot-password" \
        -H "Content-Type: application/json" \
        -d '{"email": "test@example.com"}'
      ```

2. **Expected Result:**

      - Response: `{"success": true, "message": "If the email address exists in our system, you will receive a password reset code."}`
      - Check email inbox for verification code

3. **Verify Code:**

      ```bash
      curl -X POST "http://localhost:5117/api/Account/verify-code" \
        -H "Content-Type: application/json" \
        -d '{"email": "test@example.com", "code": "ACTUAL_CODE_FROM_EMAIL"}'
      ```

4. **Reset Password:**
      ```bash
      curl -X POST "http://localhost:5117/api/Account/reset-password" \
        -H "Content-Type: application/json" \
        -d '{
          "email": "test@example.com",
          "verificationCode": "ACTUAL_CODE_FROM_EMAIL",
          "newPassword": "NewPassword123!",
          "confirmPassword": "NewPassword123!"
        }'
      ```

#### Test 2: Error Scenarios

1. **Invalid Email:**

      ```bash
      curl -X POST "http://localhost:5117/api/Account/forgot-password" \
        -H "Content-Type: application/json" \
        -d '{"email": "nonexistent@example.com"}'
      ```

2. **Invalid Verification Code:**

      ```bash
      curl -X POST "http://localhost:5117/api/Account/reset-password" \
        -H "Content-Type: application/json" \
        -d '{
          "email": "test@example.com",
          "verificationCode": "000000",
          "newPassword": "NewPassword123!",
          "confirmPassword": "NewPassword123!"
        }'
      ```

3. **Expired Code:**
      - Wait 15+ minutes after requesting password reset
      - Try to use the code (should fail)

### Frontend Integration Examples

#### React/JavaScript Example

```javascript
// Forgot Password
const forgotPassword = async (email) => {
	try {
		const response = await fetch('/api/Account/forgot-password', {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({ email }),
		});
		const data = await response.json();
		return data;
	} catch (error) {
		console.error('Error:', error);
		throw error;
	}
};

// Verify Code
const verifyCode = async (email, code) => {
	try {
		const response = await fetch('/api/Account/verify-code', {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({ email, code }),
		});
		const data = await response.json();
		return data;
	} catch (error) {
		console.error('Error:', error);
		throw error;
	}
};

// Reset Password
const resetPassword = async (
	email,
	verificationCode,
	newPassword,
	confirmPassword
) => {
	try {
		const response = await fetch('/api/Account/reset-password', {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({
				email,
				verificationCode,
				newPassword,
				confirmPassword,
			}),
		});
		const data = await response.json();
		return data;
	} catch (error) {
		console.error('Error:', error);
		throw error;
	}
};
```

#### Angular/TypeScript Example

```typescript
// Password Reset Service
@Injectable({
	providedIn: 'root',
})
export class PasswordResetService {
	private baseUrl = '/api/Account';

	constructor(private http: HttpClient) {}

	forgotPassword(email: string): Observable<any> {
		return this.http.post(`${this.baseUrl}/forgot-password`, {
			email,
		});
	}

	verifyCode(email: string, code: string): Observable<any> {
		return this.http.post(`${this.baseUrl}/verify-code`, {
			email,
			code,
		});
	}

	resetPassword(
		email: string,
		verificationCode: string,
		newPassword: string,
		confirmPassword: string
	): Observable<any> {
		return this.http.post(`${this.baseUrl}/reset-password`, {
			email,
			verificationCode,
			newPassword,
			confirmPassword,
		});
	}
}
```

## Error Handling

### Common Error Responses

#### 400 Bad Request

```json
{
	"success": false,
	"message": "Invalid request data",
	"errors": {
		"email": ["The Email field is required."],
		"verificationCode": ["The VerificationCode field is required."]
	}
}
```

#### 400 Bad Request - Invalid Code

```json
{
	"success": false,
	"message": "Invalid or expired verification code. Please request a new code."
}
```

#### 500 Internal Server Error

```json
{
	"success": false,
	"message": "An error occurred while processing your request. Please try again later."
}
```

### Frontend Error Handling

```javascript
const handlePasswordReset = async (email) => {
	try {
		const response = await forgotPassword(email);
		if (response.success) {
			// Show success message
			setMessage('Password reset code sent to your email');
		}
	} catch (error) {
		if (error.response?.status === 400) {
			// Handle validation errors
			setError('Please check your email address');
		} else if (error.response?.status === 500) {
			// Handle server errors
			setError('Server error. Please try again later');
		} else {
			// Handle network errors
			setError('Network error. Please check your connection');
		}
	}
};
```

## Security Considerations

### Code Security

- **Expiry Time:** Verification codes expire after 15 minutes
- **Single Use:** Codes are consumed after successful verification
- **Rate Limiting:** Consider implementing rate limiting for forgot password requests
- **Case Insensitive:** Codes are case-insensitive for better UX

### Email Security

- **No User Enumeration:** System doesn't reveal if email exists or not
- **Secure SMTP:** Uses TLS encryption for email transmission
- **App Passwords:** Uses app-specific passwords for Gmail

### Password Requirements

- **Minimum Length:** 8 characters
- **Complexity:** Should include uppercase, lowercase, numbers, and special characters
- **Confirmation:** New password must be confirmed

### Best Practices

1. **Log Security Events:** All password reset attempts are logged
2. **Monitor Failed Attempts:** Track multiple failed verification attempts
3. **Secure Storage:** Verification codes are stored in memory cache only
4. **HTTPS Only:** Use HTTPS in production for all API calls

## Troubleshooting

### Common Issues

1. **Email Not Received:**

      - Check spam folder
      - Verify SMTP configuration
      - Check email address is correct

2. **Code Not Working:**

      - Ensure code is entered within 15 minutes
      - Check for typos in email or code
      - Request a new code if expired

3. **SMTP Errors:**

      - Verify Gmail app password is correct
      - Check 2FA is enabled on Gmail account
      - Ensure SMTP settings are correct

4. **Application Not Starting:**
      - Check database connection
      - Verify all services are registered in Program.cs
      - Check for compilation errors

### Debug Steps

1. **Check Application Logs:**

      ```bash
      dotnet run --verbosity normal
      ```

2. **Test Email Configuration:**

      - Use a simple email test first
      - Verify SMTP credentials

3. **Database Verification:**
      - Ensure user exists in database
      - Check email field is populated

## Support

For technical support or questions about the password reset functionality, please contact the development team or refer to the main API documentation.

---

**Last Updated:** December 2024  
**Version:** 1.0  
**API Version:** v1
