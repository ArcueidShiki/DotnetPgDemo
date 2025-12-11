using System;

namespace consoleapp;

#region Access Modifiers: public, private, protected, internal, protected internal, private protected

/// <summary>
/// PUBLIC: Accessible from anywhere - no restrictions
/// </summary>
public class PublicClass
{
    public string PublicField = "I'm public - accessible everywhere";

    public void PublicMethod() => Console.WriteLine("PublicMethod called");
}

/// <summary>
/// INTERNAL: Accessible only within the same assembly (project)
/// </summary>
internal class InternalClass
{
    internal string InternalField = "I'm internal - only accessible within this assembly";

    internal void InternalMethod() => Console.WriteLine("InternalMethod called - same assembly only");
}

/// <summary>
/// Demonstrates all access modifiers within a class
/// </summary>
public class AccessModifiersDemo
{
    public string PublicField = "Public: Anyone can access";
    private string _privateField = "Private: Only this class can access";
    protected string ProtectedField = "Protected: This class and derived classes";
    internal string InternalField = "Internal: Same assembly only";
    protected internal string ProtectedInternalField = "Protected Internal: Same assembly OR derived classes";
    private protected string PrivateProtectedField = "Private Protected: Derived classes in same assembly only";

    public void ShowAllFields()
    {
        Console.WriteLine("=== Access Modifiers Demo ===");
        Console.WriteLine($"  public: {PublicField}");
        Console.WriteLine($"  private: {_privateField}");
        Console.WriteLine($"  protected: {ProtectedField}");
        Console.WriteLine($"  internal: {InternalField}");
        Console.WriteLine($"  protected internal: {ProtectedInternalField}");
        Console.WriteLine($"  private protected: {PrivateProtectedField}");
    }
}

/// <summary>
/// Derived class to show protected access
/// </summary>
public class DerivedAccessDemo : AccessModifiersDemo
{
    public void ShowProtectedAccess()
    {
        Console.WriteLine("=== Derived Class Access ===");
        Console.WriteLine($"  Can access public: {PublicField}");
        // Console.WriteLine(_privateField); // ERROR: Cannot access private
        Console.WriteLine($"  Can access protected: {ProtectedField}");
        Console.WriteLine($"  Can access internal: {InternalField}");
        Console.WriteLine($"  Can access protected internal: {ProtectedInternalField}");
        Console.WriteLine($"  Can access private protected: {PrivateProtectedField}");
    }
}

#endregion

#region SEALED Keyword - Prevents inheritance

/// <summary>
/// SEALED: Prevents a class from being inherited
/// Use when: You want to prevent further derivation for security, performance, or design reasons
/// </summary>
public sealed class SealedClass
{
    public string Data { get; set; } = "I cannot be inherited!";

    public void Display() => Console.WriteLine($"SealedClass.Display(): {Data}");
}

// ERROR: Cannot inherit from sealed class
// public class TryInheritSealed : SealedClass { }

/// <summary>
/// Base class for demonstrating sealed methods
/// </summary>
public class BaseWithVirtual
{
    public virtual void VirtualMethod() => Console.WriteLine("BaseWithVirtual.VirtualMethod()");
}

/// <summary>
/// Derived class that seals an overridden method
/// </summary>
public class DerivedWithSealedMethod : BaseWithVirtual
{
    // Sealed override - further derived classes cannot override this
    public sealed override void VirtualMethod() => Console.WriteLine("DerivedWithSealedMethod.VirtualMethod() - SEALED");
}

/// <summary>
/// Further derived class - cannot override sealed method
/// </summary>
public class FurtherDerived : DerivedWithSealedMethod
{
    // ERROR: Cannot override sealed method
    // public override void VirtualMethod() { }

    public void ShowSealedBehavior()
    {
        Console.WriteLine("FurtherDerived calling VirtualMethod:");
        VirtualMethod(); // Calls the sealed version from DerivedWithSealedMethod
    }
}

#endregion

#region STATIC Keyword - Class level members, no instance needed

/// <summary>
/// STATIC CLASS: Cannot be instantiated, all members must be static
/// Use for: Utility classes, extension methods, constants
/// </summary>
public static class StaticUtilityClass
{
    // Static field - shared across all code
    public static int CallCount = 0;

    // Static readonly - can only be set at declaration or in static constructor
    public static readonly DateTime CreatedAt = DateTime.Now;

    // Static constructor - runs once when class is first accessed
    static StaticUtilityClass()
    {
        Console.WriteLine($"StaticUtilityClass static constructor called at {CreatedAt}");
    }

    public static void IncrementAndShow()
    {
        CallCount++;
        Console.WriteLine($"StaticUtilityClass.IncrementAndShow() called {CallCount} time(s)");
    }

    public static int Add(int a, int b) => a + b;
}

/// <summary>
/// Regular class with static members
/// </summary>
public class ClassWithStaticMembers
{
    // Instance field - each object has its own copy
    public string InstanceName { get; set; }

    // Static field - shared by all instances
    public static int InstanceCount = 0;

    public ClassWithStaticMembers(string name)
    {
        InstanceName = name;
        InstanceCount++;
    }

    // Instance method - requires an object
    public void ShowInstance() => Console.WriteLine($"Instance: {InstanceName}");

    // Static method - called on class, not instance
    public static void ShowTotalInstances() => Console.WriteLine($"Total instances created: {InstanceCount}");
}

#endregion

#region READONLY Keyword - Immutable after initialization

/// <summary>
/// READONLY: Field can only be assigned at declaration or in constructor
/// Difference from const: readonly is evaluated at runtime, const at compile time
/// </summary>
public class ReadonlyDemo
{
    // Readonly field - set at declaration
    public readonly string ReadonlyAtDeclaration = "Set at declaration";

    // Readonly field - set in constructor
    public readonly string ReadonlyInConstructor;

    // Readonly field - reference type (the reference is readonly, not the object)
    public readonly List<string> ReadonlyList = new List<string>();

    // CONST: Compile-time constant, must be primitive or string, implicitly static
    public const double PI = 3.14159265359;
    public const string CONST_STRING = "I'm a compile-time constant";

    // Static readonly - like const but evaluated at runtime
    public static readonly DateTime AppStartTime = DateTime.Now;

    public ReadonlyDemo(string constructorValue)
    {
        ReadonlyInConstructor = constructorValue;
        // ReadonlyAtDeclaration = "Cannot reassign!"; // ERROR
    }

    public void TryModify()
    {
        // Cannot reassign readonly field
        // ReadonlyInConstructor = "new value"; // ERROR

        // BUT can modify the object a readonly reference points to!
        ReadonlyList.Add("Item 1"); // This is allowed!
        ReadonlyList.Add("Item 2"); // This is allowed!
        Console.WriteLine($"ReadonlyList has {ReadonlyList.Count} items - reference is readonly, not the list!");
    }

    public void ShowValues()
    {
        Console.WriteLine("=== Readonly vs Const Demo ===");
        Console.WriteLine($"  readonly (declaration): {ReadonlyAtDeclaration}");
        Console.WriteLine($"  readonly (constructor): {ReadonlyInConstructor}");
        Console.WriteLine($"  const PI: {PI}");
        Console.WriteLine($"  const string: {CONST_STRING}");
        Console.WriteLine($"  static readonly AppStartTime: {AppStartTime}");
    }
}

#endregion

#region ABSTRACT Keyword - Must be implemented by derived classes

/// <summary>
/// ABSTRACT CLASS: Cannot be instantiated, serves as a base class
/// Use when: You want to define a common interface with some default implementation
/// </summary>
public abstract class AbstractShape
{
    // Abstract property - must be implemented
    public abstract string Name { get; }

    // Abstract method - must be implemented
    public abstract double CalculateArea();

    // Concrete method - shared implementation
    public void Describe()
    {
        Console.WriteLine($"I am a {Name} with area {CalculateArea():F2}");
    }
}

public class Circle : AbstractShape
{
    public double Radius { get; set; }

    public Circle(double radius) => Radius = radius;

    public override string Name => "Circle";

    public override double CalculateArea() => Math.PI * Radius * Radius;
}

public class Rectangle : AbstractShape
{
    public double Width { get; set; }
    public double Height { get; set; }

    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public override string Name => "Rectangle";

    public override double CalculateArea() => Width * Height;
}

#endregion

#region VIRTUAL and OVERRIDE Keywords - Polymorphism

/// <summary>
/// VIRTUAL: Method can be overridden in derived classes
/// OVERRIDE: Provides new implementation of virtual method
/// </summary>
public class VirtualBaseClass
{
    // Virtual method - can be overridden
    public virtual void Greet() => Console.WriteLine("Hello from VirtualBaseClass!");

    // Non-virtual method - cannot be overridden (can only be hidden with 'new')
    public void NonVirtualMethod() => Console.WriteLine("NonVirtualMethod from Base");
}

public class VirtualDerivedClass : VirtualBaseClass
{
    // Override the virtual method
    public override void Greet() => Console.WriteLine("Hello from VirtualDerivedClass!");

    // Hiding the non-virtual method (not recommended, use 'new' keyword to suppress warning)
    public new void NonVirtualMethod() => Console.WriteLine("NonVirtualMethod from Derived (hiding base)");
}

public class VirtualDerivedClass2 : VirtualBaseClass
{
    // Can choose not to override - base implementation is used

    // Can also call base implementation
    public override void Greet()
    {
        base.Greet(); // Call base class version
        Console.WriteLine("...and also from VirtualDerivedClass2!");
    }
}

#endregion

#region PARTIAL Keyword - Split class across multiple files

/// <summary>
/// PARTIAL: Allows splitting a class, struct, interface, or method across multiple files
/// Use when: Working with auto-generated code, large classes, or team development
/// </summary>
public partial class PartialClassDemo
{
    public string Part1Property { get; set; } = "From Part 1";

    public void Part1Method() => Console.WriteLine("PartialClassDemo.Part1Method()");
}

// This would typically be in another file
public partial class PartialClassDemo
{
    public string Part2Property { get; set; } = "From Part 2";

    public void Part2Method() => Console.WriteLine("PartialClassDemo.Part2Method()");

    // Partial method - declaration
    partial void OnSomethingHappened();
}

// Implementation of partial method (optional)
public partial class PartialClassDemo
{
    partial void OnSomethingHappened()
    {
        Console.WriteLine("Partial method OnSomethingHappened() was implemented!");
    }

    public void TriggerPartialMethod()
    {
        OnSomethingHappened();
    }
}

#endregion

#region NEW Keyword (for hiding) and VOLATILE, EXTERN keywords

/// <summary>
/// NEW (hiding): Explicitly hides a member from a base class
/// </summary>
public class BaseForHiding
{
    public void Method() => Console.WriteLine("BaseForHiding.Method()");
    public int Value = 10;
}

public class DerivedWithNew : BaseForHiding
{
    // 'new' keyword explicitly hides base member (suppresses compiler warning)
    public new void Method() => Console.WriteLine("DerivedWithNew.Method() - hiding base");
    public new int Value = 20;

    public void ShowBoth()
    {
        Console.WriteLine($"Derived Value: {Value}");
        Console.WriteLine($"Base Value: {base.Value}");
        Method();
        base.Method();
    }
}

/// <summary>
/// VOLATILE: Indicates a field might be modified by multiple threads
/// Used for: Thread synchronization, ensuring reads/writes aren't cached
/// </summary>
public class VolatileDemo
{
    // Volatile ensures the value is always read from memory, not from CPU cache
    public volatile bool IsRunning = true;

    public void Stop() => IsRunning = false;
}

#endregion

#region RECORD Keyword (C# 9+) - Immutable reference type with value semantics

/// <summary>
/// RECORD: Reference type with built-in value equality
/// Use when: You need immutable data objects with value-based equality
/// </summary>
public record Person(string FirstName, string LastName);

public record PersonWithMethods(string FirstName, string LastName)
{
    public string FullName => $"{FirstName} {LastName}";
}

// Record struct (C# 10+) - value type record
public record struct Point(int X, int Y);

#endregion

#region Test Runner Class

public static class AccessControlKeywordsTestRunner
{
    public static void RunAllTests()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║        C# Access Control & Keywords Test Suite                 ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        TestAccessModifiers();
        TestSealed();
        TestStatic();
        TestReadonly();
        TestAbstract();
        TestVirtualOverride();
        TestPartial();
        TestNewKeyword();
        TestRecords();
    }

    private static void TestAccessModifiers()
    {
        Console.WriteLine("\n┌─────────────────────────────────────────┐");
        Console.WriteLine("│ 1. ACCESS MODIFIERS TEST                │");
        Console.WriteLine("└─────────────────────────────────────────┘");

        var accessDemo = new AccessModifiersDemo();
        accessDemo.ShowAllFields();

        var derivedDemo = new DerivedAccessDemo();
        derivedDemo.ShowProtectedAccess();

        var internalClass = new InternalClass();
        Console.WriteLine($"\nInternal class access: {internalClass.InternalField}");
    }

    private static void TestSealed()
    {
        Console.WriteLine("\n┌─────────────────────────────────────────┐");
        Console.WriteLine("│ 2. SEALED KEYWORD TEST                  │");
        Console.WriteLine("└─────────────────────────────────────────┘");

        var sealed1 = new SealedClass { Data = "Custom data" };
        sealed1.Display();

        Console.WriteLine("\nSealed method inheritance chain:");
        var furtherDerived = new FurtherDerived();
        furtherDerived.ShowSealedBehavior();
    }

    private static void TestStatic()
    {
        Console.WriteLine("\n┌─────────────────────────────────────────┐");
        Console.WriteLine("│ 3. STATIC KEYWORD TEST                  │");
        Console.WriteLine("└─────────────────────────────────────────┘");

        Console.WriteLine($"\nStatic utility class:");
        StaticUtilityClass.IncrementAndShow();
        StaticUtilityClass.IncrementAndShow();
        StaticUtilityClass.IncrementAndShow();
        Console.WriteLine($"StaticUtilityClass.Add(5, 3) = {StaticUtilityClass.Add(5, 3)}");

        Console.WriteLine($"\nStatic vs Instance members:");
        var obj1 = new ClassWithStaticMembers("Object1");
        var obj2 = new ClassWithStaticMembers("Object2");
        var obj3 = new ClassWithStaticMembers("Object3");

        obj1.ShowInstance();
        obj2.ShowInstance();
        ClassWithStaticMembers.ShowTotalInstances();
    }

    private static void TestReadonly()
    {
        Console.WriteLine("\n┌─────────────────────────────────────────┐");
        Console.WriteLine("│ 4. READONLY vs CONST TEST               │");
        Console.WriteLine("└─────────────────────────────────────────┘");

        var readonlyDemo = new ReadonlyDemo("Set in constructor");
        readonlyDemo.ShowValues();
        readonlyDemo.TryModify();
    }

    private static void TestAbstract()
    {
        Console.WriteLine("\n┌─────────────────────────────────────────┐");
        Console.WriteLine("│ 5. ABSTRACT KEYWORD TEST                │");
        Console.WriteLine("└─────────────────────────────────────────┘");

        // Cannot instantiate abstract class
        // var shape = new AbstractShape(); // ERROR

        AbstractShape circle = new Circle(5);
        AbstractShape rectangle = new Rectangle(4, 6);

        circle.Describe();
        rectangle.Describe();

        // Polymorphism in action
        AbstractShape[] shapes = { circle, rectangle, new Circle(3) };
        Console.WriteLine("\nPolymorphic array iteration:");
        foreach (var shape in shapes)
        {
            Console.WriteLine($"  {shape.Name}: Area = {shape.CalculateArea():F2}");
        }
    }

    private static void TestVirtualOverride()
    {
        Console.WriteLine("\n┌─────────────────────────────────────────┐");
        Console.WriteLine("│ 6. VIRTUAL/OVERRIDE TEST                │");
        Console.WriteLine("└─────────────────────────────────────────┘");

        VirtualBaseClass baseObj = new VirtualBaseClass();
        VirtualBaseClass derivedAsBase = new VirtualDerivedClass();
        VirtualDerivedClass derivedObj = new VirtualDerivedClass();
        VirtualBaseClass derived2AsBase = new VirtualDerivedClass2();

        Console.WriteLine("Calling Greet() on different objects:");
        Console.Write("  baseObj.Greet(): "); baseObj.Greet();
        Console.Write("  derivedAsBase.Greet(): "); derivedAsBase.Greet(); // Polymorphic call
        Console.Write("  derivedObj.Greet(): "); derivedObj.Greet();
        Console.Write("  derived2AsBase.Greet(): "); derived2AsBase.Greet();

        Console.WriteLine("\nMethod hiding (non-virtual):");
        Console.Write("  baseObj.NonVirtualMethod(): "); baseObj.NonVirtualMethod();
        Console.Write("  derivedAsBase.NonVirtualMethod(): "); derivedAsBase.NonVirtualMethod(); // NOT polymorphic
        Console.Write("  derivedObj.NonVirtualMethod(): "); derivedObj.NonVirtualMethod();
    }

    private static void TestPartial()
    {
        Console.WriteLine("\n┌─────────────────────────────────────────┐");
        Console.WriteLine("│ 7. PARTIAL KEYWORD TEST                 │");
        Console.WriteLine("└─────────────────────────────────────────┘");

        var partial = new PartialClassDemo();
        Console.WriteLine($"Part1Property: {partial.Part1Property}");
        Console.WriteLine($"Part2Property: {partial.Part2Property}");
        partial.Part1Method();
        partial.Part2Method();
        partial.TriggerPartialMethod();
    }

    private static void TestNewKeyword()
    {
        Console.WriteLine("\n┌─────────────────────────────────────────┐");
        Console.WriteLine("│ 8. NEW (HIDING) KEYWORD TEST            │");
        Console.WriteLine("└─────────────────────────────────────────┘");

        var derived = new DerivedWithNew();
        derived.ShowBoth();

        Console.WriteLine("\nThrough base type reference:");
        BaseForHiding asBase = derived;
        Console.Write("  asBase.Method(): "); asBase.Method(); // Calls BASE method (no polymorphism)
        Console.WriteLine($"  asBase.Value: {asBase.Value}"); // Shows BASE value
    }

    private static void TestRecords()
    {
        Console.WriteLine("\n┌─────────────────────────────────────────┐");
        Console.WriteLine("│ 9. RECORD KEYWORD TEST (C# 9+)         │");
        Console.WriteLine("└─────────────────────────────────────────┘");

        var person1 = new Person("John", "Doe");
        var person2 = new Person("John", "Doe");
        var person3 = new Person("Jane", "Doe");

        Console.WriteLine($"person1: {person1}"); // Built-in ToString()
        Console.WriteLine($"person2: {person2}");
        Console.WriteLine($"person1 == person2: {person1 == person2}"); // Value equality!
        Console.WriteLine($"person1 == person3: {person1 == person3}");

        // With expression for immutable copy with changes
        var person4 = person1 with { FirstName = "Jack" };
        Console.WriteLine($"person4 (copied from person1): {person4}");

        var personWithMethods = new PersonWithMethods("Alice", "Smith");
        Console.WriteLine($"FullName: {personWithMethods.FullName}");

        // Record struct
        var point1 = new Point(10, 20);
        var point2 = new Point(10, 20);
        Console.WriteLine($"point1: {point1}, point1 == point2: {point1 == point2}");
    }
}

#endregion
