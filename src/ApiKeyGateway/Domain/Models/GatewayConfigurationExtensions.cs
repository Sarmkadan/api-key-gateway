using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Domain.Models
{
	/// <summary>
	/// Extension methods that add useful behaviour to <see cref="GatewayConfiguration"/>.
	/// </summary>
	public static class GatewayConfigurationExtensions
	{
		/// <summary>
		/// Retrieves a custom setting from the <see cref="GatewayConfiguration.CustomSettings"/> dictionary.
		/// </summary>
		/// <param name="config">The configuration instance.</param>
		/// <param name="key">The key of the setting to retrieve.</param>
		/// <returns>The setting value, or <c>null</c> if the key does not exist.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="config"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="key"/> is <c>null</c> or empty.</exception>
		public static string? GetCustomSetting(this GatewayConfiguration config, string key)
		{
			ArgumentNullException.ThrowIfNull(config);
			ArgumentException.ThrowIfNullOrEmpty(key);

			return config.CustomSettings.TryGetValue(key, out var value) ? value : null;
		}

		/// <summary>
		/// Adds or updates a custom setting in the <see cref="GatewayConfiguration.CustomSettings"/> dictionary.
		/// </summary>
		/// <param name="config">The configuration instance.</param>
		/// <param name="key">The key of the setting to add or update.</param>
		/// <param name="value">The value to assign to the setting.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="config"/> or <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException"><paramref name="key"/> is <c>null</c> or empty.</exception>
		public static void SetCustomSetting(this GatewayConfiguration config, string key, string value)
		{
			ArgumentNullException.ThrowIfNull(config);
			ArgumentException.ThrowIfNullOrEmpty(key);
			ArgumentNullException.ThrowIfNull(value);

			config.CustomSettings[key] = value;
		}

		/// <summary>
		/// Determines whether the configuration enforces secure communication.
		/// </summary>
		/// <param name="config">The configuration instance.</param>
		/// <returns>
		/// <c>true</c> if <see cref="GatewayConfiguration.RequireSsl"/> is enabled and a non‑empty
		/// <see cref="GatewayConfiguration.JwtSecret"/> is provided; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="config"/> is <c>null</c>.</exception>
		public static bool IsSecure(this GatewayConfiguration config)
		{
			ArgumentNullException.ThrowIfNull(config);
			return config.RequireSsl && !string.IsNullOrWhiteSpace(config.JwtSecret);
		}

		/// <summary>
		/// Gets the effective per‑hour rate limit based on the configuration.
		/// </summary>
		/// <param name="config">The configuration instance.</param>
		/// <returns>
		/// The value of <see cref="GatewayConfiguration.DefaultRateLimitPerHour"/> when
		/// <see cref="GatewayConfiguration.EnableRateLimiting"/> is enabled; otherwise,
		/// <see cref="int.MaxValue"/> to indicate no limit.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="config"/> is <c>null</c>.</exception>
		public static int GetEffectiveRateLimit(this GatewayConfiguration config)
		{
			ArgumentNullException.ThrowIfNull(config);
			return config.EnableRateLimiting ? config.DefaultRateLimitPerHour : int.MaxValue;
		}
	}
}
