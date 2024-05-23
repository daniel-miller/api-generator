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

        public string[] GetToolsets(string toolkit)
        {
            return this.Where(x => x.DomainToolkit == toolkit)
                .Select(x => x.DomainToolset)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
        }

        public string[] GetEntities(string toolkit, string toolset)
        {
            return this.Where(x => x.DomainToolkit == toolkit && x.DomainToolset == toolset)
                .Select(x => x.DomainEntity)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
        }

        public Endpoint GetEndpoint(string toolkit, string toolset, string entity)
        {
            return this.Single(x => x.DomainToolkit == toolkit && x.DomainToolset == toolset && x.DomainEntity == entity);
        }

        public Endpoint GetEndpoint(string toolkit, string entity)
        {
            var endpoints = this.Where(x => x.DomainToolkit == toolkit && x.DomainEntity == entity);
            if (endpoints.Count() == 0)
                throw new Exception($"No endpoint found for entity {entity} in toolkit {toolkit}.");
            if (endpoints.Count() > 1)
                throw new Exception($"Multiple endpoints found for entity {entity} in toolkit {toolkit}.");
            return endpoints.Single();
        }
    }
}
