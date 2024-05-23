using System;

namespace ApiGenerator.Utility
{
    [Serializable]
    public class Endpoint
    {
        public string DatabaseObject { get; set; }
        public string DatabaseSchema { get; set; }
        public string DatabaseTable { get; set; }
        public string DatabasePrimaryKey { get; set; }
        public int DatabasePrimaryKeySize { get; set; }

        public string DomainToolkit { get; set; }
        public string DomainToolset { get; set; }
        public string DomainEntity { get; set; }

        public string EndpointBase { get; set; }
        public string EndpointCollection { get; set; }
        public string EndpointItem { get; set; }

        public string DatabaseObjects => Inflector.PluralizeNoun(DatabaseObject);
    }
}
