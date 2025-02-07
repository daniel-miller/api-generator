using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ApiGenerator.Utility
{
    public class Generator
    {
        private Entity _entity;
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

        public Generator(Entity entity, string package)
        {
            _entity = entity;
            _package = package;

            _database = new Database();

            var table = _database.GetTable(_entity.StorageSchema, _entity.StorageTable);
            var primaryKey = _entity.StorageKey.Split(',');

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

        public string CollectionSlug()
        {
            var apiSpace = _entity.ComponentName + "Api";

            if (_entity.ComponentFeature != "-")
                apiSpace += "." + _entity.ComponentFeature;
            
            if (_entity.EntityName != "-")
                apiSpace += "." + _entity.EntityName;
            
            return apiSpace;
        }

        public string ApiGroup()
        {
            var apiGroup = _entity.ComponentName;

            if (_entity.ComponentFeature != "-")
                apiGroup += ": " + _entity.ComponentFeature;
            
            return apiGroup;
        }

        public string BuildQuery()
        {
            var criteria = new List<DataColumn>();

            var table = _database.GetTable(_entity.StorageSchema, _entity.StorageTable);
            var primaryKey = _entity.StorageKey.Split(',');

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
            return Inflector.ConvertFirstLetterToLowercase(_entity.EntityName);
        }

        public string Namespace(string package = null)
        {
            var space = $"{Settings.PlatformName}.{package ?? _package}.{_entity.ComponentName}";

            if (_entity.ComponentName == "Integration" || _entity.ComponentName == "Plugin")
                space += "." + _entity.ComponentFeature;

            return space;
        }

        public string[] StorageKey()
        {
            return _entity.StorageKey.Split(new char[] {','});
        }

        public string PrimaryKeyEqualityExpression()
        {
            return _pkEqualityExpression;
        }

        public string PrimaryKeyMethodArguments(bool stripIdentifierSuffix = false, string argument = null)
        {
            var table = _database.GetTable(_entity.StorageSchema, _entity.StorageTable);
            var primaryKey = _entity.StorageKey.Split(',');

            var pkMethodArguments = "";

            for (int i = 0; i < primaryKey.Length; i++)
            {
                var column = table.Columns[primaryKey[i]];
                var typeName = Database.TypeNameOrAlias(column.DataType);
                var variableName = stripIdentifierSuffix ? column.ColumnName.Replace("Identifier","") : column.ColumnName;

                if (i > 0)
                {
                    pkMethodArguments += ", ";
                }

                if (argument != null)
                {
                    pkMethodArguments += argument + "." + variableName;
                }
                else
                {
                    pkMethodArguments += Inflector.ConvertFirstLetterToLowercase(variableName);
                }
                
            }

            return pkMethodArguments;
        }

        public string PrimaryKeyMethodParameters(bool stripIdentifierSuffix = false)
        {
            var table = _database.GetTable(_entity.StorageSchema, _entity.StorageTable);
            var primaryKey = _entity.StorageKey.Split(',');

            var pkMethodParameters = "";

            for (int i = 0; i < primaryKey.Length; i++)
            {
                var column = table.Columns[primaryKey[i]];
                var typeName = Database.TypeNameOrAlias(column.DataType);
                var variableName = Inflector.ConvertFirstLetterToLowercase(column.ColumnName);
                variableName = stripIdentifierSuffix ? variableName.Replace("Identifier", "") : variableName;

                if (i > 0)
                {
                    pkMethodParameters += ", ";
                }

                pkMethodParameters += (_package == "Api" ? "[FromRoute] " : "") + typeName + " " + variableName;
            }

            return pkMethodParameters;
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

            var table = _database.GetTable(_entity.StorageSchema, _entity.StorageTable);
            var primaryKey = _entity.StorageKey.Split(',');

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

            if (_entity.EntityName == "Module")
            {
                statements += $"\r\nusing ModuleHandle = {Settings.PlatformName}.{package}.Curriculum.ModuleHandle;";
            }

            return statements;
        }
    }
}
