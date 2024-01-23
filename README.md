# .NET Core API Code Generator

This reposistory contains the [T4 Text Templates](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates) that I use to generate boilerplate C# source code for new .NET Core APIs.


## T4 Text Templates

Classes are generated for these layers:

* Contract
* Domain
* Persistence
* API
* SDK


## Get Started

First, update the configuration for your local environment. You'll find these settings in `Settings.json`:

1. **CommonName**: This is the name of the library used for common, cross-cutting concerns.
1. **DatabaseName**: This is the name of your SQL Server database.
2. **EndpointTable**: This is the name of the database table in which your API endpoints are defined.
3. **OutputDirectory**: This is the directory on your local file system where the generated classes are written.
4. **PlatformName**: This is the name of your application.