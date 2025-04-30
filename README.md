NetTools.Serialization.Json is a minimal and feature-rich .NET Standard 2.1 class library for JSON serialization & deserialization.

Initially a fork of "Really simple JSON parser in ~300 lines", and excluding the bloatware in libraries such as NewtonSoft's JSON.Net, NetTools.Serialization.Json has evoled into a robust, lightweight, and simple to use helper class.

Extension and adding support for object types (where required) is heavily based on factory & injection design patterns.

This library works around deserializing to interface types, where the compiler can't usually create an instance of this type using reflection.

This can be done using the KnownObject attribute at either property or class level, or by adding an IStingSerializer into NetTools.Serialization.Json's SerializationFactory.
