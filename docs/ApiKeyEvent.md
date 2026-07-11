# ApiKeyEvent
The `ApiKeyEvent` type represents a record of an event related to an API key, such as creation, rotation, or disabling. It provides a snapshot of the key's state at the time of the event, including the event's timestamp, the key's identifier, and the user who triggered the event. This type is used to track changes to API keys and provide an audit trail for security and compliance purposes.

## API
* `EventId`: A unique identifier for the event, represented as a `Guid`.
* `Timestamp`: The date and time when the event occurred, represented as a `DateTime`.
* `ApiKeyId`: The identifier of the API key associated with the event, represented as a `string`.
* `Name`: The name of the API key, represented as a `string`.
* `CreatedBy`: The user who created the API key, represented as a `string`.
* `RotatedBy`: The user who rotated the API key, represented as a `string`.
* `DisabledBy`: The user who disabled the API key, represented as a `string`.
* `Reason`: The reason for the event, represented as a `string`.
* `ChangedFields`: A dictionary of fields that were changed during the event, represented as a `Dictionary<string, object>`.
* `UpdatedBy`: The user who updated the API key, represented as a `string`.
* `OldLimit`: The previous limit of the API key, represented as an `int`.
* `NewLimit`: The new limit of the API key, represented as an `int`.
* `ChangedBy`: The user who changed the API key, represented as a `string`.

## Usage
The following examples demonstrate how to use the `ApiKeyEvent` type:
```csharp
// Create a new ApiKeyEvent instance
ApiKeyEvent event1 = new ApiKeyEvent
{
    EventId = Guid.NewGuid(),
    Timestamp = DateTime.UtcNow,
    ApiKeyId = "12345",
    Name = "My API Key",
    CreatedBy = "John Doe"
};

// Update an existing ApiKeyEvent instance
ApiKeyEvent event2 = new ApiKeyEvent
{
    EventId = Guid.NewGuid(),
    Timestamp = DateTime.UtcNow,
    ApiKeyId = "12345",
    Name = "My API Key",
    UpdatedBy = "Jane Doe",
    OldLimit = 100,
    NewLimit = 200
};
```

## Notes
When working with `ApiKeyEvent` instances, consider the following edge cases:
* The `ChangedFields` dictionary may contain null values if the corresponding fields were not changed during the event.
* The `UpdatedBy` field may be null if the event was not triggered by a user update.
* The `OldLimit` and `NewLimit` fields may be equal if the limit was not changed during the event.
* The `ApiKeyEvent` type is not thread-safe, as it contains mutable fields. When accessing or modifying `ApiKeyEvent` instances from multiple threads, use proper synchronization mechanisms to ensure data integrity.
