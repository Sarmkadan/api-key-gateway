// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides extension methods for <see cref="ApiKeyServiceTests"/> to facilitate test execution.
/// </summary>
public static class ApiKeyServiceTestsExtensions
{
    /// <summary>
    /// Executes the disable key tests for the provided <see cref="ApiKeyServiceTests"/> instance.
    /// </summary>
    /// <param name="sut">The <see cref="ApiKeyServiceTests"/> instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sut"/> is null.</exception>
    public static async Task RunDisableKeyTests(this ApiKeyServiceTests sut)
    {
        ArgumentNullException.ThrowIfNull(sut);
        await sut.DisableKeyAsync_KeyNotFoundInRepository_ReturnsFalse();
        await sut.DisableKeyAsync_ExistingKey_DisablesAndPersists();
    }

    /// <summary>
    /// Executes the revoke key tests for the provided <see cref="ApiKeyServiceTests"/> instance.
    /// </summary>
    /// <param name="sut">The <see cref="ApiKeyServiceTests"/> instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sut"/> is null.</exception>
    public static async Task RunRevokeKeyTests(this ApiKeyServiceTests sut)
    {
        ArgumentNullException.ThrowIfNull(sut);
        await sut.RevokeKeyAsync_ExistingKey_SetsRevokedStatusAndPersists();
    }

    /// <summary>
    /// Executes all fact-based tests for the provided <see cref="ApiKeyServiceTests"/> instance.
    /// </summary>
    /// <param name="sut">The <see cref="ApiKeyServiceTests"/> instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sut"/> is null.</exception>
    public static async Task RunAllFactTests(this ApiKeyServiceTests sut)
    {
        ArgumentNullException.ThrowIfNull(sut);
        await sut.CreateKeyAsync_ValidArguments_CreatesKeyWithExpectedPrefix();
        await sut.DisableKeyAsync_KeyNotFoundInRepository_ReturnsFalse();
        await sut.DisableKeyAsync_ExistingKey_DisablesAndPersists();
        await sut.ValidateKeyAsync_EmptyKeyValue_ThrowsUnauthorizedException();
        await sut.GetConsumerKeysAsync_EmptyConsumerId_ReturnsEmptyListWithoutQueryingRepository();
        await sut.RevokeKeyAsync_ExistingKey_SetsRevokedStatusAndPersists();
    }
}
