# .NET Core API Code Generator

This repository contains the [T4 Text Templates](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates) I use to generate boilerplate C# source code when I build a new .NET API for access to an existing SQL Server database.

Based on API endpoint specifications defined in a database table (which you need to create and load in advance), this code generator creates classes for each of the following layers:

- **Contract** classes implement the request and response objects to work with the API.
- **Service** classes implement the data access layer, domain models, and application logic.
- **API** classes implement the controllers to respond to read, write, and delete requests.

You'll notice the Contract classes are generated for .NET Standard 2.0 libraries so they can be used in any .NET Core and/or .NET Framework application. Using the Client (SDK) classes in the Contract library is completely optional; the generated API can be invoked from any application using any programing language that supports REST API calls.

## How to Use the Generator

First, update the configuration for your local environment. You'll find these configuration settings in `Settings.cs`:

- **DatabaseName** is the name of the SQL Server database in which your API endpoint specification table is stored.
- **OutputDirectory** is the directory on your local file system where the generated source code files are written.
- **PlatformName** is the name of your application.

Next, create a table in your database in which to define your API endpoints. The table name must be `databases.TApiEndpoint` and it must contain these columns:

```
create table databases.TApiEndpoint
(
    Component varchar(100) not null -- Table | Projection | View | Procedure
  , DatabaseSchema varchar(100) not null 
  , DatabaseObject varchar(100) not null
  , PrimaryKey varchar(100) not null
  , PrimaryKeySize int not null
  , DomainToolkit varchar(100) not null
  , DomainFeature varchar(100) not null
  , DomainEntity varchar(100) not null
  , EndpointBase varchar(100) not null
  , EndpointCollection varchar(100) not null
  , EndpointItem varchar(100) not null
  , EndpointIdentifier uniqueidentifier not null primary key
)
```

Next, add a record to this table for each each database table (and, optionally, each database view) you want to access through your API. Typically, I build my specifications in a Microsoft Excel spreadsheet, then I use an Excel formula to create a SQL INSERT statement to load my specifications into this table. Refer to the "API Endpoint Specification Example.xlsx" for an example.

Finally, select the template named `Generate.tt` and click "Run Custom Tool". Your generated code is output to the directory specified by your settings.

## Tips and Tricks

### Missing Specifications

I use this query to identify database objects for which there are no specifications in my ApiEndpoint table. These represent missing endpoints, if you want your API to provide a full-coverage data access layer to your database.

```
with cte as (
                select 'Table'        as Component
                     , t.TABLE_SCHEMA as DatabaseSchema
                     , t.TABLE_NAME   as DatabaseObject
                from INFORMATION_SCHEMA.TABLES as t
                where t.TABLE_TYPE = 'Base Table'
                union
                select 'View'
                     , t.TABLE_SCHEMA
                     , t.TABLE_NAME as DatabaseObject
                from INFORMATION_SCHEMA.TABLES as t
                where t.TABLE_TYPE <> 'Base Table'
                union
                select 'Procedure'
                     , ROUTINE_SCHEMA
                     , ROUTINE_NAME
                from INFORMATION_SCHEMA.ROUTINES
                where ROUTINE_TYPE = 'PROCEDURE'
            )
select cte.Component
     , cte.DatabaseSchema
     , cte.DatabaseObject
from cte
where DatabaseObject not in ( 'TApiEndpoint' )
      and not exists (
                         select *
                         from databases.TApiEndpoint as e
                         where cte.DatabaseSchema = e.DatabaseSchema
                               and cte.DatabaseObject = e.DatabaseObject
                     )
order by Component
       , DatabaseSchema
       , DatabaseObject
```

### Dead-Ends

I use this query to identify API endpoint specifications for which there is no corresponding database table. These represent "broken" endpoints.

```
select e.DatabaseSchema
     , e.DatabaseObject
from databases.TApiEndpoint as e
where not exists (
                     select *
                     from INFORMATION_SCHEMA.TABLES as t
                     where TABLE_SCHEMA = e.DatabaseSchema
                           and t.TABLE_NAME = e.DatabaseObject
                 )
order by e.DatabaseSchema
       , e.DatabaseObject
```

### Entity Framework 6

You can generate an entity class for Entity Framework 6 (in a .NET Framework 4.8 library) using the text template `Service/EF6/Entity.tt`.
