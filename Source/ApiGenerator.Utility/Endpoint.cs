using System;

namespace ApiGenerator.Utility
{
    [Serializable]
    public class Endpoint
    {
        public string Component { get; set; }

        public string DatabaseSchema { get; set; }
        public string DatabaseObject { get; set; }

        public string PrimaryKey { get; set; }
        public int PrimaryKeySize { get; set; }

        public string DomainToolkit { get; set; }
        public string DomainFeature { get; set; }
        public string DomainEntity { get; set; }

        public string EndpointBase { get; set; }
        public string EndpointCollection { get; set; }
        public string EndpointItem { get; set; }

        public string Components 
            => Inflector.PluralizeNoun(Component);
        
        public string DomainEntities 
            => Inflector.PluralizeNoun(DomainEntity);

        public string Layer
        {
            get 
            { 
                if (Component == "Procedure" || Component == "Table" || Component == "View")
                    return "Data";

                if (Component == "Projection")
                    return "State";

                return "Process";
            }
        }
    }
}
