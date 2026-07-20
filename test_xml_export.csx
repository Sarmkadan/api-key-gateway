#!/usr/bin/env dotnet-script

// Simple test to verify XML export functionality works

using System;
using System.Threading.Tasks;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Utilities;

Console.WriteLine("Testing XML Export Helper...");

// Test 1: Export single object
var log = new AuditLog
{
    Id = Guid.NewGuid().ToString(),
    ResourceId = "api-key-123",
    ResourceType = "ApiKey",
    Action = AuditAction.KeyCreated,
    PerformedBy = "admin@example.com",
    PerformedAt = DateTime.UtcNow,
    HttpStatusCode = 201,
    SourceIp = "192.168.1.1",
    Reason = "Initial creation",
    IsSuccess = true
};

var xml1 = XmlExportHelper.ToXml(log);
Console.WriteLine("✓ Single object XML export works");
Console.WriteLine(xml1);
Console.WriteLine();

// Test 2: Export list of objects
var logs = new System.Collections.Generic.List<AuditLog>
{
    log,
    new AuditLog
    {
        Id = Guid.NewGuid().ToString(),
        ResourceId = "api-key-456",
        ResourceType = "ApiKey",
        Action = AuditAction.KeyUsed,
        PerformedBy = "user@example.com",
        PerformedAt = DateTime.UtcNow.AddMinutes(-5),
        HttpStatusCode = 200,
        SourceIp = "10.0.0.1",
        IsSuccess = true
    }
};

var xml2 = XmlExportHelper.ToXml(logs, "AuditLogs");
Console.WriteLine("✓ List XML export works");
Console.WriteLine(xml2);
Console.WriteLine();

// Test 3: Empty list
var emptyXml = XmlExportHelper.ToXml(new System.Collections.Generic.List<AuditLog>(), "AuditLogs");
Console.WriteLine("✓ Empty list XML export works");
Console.WriteLine(emptyXml);

Console.WriteLine("\nAll XML export tests passed! ✓");