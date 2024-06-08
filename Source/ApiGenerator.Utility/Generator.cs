using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ApiGenerator.Utility
{
    public class Generator
    {
        private Endpoint _endpoint;
        private string _package;

        private Database _database;

        private string _pkMethodParameters;
        private string _pkMethodArguments;
        private string _pkEqualityExpression;

        private string _pkPropertyNames;
        private string _pkPropertyValues;
        private string _pkPropertyValuesForCreate;
        private string _pkPropertyValuesForGet;
        private string _pkPropertyValuesForModify;

        public Generator(Endpoint endpoint, string package)
        {
            _endpoint = endpoint;
            _package = package;

            _database = new Database();

            var table = _database.GetTable(_endpoint.DatabaseSchema, _endpoint.DatabaseObject);
            var primaryKey = _endpoint.PrimaryKey.Split(',');

            _pkMethodParameters = "";
            _pkMethodArguments = "";
            _pkEqualityExpression = "";

            _pkPropertyNames = "";
            _pkPropertyValues = "";
            _pkPropertyValuesForCreate = "";
            _pkPropertyValuesForGet = "";
            _pkPropertyValuesForModify = "";

            for (int i = 0; i < primaryKey.Length; i++)
            {
                var column = table.Columns[primaryKey[i]];
                var typeName = Database.TypeNameOrAlias(column.DataType);
                var variableName = Inflector.ConvertFirstLetterToLowercase(column.ColumnName);

                if (i > 0)
                {
                    _pkMethodParameters += ", ";
                    _pkMethodArguments += ", ";
                    _pkEqualityExpression += " && ";

                    _pkPropertyNames += ", ";
                    _pkPropertyValuesForGet += ", ";
                }
                
                _pkMethodParameters += (_package == "Api" ? "[FromRoute] " : "") + typeName + " " + variableName;
                _pkMethodArguments += variableName;
                _pkEqualityExpression += "x." + column.ColumnName + " == " + variableName;

                _pkPropertyNames += "entity." + column.ColumnName;
                _pkPropertyValues += column.ColumnName + " {entity." + column.ColumnName + "}";

                _pkPropertyValuesForCreate += column.ColumnName + " {create." + column.ColumnName + "}";
                _pkPropertyValuesForGet += "create." + column.ColumnName;
                _pkPropertyValuesForModify += column.ColumnName + " {" + variableName + "}";
            }
        }

        public string ApiCollection()
        {
            var apiSpace = _endpoint.DomainToolkit + "Api";

            if (_endpoint.DomainFeature != "-")
                apiSpace += "." + _endpoint.DomainFeature;
            
            if (_endpoint.DomainEntity != "-")
                apiSpace += "." + _endpoint.DomainEntity;
            
            return apiSpace;
        }

        public string ApiGroup()
        {
            var apiGroup = _endpoint.DomainToolkit;

            if (_endpoint.DomainFeature != "-")
                apiGroup += ": " + _endpoint.DomainFeature;
            
            return apiGroup;
        }

        public string BuildQuery()
        {
            var criteria = new List<DataColumn>();

            var table = _database.GetTable(_endpoint.DatabaseSchema, _endpoint.DatabaseObject);
            var primaryKey = _endpoint.PrimaryKey.Split(',');

            for (int i = 0; i < primaryKey.Length; i++)
                criteria.Add(table.Columns[primaryKey[i]]);

            criteria.Sort(new Database.EntityColumnComparer());

            var query = "";
            for (int i = 0; i < criteria.Count; i++)
            {
                var column = criteria[i];
                var typeName = Database.TypeNameOrAlias(column.DataType);

                query += "\r\n";
                query += "        if (criteria." + column.ColumnName + " != null)\r\n";
                query += "            query = query.Where(x => x." + column.ColumnName + " == criteria." + column.ColumnName + ");\r\n";
            }

            return query;
        }

        public string EntityVariable()
        {
            return Inflector.ConvertFirstLetterToLowercase(_endpoint.DomainEntity);
        }

        public string Namespace(string package = null)
        {
            var space = $"{Settings.PlatformName}.{package ?? _package}.{_endpoint.DomainToolkit}";

            if (_endpoint.DomainToolkit == "Integration" || _endpoint.DomainToolkit == "Plugin")
                space += "." + _endpoint.DomainFeature;

            return space;
        }

        public string[] PrimaryKey()
        {
            return _endpoint.PrimaryKey.Split(new char[] {','});
        }

        public string PrimaryKeyEqualityExpression()
        {
            return _pkEqualityExpression;
        }

        public string PrimaryKeyMethodArguments()
        {
            return _pkMethodArguments;
        }

        public string PrimaryKeyMethodParameters()
        {
            return _pkMethodParameters;
        }

        public string PrimaryKeyPropertyNames()
        {
            return _pkPropertyNames;
        }

        public string PrimaryKeyPropertyValues()
        {
            return _pkPropertyValues;
        }

        public string PrimaryKeyPropertyValuesForCreate()
        {
            return _pkPropertyValuesForCreate;
        }

        public string PrimaryKeyPropertyValuesForGet()
        {
            return _pkPropertyValuesForGet;
        }

        public string PrimaryKeyPropertyValuesForModify()
        {
            return _pkPropertyValuesForModify;
        }

        public string PropertyDeclarations(PropertyType type, bool allowNull = false, bool isCore = false, int tabs = 2)
        {
            var indent = new string(' ', tabs * 4);

            var table = _database.GetTable(_endpoint.DatabaseSchema, _endpoint.DatabaseObject);
            var primaryKey = _endpoint.PrimaryKey.Split(',');

            var columns = new List<DataColumn>();
            foreach (DataColumn column in table.Columns)
            {
                if (type == PropertyType.OnlyPrimaryKey && !primaryKey.Contains(column.ColumnName))
                    continue;

                if (type == PropertyType.ExcludePrimaryKey && primaryKey.Contains(column.ColumnName))
                    continue;

                columns.Add(column);
            }
            columns.Sort(new Database.EntityColumnComparer());

            var declarations = "";
            string lasttype = null;

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                var typeName = Database.TypeNameOrAlias(column.DataType);
                var datatype = typeName;

                var isString = column.DataType == typeof(string);
                var isByteArray = column.DataType == typeof(byte[]);
                
                var isRequired = !allowNull && !column.AllowDBNull;

                if (!isRequired && (isCore || !(isString || isByteArray)))
                    datatype += "?";

                if (declarations != string.Empty && lasttype != typeName)
                    declarations += "\r\n";

                declarations += indent + "public " + datatype + " " + column.ColumnName + " { get; set; }";

                if (isCore && (isString || isByteArray) && !datatype.EndsWith("?"))
                    declarations += " = null!;";

                if (i < columns.Count - 1)
                    declarations += "\r\n";

                lasttype = typeName;
            }

            return declarations;
        }

        public string UsingStatements(string package = null)
        {
            string statements = $"using {Namespace(package)};";

            if (_endpoint.DomainEntity == "Module")
            {
                statements += $"\r\nusing ModuleHandle = {Settings.PlatformName}.{package}.Curriculum.ModuleHandle;";
            }

            return statements;
        }
    }
}
