# .NET Core API Code Generator

This reposistory contains the [T4 Text Templates](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates) that I use to generate boilerplate C# source code when I need to build a new .NET API for access to an existing SQL Server database.

Based on API endpoint specifications defined in a database table (which you need to create and load in advance), this code generator creates classes for each of the following layers:

- **Contract** classes define the request and response objects to read, write, and delete entities in your database.
- **Domain** classes define the entities used to read and write data using Entity Framework Core.
- **Persistence** classes define the search and store functionality for your data access layer.
- **API** classes define the controllers to respond to read, write, and delete requests.
- **SDK** classes define the clients you can use in all your .NET applications to work with your new API.

You'll notice the Contract and SDK classes are generated for .NET Standard 2.0 libraries so they can be used in any .NET Core and/or .NET Framework application. Of course, the SDK classes are optional, because your API can be invoked from any application using any programing language that supports REST API calls.

## How to Use the Generator

First, update the configuration for your local environment. You'll find these configuration settings in `Settings.json`:

- **CommonName** is the name of the library used for common, cross-cutting concerns. Feel free to use your own library here, or you can my [Common](https://github.com/Daniel-Miller/Common) library.
- **DatabaseName** is the name of the SQL Server database in which your API endpoint specification table is stored.
- **EndpointTable** is the name of the database table in which your API endpoint specifications are defined.
- **OutputDirectory** is the directory on your local file system where the generated source code files are written.
- **PlatformName** is the name of your application.

Next, create a table in your database in which to define your API endpoints. You can assign any name you want to this table, but it must contain these columns:

```
create table databases.TApiEndpoint
(
    DatabaseSchema varchar(100) not null
  , DatabaseTable varchar(100) not null
  , DatabasePrimaryKey varchar(100) not null
  , DatabasePrimaryKeySize int not null
  , DomainToolkit varchar(100) not null
  , DomainToolset varchar(100) not null
  , DomainEntity varchar(100) not null
  , EndpointIdentifier uniqueidentifier not null primary key
  , EndpointBase varchar(100) not null
  , EndpointCollection varchar(100) not null
  , EndpointItem varchar(100) not null,
)
```

Next, add a record to this table for each each database table (and each database view) that you want to access through your API. I like to build my specifications in a Microsoft Excel spreadsheet, and use an Excel formula to produce the SQL INSERT statements needed to load my specifications into this table. Refer to the "API Endpoint Specification Example.xlsx" for an example.

Finally, select the template named `Generate.tt` and click "Run Custom Tool". Your new code is output to the directory specified by your settings.

## Tips and Tricks

### Missing Specifications

I use this query to identify database tables for which there are no specifications in my ApiEndpoint table. These represent "missing" endpoints, if you want your API to provide a full-coverage data access layer to your database.

```
select * from INFORMATION_SCHEMA.TABLES as t where t.TABLE_TYPE = 'Base Table' and not exists 
(select * from databases.TApiEndpoint as e where TABLE_SCHEMA = e.DatabaseSchema and t.TABLE_NAME = e.DatabaseTable)
```

### Dead-Ends

I use this query to identify API endpoint specifications for which there is no corresponding database table. These represent "broken" endpoints.

```
select * from databases.TApiEndpoint as e where not exists 
(select * from INFORMATION_SCHEMA.TABLES as t where TABLE_SCHEMA = e.DatabaseSchema and t.TABLE_NAME = e.DatabaseTable)
```
