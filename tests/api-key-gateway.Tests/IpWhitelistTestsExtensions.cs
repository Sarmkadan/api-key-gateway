// © 2024 RedRocket. All rights reserved.
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;

/// <summary>
/// Extension helpers for <see cref="IpWhitelistTests"/> that make test setup and
/// common whitelist operations more concise and expressive.
/// </summary>
namespace ApiKeyGateway.Tests
{
    /// <summary>
    /// Provides useful extension methods for the <see cref="IpWhitelistTests"/> test class.
    /// </summary>
    public static class IpWhitelistTestsExtensions
    {
        /// <summary>
        /// Parses a comma‑separated whitelist string into a read‑only list of trimmed IP addresses.
        /// </summary>
        /// <param name="tests">The test instance (unused, required for extension syntax).</param>
        /// <param name="whitelist">A comma‑separated list of IP addresses; may be <c>null</c> or empty.</param>
        /// <returns>A read‑only list of trimmed, non‑empty IP strings.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> ParseWhitelist(this IpWhitelistTests tests, string? whitelist)
        {
            ArgumentNullException.ThrowIfNull(tests);

            return whitelist is null
                ? Array.Empty<string>()
                : whitelist
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => p.Length > 0)
                    .ToArray();
        }

        /// <summary>
        /// Creates a new <see cref="ApiKey"/> instance pre‑populated with the supplied identifier
        /// and whitelist string.
        /// </summary>
        /// <param name="tests">The test instance (unused, required for extension syntax).</param>
        /// <param name="id">The identifier for the API key. Must not be <c>null</c> or empty.</param>
        /// <param name="whitelist">A comma‑separated whitelist string; may be <c>null</c>.</param>
        /// <returns>A new <see cref="ApiKey"/> with the given values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is <c>null</c> or empty.</exception>
        public static ApiKey CreateApiKey(this IpWhitelistTests tests, string id, string? whitelist = null)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentException.ThrowIfNullOrEmpty(id);

            return new ApiKey
            {
                Id = id,
                IpWhitelist = whitelist
            };
        }

        /// <summary>
        /// Sets up a fresh <see cref="ApiKeyService"/> together with its mocked dependencies.
        /// </summary>
        /// <param name="tests">The test instance (unused, required for extension syntax).</param>
        /// <returns>
        /// A tuple containing the repository mock, logger mock, and the instantiated service.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static (Mock<IApiKeyRepository> RepositoryMock, Mock<ILogger<ApiKeyService>> LoggerMock, ApiKeyService Service) SetupService(this IpWhitelistTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var repositoryMock = new Mock<IApiKeyRepository>();
            var loggerMock = new Mock<ILogger<ApiKeyService>>();
            var service = new ApiKeyService(repositoryMock.Object, loggerMock.Object);

            return (repositoryMock, loggerMock, service);
        }
    }
}
