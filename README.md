# InternalReflector

A .NET library that wraps Reflection to provide access to internal or private fields, properties, and methods in classes.

## Features

- Access private and internal members of any class
- Support for static and instance members
- Type-safe generic methods
- Compatible with .NET Framework 2.0+ and .NET 5+ up to .NET 10
- Simple and intuitive API

## Installation

```bash
dotnet add package InternalReflector
```

## Usage

### Calling Private/Internal Methods

```csharp
// Static method
var result = InternalReflector<SomeClass>.Call("SomePrivateMethod", "some-string-parameter", 1234);

// Instance method
var instance = new SomeClass();
var result = InternalReflector<SomeClass>.Call(instance, "SomePrivateMethod", "some-string-parameter", 1234);
```

### Accessing Private/Internal Fields

```csharp
// Static field
var value = InternalReflector<SomeClass>.GetField<int>("SomePrivateField");

// Instance field
var instance = new SomeClass();
var value = InternalReflector<SomeClass>.GetField<int>(instance, "SomePrivateField");

// Setting field values
InternalReflector<SomeClass>.SetField("SomePrivateField", 42);
InternalReflector<SomeClass>.SetField(instance, "SomePrivateField", 42);
```

### Accessing Private/Internal Properties

```csharp
// Static property
var value = InternalReflector<SomeClass>.GetProperty<float>("SomeInternalProperty");

// Instance property
var instance = new SomeClass();
var value = InternalReflector<SomeClass>.GetProperty<float>(instance, "SomeInternalProperty");

// Setting property values
InternalReflector<SomeClass>.SetProperty("SomeInternalProperty", 3.14f);
InternalReflector<SomeClass>.SetProperty(instance, "SomeInternalProperty", 3.14f);
```

## Example

```csharp
public class StaticExampleClass
{
    private static int _privateStaticField = 10;
    internal static string InternalStaticProperty { get; set; } = "Hello (static)";

    private static int PrivateStaticMethod(int value)
    {
        return _privateStaticField + value;
    }
}

public class InstanceExampleClass
{
    private int _privateField = 10;
    internal string InternalProperty { get; set; } = "Hello (instance)";

    private int PrivateMethod(int value)
    {
        return _privateField + value;
    }
}

// Static member usage
var staticResult = InternalReflector<StaticExampleClass>.Call("PrivateStaticMethod", 5); // Returns 15
var staticFieldValue = InternalReflector<StaticExampleClass>.GetField<int>("_privateStaticField"); // Returns 10
var staticPropertyValue = InternalReflector<StaticExampleClass>.GetProperty<string>("InternalStaticProperty"); // Returns "Hello (static)"

// Instance member usage
var instance = new InstanceExampleClass();
var instanceResult = InternalReflector<InstanceExampleClass>.Call(instance, "PrivateMethod", 5); // Returns 15
var instanceFieldValue = InternalReflector<InstanceExampleClass>.GetField<int>(instance, "_privateField"); // Returns 10
var instancePropertyValue = InternalReflector<InstanceExampleClass>.GetProperty<string>(instance, "InternalProperty"); // Returns "Hello (instance)"
```

## Supported Frameworks

- .NET Framework 2.0, 3.5, 4.0, 4.5, 4.5.1, 4.5.2, 4.6, 4.6.1, 4.6.2, 4.7, 4.7.1, 4.7.2, 4.8, 4.8.1
- .NET 5.0, 6.0, 7.0, 8.0, 9.0, 10.0

## Publish to NuGet (GitHub Actions)

1. In GitHub repository settings, add a secret named `NUGET_API_KEY` with your NuGet.org API key.
2. Ensure package metadata in `InternalReflector/InternalReflector.csproj` (for example `Authors` and `RepositoryUrl`) is set to your real values.
3. Create and push a version tag (for example `v1.0.1`):

```bash
git tag v1.0.1
git push origin v1.0.1
```

The workflow at `.github/workflows/publish-nuget.yml` will restore, build, test, pack, and publish to NuGet.org.

## License

MIT License
