## C# Access Control Keywords Reference

This section documents C# access control and modifier keywords with explanations, use cases, and examples. See `consoleapp/AccessControlKeywordsTest.cs` for runnable demonstrations.

### 1. Access Modifiers

| Modifier             | Accessibility                                                |
| -------------------- | ------------------------------------------------------------ |
| `public`             | No restrictions - accessible from anywhere                   |
| `private`            | Only within the containing class/struct                      |
| `protected`          | Within the class and derived classes                         |
| `internal`           | Within the same assembly (project)                           |
| `protected internal` | Same assembly OR derived classes (union)                     |
| `private protected`  | Derived classes within the same assembly only (intersection) |

**When to use:**

- `public`: APIs, DTOs, interfaces meant for external consumption
- `private`: Implementation details, helper methods, internal state
- `protected`: Base class members that derived classes need to access/override
- `internal`: Classes/methods only used within your project, not exposed to consumers
- `protected internal`: Rare - when you need both assembly access and inheritance access
- `private protected`: Framework design - derived classes in same assembly need access

### 2. `sealed` Keyword

**Purpose:** Prevents a class from being inherited or prevents further override of a method.

**Why use sealed:**

1. **Security**: Prevents malicious code from inheriting and overriding behavior
2. **Performance**: JIT compiler can optimize calls to sealed classes/methods
3. **Design Intent**: Communicates "this class is complete, don't extend it"

```csharp
public sealed class FinalClass { } // Cannot be inherited

public class Derived : Base
{
    public sealed override void Method() { } // Cannot be overridden further
}
```

**When to use:**

- Utility classes that shouldn't be extended
- Security-critical classes
- When inheritance would break invariants
- Performance-critical code paths

### 3. `static` Keyword

**Purpose:** Declares members that belong to the type itself rather than instances.

**Static Class:**

- Cannot be instantiated
- All members must be static
- Implicitly sealed

**Static Members:**

- Shared across all instances
- Accessed via type name, not instance
- Static constructor runs once when type is first accessed

```csharp
public static class MathHelper
{
    public static readonly DateTime Created = DateTime.Now;
    public static int Add(int a, int b) => a + b;
}
```

**When to use:**

- Utility/helper methods (e.g., `Math.Sqrt()`)
- Extension methods (must be in static class)
- Factory methods
- Shared state (use with caution - thread safety!)
- Constants and configuration

### 4. `readonly` vs `const`

| Feature              | `const`                  | `readonly`                           |
| -------------------- | ------------------------ | ------------------------------------ |
| When assigned        | Compile time             | Runtime (declaration or constructor) |
| Implicitly static    | Yes                      | No                                   |
| Types allowed        | Primitives, string, null | Any type                             |
| Embedded in assembly | Yes                      | No                                   |

```csharp
public const double PI = 3.14159;              // Compile-time constant
public readonly DateTime Created;               // Set in constructor
public static readonly DateTime AppStart = DateTime.Now; // Runtime constant
```

**Important:** `readonly` on reference types makes the **reference** immutable, not the object!

```csharp
public readonly List<int> Items = new();
Items.Add(1);  // ✓ Allowed - modifying the list
Items = new(); // ✗ Error - cannot reassign reference
```

**When to use:**

- `const`: Mathematical constants, compile-time known values
- `readonly`: Values known at runtime but shouldn't change after construction
- `static readonly`: Singleton instances, runtime-computed constants

### 5. `abstract` Keyword

**Purpose:** Defines incomplete members that derived classes must implement.

```csharp
public abstract class Shape
{
    public abstract double Area { get; }      // Must be implemented
    public abstract double CalculateArea();   // Must be implemented
    public void Describe() { }                // Concrete - shared implementation
}
```

**When to use:**

- Defining contracts with partial implementation
- Template Method pattern
- When you want to force derived classes to provide specific behavior
- Creating type hierarchies with common behavior

### 6. `virtual` and `override` Keywords

**Purpose:** Enable runtime polymorphism.

```csharp
public class Animal
{
    public virtual void Speak() => Console.WriteLine("...");
}

public class Dog : Animal
{
    public override void Speak() => Console.WriteLine("Woof!");
}

Animal animal = new Dog();
animal.Speak(); // Output: "Woof!" - polymorphic call
```

**Key Points:**

- `virtual` allows but doesn't require override
- `override` replaces the base implementation
- `base.Method()` can call the original
- Without `virtual`, method calls are resolved at compile time

### 7. `new` Keyword (Member Hiding)

**Purpose:** Explicitly hides a base class member (not polymorphic!).

```csharp
public class Base { public void Method() => Console.WriteLine("Base"); }
public class Derived : Base
{
    public new void Method() => Console.WriteLine("Derived");
}

Base b = new Derived();
b.Method();  // Output: "Base" - NOT polymorphic!
```

**When to use:**

- Rarely! Usually indicates a design problem
- When inheriting from a class you don't control
- Explicitly documenting intentional hiding

### 8. `partial` Keyword

**Purpose:** Split a type definition across multiple files.

```csharp
// File1.cs
public partial class MyClass { public void Part1Method() { } }

// File2.cs
public partial class MyClass { public void Part2Method() { } }
```

**When to use:**

- Auto-generated code (WinForms, EF, gRPC)
- Large classes in team environments
- Separating concerns within a single class

### 9. `record` Keyword (C# 9+)

**Purpose:** Reference type with value-based equality and immutability.

```csharp
public record Person(string Name, int Age);

var p1 = new Person("John", 30);
var p2 = new Person("John", 30);
p1 == p2  // True! Value equality

var p3 = p1 with { Age = 31 };  // Immutable copy with changes
```

**When to use:**

- DTOs and data transfer objects
- Immutable data models
- When you need value semantics for reference types
- Domain-driven design value objects

### 10. `volatile` Keyword

**Purpose:** Indicates a field may be modified by multiple threads.

```csharp
public volatile bool IsRunning = true;
```

**When to use:**

- Simple flags accessed by multiple threads
- When you need memory visibility guarantees
- Note: For complex synchronization, prefer `lock`, `Interlocked`, or `Concurrent*` collections
