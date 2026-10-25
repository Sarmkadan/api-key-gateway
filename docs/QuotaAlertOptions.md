# QuotaAlertOptions

`QuotaAlertOptions` holds the settings for proactive quota threshold alerting. It is defined in `src/ApiKeyGateway/Configuration/QuotaAlertOptions.cs` and bound from the `QuotaAlerts` configuration section (`QuotaAlertOptions.SectionName`). The options are registered in `Configuration/ServiceCollectionExtensions.cs` with `services.Configure<QuotaAlertOptions>(...)` and consumed as `IOptions<QuotaAlertOptions>` by `QuotaAlertEvaluator`.

See [QuotaAlertEvaluator](QuotaAlertEvaluator.md) for how the thresholds are evaluated and [QuotaThresholdReachedEvent](QuotaThresholdReachedEvent.md) for the event that is published.

---

## Configuration

```json
{
  "QuotaAlerts": {
    "Enabled": true,
    "ThresholdPercentages": [80, 100]
  }
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | `bool` | `true` | Turns quota threshold evaluation on or off. When `false`, `QuotaAlertEvaluator` returns without checking any thresholds. |
| `ThresholdPercentages` | `List<int>` | `[80, 100]` | Percentages of the quota limit at which a `QuotaThresholdReachedEvent` is published. |

---

## Members

### `SectionName`

```csharp
public const string SectionName = "QuotaAlerts";
```

The configuration section the options are bound from. Use this constant instead of the string literal when reading the section in code.

### `Enabled`

Whether quota threshold alerting is enabled. Defaults to `true`.

### `ThresholdPercentages`

Percentage thresholds of the quota limit at which an alert is raised. Defaults to `80` and `100`. The property is a plain `List<int>`, so it can be set in code or bound from configuration. The list is not validated on assignment. Filtering happens in `GetEffectiveThresholds()`.

### `GetEffectiveThresholds()`

```csharp
public IReadOnlyList<int> GetEffectiveThresholds()
```

Returns the thresholds that the evaluator actually uses. The method:

1. Keeps only values in the range `(0, 100]`. Zero, negative values and values above `100` are dropped.
2. Removes duplicates.
3. Sorts the remaining values in ascending order.

It returns an empty list if no valid values remain. It also handles a `null` `ThresholdPercentages` list. In that case it returns an empty list instead of throwing.

---

## Examples

Use the defaults (80% and 100%):

```json
"QuotaAlerts": {
  "Enabled": true,
  "ThresholdPercentages": [80, 100]
}
```

Alert at 50%, 80%, 90% and 100%, and turn alerting off in a non-production environment:

```json
"QuotaAlerts": {
  "Enabled": false,
  "ThresholdPercentages": [50, 80, 90, 100]
}
```

Invalid and duplicate values are cleaned up before use. With the following configuration, `GetEffectiveThresholds()` returns `[25, 50, 90]`:

```json
"QuotaAlerts": {
  "ThresholdPercentages": [90, 0, 50, 150, 50, -5, 25]
}
```

---

## Notes

* **Lists are merged with the defaults, not replaced.** The default `ThresholdPercentages` value is `[80, 100]`, and the configuration binder adds values from configuration to the existing list. For example, setting `ThresholdPercentages` to `[50, 90]` in configuration produces `[80, 100, 50, 90]`, which `GetEffectiveThresholds()` turns into `[50, 80, 90, 100]`. If you need only your own thresholds, you cannot remove `80` and `100` through configuration alone. Replace the list in code instead, for example with a `PostConfigure<QuotaAlertOptions>` callback that assigns `ThresholdPercentages` after binding.
* **Invalid values are ignored, not reported.** Values outside `(0, 100]` do not produce an error or a log entry. They are dropped silently.
* **Changes need a restart.** `QuotaAlertEvaluator` takes `IOptions<QuotaAlertOptions>`, which is a snapshot taken at startup. Editing `QuotaAlerts` in `appsettings.json` has no effect until the process restarts. The evaluator calls `GetEffectiveThresholds()` on each evaluation, so the cleanup described above applies every time.
