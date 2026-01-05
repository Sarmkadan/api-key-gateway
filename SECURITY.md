# Security Policy

## Reporting a Vulnerability

**Please do NOT open a public GitHub issue for security vulnerabilities.**

We take security seriously and appreciate responsible disclosure. If you discover a security vulnerability, please report it through one of the following methods:

### GitHub Private Vulnerability Reporting (Recommended)

Visit: https://github.com/sarmkadan/api-key-gateway/security/advisories/new

This allows you to report the vulnerability privately to the maintainers.

### Email

Email your report to: **rutova2@gmail.com**

Include:
- Description of the vulnerability
- Steps to reproduce (if applicable)
- Affected versions
- Potential impact
- Suggested fix (if you have one)

## Response Timeline

- **48 hours**: Initial acknowledgment of your report
- **1 week**: Detailed assessment and timeline for fix
- **Public disclosure**: Coordinated with you after a fix is available

## Supported Versions

Security updates are provided for:

| Version | Supported          |
| ------- | ------------------ |
| 1.x     | ✅ Yes             |
| < 1.0   | ❌ No              |

We recommend upgrading to the latest version to receive all security patches and improvements.

## Security Best Practices

When deploying API Key Gateway:

1. **Use HTTPS** - Always deploy with TLS/SSL in production
2. **Secure Secrets** - Store database credentials and keys in secure vaults (e.g., AWS Secrets Manager, Azure Key Vault)
3. **Database Security** - Use strong credentials and limit network access to your database
4. **Regular Updates** - Keep your deployment updated with the latest releases
5. **Monitor Logs** - Review audit logs regularly for suspicious activity
6. **Rate Limiting** - Configure appropriate rate limits for your use case
7. **API Key Rotation** - Implement regular key rotation policies
8. **Access Control** - Restrict admin endpoint access to authorized users/IPs
9. **Enable Auditing** - Use the built-in audit logging to track API access

## Vulnerability Disclosure

Once a security issue is confirmed and a fix is available:

1. A patch release will be published
2. Security advisories will be posted on GitHub
3. The issue will be documented in release notes
4. Credit will be given to the reporter (unless requested otherwise)

Thank you for helping keep API Key Gateway secure!

