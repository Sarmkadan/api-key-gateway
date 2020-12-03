# CollectionExtensions
Provides a set of extension methods for working with `IEnumerable<T>` sequences, offering common operations such as pagination, batching, distinctness checks, and safe iteration without altering the original source.

## API
### Paginate<T>
- **Purpose:** Returns an enumerable that yields the source sequence in pages of a specified size.  
- **Parameters:** The method is invoked on an `IEnumerable<T>` instance (the source sequence). Additional parameters are defined by the implementation (e.g., page size).  
- **Return Value:** `IEnumerable<T>` representing the paginated view of the source.  
- **Exceptions:**  
  - `ArgumentNullException` if the source sequence is `null`.  
  - Implementation‑specific exceptions (e.g., `ArgumentOutOfRangeException`) for invalid page‑size arguments.

### IsEmpty<T>
- **Purpose:** Determines whether the source sequence contains no elements.  
- **Parameters:** The method is invoked on an `IEnumerable<T>` instance (the source sequence).  
- **Return Value:** `true` if the source is empty; otherwise `false`.  
- **Exceptions:** `ArgumentNullException` if the source sequence is `null`.

### HasItems<T>
- **Purpose:** Determines whether the source sequence contains at least one element.  
- **Parameters:** The method is invoked on an `IEnumerable<T>` instance (the source sequence).  
- **Return Value:** `true` if the source contains one or more elements; otherwise `false`.  
- **Exceptions:** `ArgumentNullException` if the source sequence is `null`.

### CountBy<T, TKey>
- **Purpose:** Groups elements of the source sequence by a key selector and returns a dictionary mapping each key to the number of elements with that key.  
- **Parameters:** The method is invoked on an `IEnumerable<T>` instance (the source sequence). Additional parameters are defined by the implementation (e.g., key selector).  
- **Return Value:** `Dictionary<TKey, int>` where each key is a distinct key from the source and the associated value is the count of elements producing that key.  
- **Exceptions:**  
  - `ArgumentNullException` if the source sequence or the key selector is `null`.  
  - Implementation‑specific exceptions for invalid selector logic.

### Batch<T>
- **Purpose:** Splits the source sequence into consecutive batches, each containing up to a specified number of elements.  
- **Parameters:** The method is invoked on an `IEnumerable<T>` instance (the source sequence). Additional parameters are defined by the implementation (e.g., batch size).  
- **Return Value:** `IEnumerable<IEnumerable<T>>` where each inner enumerable represents a batch of the source.  
- **Exceptions:**  
  - `ArgumentNullException` if the source sequence is `null`.  
  - Implementation‑specific exceptions (e.g., `ArgumentOutOfRangeException`) for invalid batch‑size arguments.

### DistinctBy<T, TKey>
- **Purpose:** Returns distinct elements from the source sequence according to a key selector, preserving the original order of first occurrence.  
- **Parameters:** The method is invoked on an `IEnumerable<T>` instance (the source sequence). Additional parameters are defined by the implementation (e.g., key selector).  
- **Return Value:** `IEnumerable<T>` containing the distinct elements.  
- **Exceptions:**  
  - `ArgumentNullException` if the source sequence or the key selector is `null`.  
  - Implementation‑specific exceptions for invalid selector logic.

### ForEachSafe<T>
- **Purpose:** Executes an action for each element of the source sequence, safely handling null source or action without throwing.  
- **Parameters:** The method is invoked on an `IEnumerable<T>` instance (the source sequence). Additional parameters are defined by the implementation (e.g., the action to perform).  
- **Return Value:** `void`.  
- **Exceptions:**  
  - `ArgumentNullException` if the source sequence is `null`.  
  - `ArgumentNullException` if the supplied action is `null`.

## Usage
```csharp
using System.Collections.Generic;
using System.Linq;

// Example 1: Paginate and batch a list of numbers.
var numbers = Enumerable.Range(1, 25);
var pages = numbers.Paginate(5);          // Sequences of 5 elements each.
foreach (var page in pages)
{
    var batch = page.Batch(2);            // Further split each page into pairs.
    Console.WriteLine(string.Join(", ", batch));
}

// Example 2: Count occurrences by a property and obtain distinct items.
var orders = new List<Order>
{
    new Order { Id = 1, Customer = "Alice" },
    new Order { Id = 2, Customer = "Bob" },
    new Order { Id = 3, Customer = "Alice" },
    new Order { Id = 4, Customer = "Charlie" }
};

var orderCountsByCustomer = orders.CountBy(o => o.Customer);
// orderCountsByCustomer => { ["Alice", 2], ["Bob", 1], ["Charlie", 1] }

var distinctCustomers = orders.DistinctBy(o => o.Customer);
// distinctCustomers yields orders for Alice, Bob, and Charlie in original order.
```

```csharp
using System;

// Example 3: Safe iteration and emptiness checks.
IEnumerable<string> maybeNull = null;
bool isEmpty = maybeNull.IsEmpty();          // Returns false and throws ArgumentNullException.
bool hasItems = maybeNull.HasItems();        // Returns false and throws ArgumentNullException.

IEnumerable<string> items = new[] { "a", "b", "c" };
items.ForEachSafe(s => Console.WriteLine(s.ToUpper()));
// Prints: A, B, C

items.ForEachSafe(null);                    // Throws ArgumentNullException for null action.
```

## Notes
- All extension methods treat a `null` source sequence as an error and throw `ArgumentNullException`.  
- The methods do not modify the source sequence; they rely on read‑only enumeration. Consequently, they are safe to use concurrently with other read‑only operations, but enumerating while the source is being modified by another thread yields undefined behavior.  
- For `Paginate<T>`, `Batch<T>`, and `CountBy<T,TKey>` the size‑related arguments (page size, batch size, etc.) must be positive; non‑positive values trigger implementation‑specific exceptions.  
- `DistinctBy<T,TKey>` preserves the order of the first occurrence of each distinct key; later duplicates are omitted.  
- `ForEachSafe<T>` will not execute the action if the source sequence is empty, but it will still validate that neither the source nor the action is `null`.  
- Returned enumerables are lazy‑evaluated where applicable; side effects in selectors or actions occur only during enumeration.
