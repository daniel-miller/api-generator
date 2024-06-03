using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiGenerator.Utility
{
    [Serializable]
    public class EndpointList : List<Endpoint>
    {
        public string[] GetToolkits()
        {
            return this.Select(x => x.DomainToolkit)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
        }

        public string[] GetFeatures(string toolkit)
        {
            return this.Where(x => x.DomainToolkit == toolkit)
                .Select(x => x.DomainFeature)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
        }

        public string[] GetEntities(string toolkit, string feature)
        {
            return this.Where(x => x.DomainToolkit == toolkit && x.DomainFeature == feature)
                .Select(x => x.DomainEntity)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
        }

        public Endpoint GetEndpoint(string toolkit, string feature, string entity)
        {
            var endpoints = this.Where(x => x.DomainToolkit == toolkit && x.DomainFeature == feature && x.DomainEntity == entity);
            
            if (endpoints.Count() == 0)
                throw new Exception($"No endpoint found: entity {entity}; feature {feature}; toolkit {toolkit}.");
            
            if (endpoints.Count() > 1)
                throw new Exception($"Multiple endpoints found: entity {entity}; feature {feature}; toolkit {toolkit}.");
            
            return endpoints.Single();
        }
    }
}
