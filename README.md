# NetTools.Serialization.Json

NetTools.Serialization.Json is a lightweight yet powerful .NET Standard 2.1 class library designed for JSON serialization and deserialization. It offers an intuitive API for working with JSON in your .NET applications without the overhead of larger libraries like Newtonsoft's JSON.Net.

This library evolved from the "Really simple JSON parser in ~300 lines" and has grown into a robust solution for developers seeking simplicity, speed, and extensibility. NetTools.Serialization.Json is based on factory and injection design patterns, enabling seamless support for various object types, including interface types, which are traditionally challenging to deserialize via reflection.

## Features

- **Minimal Design:** Focused and efficient, with no unnecessary complexity or bloated dependencies.
- **Powerful Deserialization:** Supports deserialization to interface types using the `KnownObject` attribute or custom serialization factories.
- **Flexible Configuration:** Options for including private fields, handling broken JSON, and customizing serialization.
- **Convenient Extensions:** Extension methods for serializing and deserializing JSON with dictionaries, lists, and dynamic objects.
- **Prettify and Minify JSON:** Easily format JSON for readability or compress it for efficiency.
- **Easy Integration:** Built with simplicity in mind, allowing quick adoption into your project.

## Getting Started

### Installation

To use NetTools.Serialization.Json in your project, simply add the library via NuGet Package Manager:
```bash
Install-Package NetTools.Serialization.Json
````

### Quick Examples

Here's a basic demonstration of how to use NetTools.Serialization.Json for serialization and deserialization:
Serialize an Object to JSON
```csharp
using NetTools.Serialization.Json;

var person = new 
{
    Name = "John",
    Age = 30,
    IsDeveloper = true
};

string jsonString = person.ToJson();
Console.WriteLine(jsonString);
```

Deserialize JSON to an Object
```csharp
using NetTools.Serialization.Json;

string jsonString = "{\"Name\":\"John\",\"Age\":30,\"IsDeveloper\":true}";

var deserializedPerson = jsonString.FromJson<dynamic>();
Console.WriteLine($"Name: {deserializedPerson.Name}, Age: {deserializedPerson.Age}, Developer: {deserializedPerson.IsDeveloper}");
```

### Additional Examples
Beautify JSON
```csharp
string uglyJson = "{\"Name\":\"John\",\"Age\":30,\"IsDeveloper\":true}";
string prettyJson = uglyJson.BeautifyJson();
Console.WriteLine(prettyJson);
```

Minify JSON
```csharp
string formattedJson = "{\n  \"Name\": \"John\",\n  \"Age\": 30,\n  \"IsDeveloper\": true\n}";
string minifiedJson = formattedJson.MinifyJson();
Console.WriteLine(minifiedJson);
```

### Advanced Usage: Deserializing to Interfaces
NetTools.Serialization.Json simplifies the deserialization of interface types. Use the KnownObject attribute or configure a custom IStringSerializer via SerializationFactory if you don't have access to the source to add attributes to enable instance creation.
```csharp
using NetTools.Serialization.Json;

[KnownObject(typeof(Person))]
public interface IPerson
{
    string Name { get; set; }
    int Age { get; set; }
}

public class Person : IPerson
{
    public string Name { get; set; }
    public int Age { get; set; }
}

string jsonString = "{\"Name\":\"John\",\"Age\":30}";
var person = jsonString.FromJson<IPerson>();
Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
```

## Contributing

We welcome contributions! To get involved:
1. Fork [NetTools.Serialization.Json](https://github.com/netmodules/NetTools.Serialization.Json), make improvements, and submit a pull request.
2. Code will be reviewed upon submission.
3. Join discussions via the [issues board](https://github.com/netmodules/NetTools.Serialization.Json/issues).

## License

NetTools.Serialization.Json is licensed under the [MIT License](https://tldrlegal.com/license/mit-license), allowing unrestricted use, modification, and distribution. If you use NetTools.Serialization.Json in your own project, we’d love to hear about your experience, and possibly feature you on our website!

Full documentation coming soon!

[NetModules Foundation](https://netmodules.net/)
