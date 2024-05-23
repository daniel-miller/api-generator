using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ApiGenerator.Utility
{
    public enum PropertyType
    {
        All,
        OnlyPrimaryKey,
        ExcludePrimaryKey
    }

    public class Generator
    {
        private Database _database;
        private Endpoint _endpoint;

        private string _pkMethodParameters;
        private string _pkMethodArguments;
        private string _pkEqualityExpression;

        private string _pkPropertyNames;
        private string _pkPropertyValues;
        private string _pkPropertyValuesForCreate;
        private string _pkPropertyValuesForGet;
        private string _pkPropertyValuesForModify;

        public Generator(Endpoint endpoint, bool isController = false)
        {
            _endpoint = endpoint;
            _database = new Database();

            var table = _database.GetTable(_endpoint.DatabaseSchema, _endpoint.DatabaseTable);
            var primaryKey = _endpoint.DatabasePrimaryKey.Split(',');

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
                
                _pkMethodParameters += (isController ? "[FromRoute] " : "") + typeName + " " + variableName;
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
            if (_endpoint.DomainToolset != "-")
                apiSpace += "." + _endpoint.DomainToolset;
            if (_endpoint.DomainEntity != "-")
                apiSpace += "." + _endpoint.DomainEntity;
            return apiSpace;
        }

        public string ApiGroup()
        {
            var apiGroup = _endpoint.DomainToolkit;
            if (_endpoint.DomainToolset != "-")
                apiGroup += ": " + _endpoint.DomainToolset;
            return apiGroup;
        }

        public string BuildQuery()
        {
            var criteria = new List<DataColumn>();

            var table = _database.GetTable(_endpoint.DatabaseSchema, _endpoint.DatabaseTable);

            for (int i = 0; i < table.PrimaryKey.Length; i++)
                criteria.Add(table.PrimaryKey[i]);

            foreach (DataColumn column in table.Columns)
            {
                if (column.ColumnName == "OrganizationIdentifier" || column.ColumnName == "OriginOrganization")
                    if (!criteria.Any(x => x.ColumnName == column.ColumnName))
                        criteria.Add(column);
            }

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

        public string[] PrimaryKey()
        {
            return _endpoint.DatabasePrimaryKey.Split(new char[] {','});
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

        public string PropertyDeclarations(PropertyType type, int tabs = 2)
        {
            var indent = new string(' ', tabs * 4);

            var table = _database.GetTable(_endpoint.DatabaseSchema, _endpoint.DatabaseTable);

            var columns = new List<DataColumn>();
            foreach (DataColumn column in table.Columns)
            {
                if (type == PropertyType.OnlyPrimaryKey && !_endpoint.DatabasePrimaryKey.Contains(column.ColumnName))
                    continue;

                if (type == PropertyType.ExcludePrimaryKey && _endpoint.DatabasePrimaryKey.Contains(column.ColumnName))
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

                if (column.DataType != typeof(string) && column.DataType != typeof(byte[]) && column.AllowDBNull)
                    datatype += "?";

                if (declarations != string.Empty && lasttype != typeName)
                    declarations += "\r\n";

                declarations += indent + "public " + datatype + " " + column.ColumnName + " { get; set; }";
                
                if (i < columns.Count - 1)
                    declarations += "\r\n";

                lasttype = typeName;
            }

            return declarations;
        }
    }
}
