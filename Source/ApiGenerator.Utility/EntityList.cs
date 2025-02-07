using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiGenerator.Utility
{
    [Serializable]
    public class EntityList : List<Entity>
    {
        public string[] GetComponents()
        {
            return this.Select(x => x.ComponentName)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
        }

        public string[] GetFeatures(string component)
        {
            return this.Where(x => x.ComponentName == component)
                .Select(x => x.ComponentFeature)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
        }

        public string[] GetEntities(string component, string feature)
        {
            return this.Where(x => x.ComponentName == component && x.ComponentFeature == feature)
                .Select(x => x.EntityName)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
        }

        public string[] GetFutureSchemaChanges(string component, string feature)
        {
            var features = this.Where(x => x.ComponentName == component && x.ComponentFeature == feature)
                .OrderBy(x => x.StorageTable)
                .ThenBy(x => x.StorageSchema)
                .ToArray();

            var list = new List<string>();

            foreach (var item in features)
            {
                var futureSchemaName = item.ComponentName.ToLower();
                
                if (item.StorageSchema != futureSchemaName)
                    list.Add($"* Move table `{item.StorageTable}` from schema `{item.StorageSchema}` to schema `{futureSchemaName}`.");

                if (item.StorageTable != item.FutureStorageTable)
                    list.Add($"* Rename table from `{item.StorageTable}` to `{item.FutureStorageTable}`.");
            }

            return list.ToArray();
        }

        public Entity GetEntity(string component, string feature, string entity)
        {
            var entities = this.Where(x => x.ComponentName == component && x.ComponentFeature == feature && x.EntityName == entity);
            
            if (entities.Count() == 0)
                throw new Exception($"No entity found: entity {entity}; feature {feature}; component {component}.");
            
            if (entities.Count() > 1)
                throw new Exception($"Multiple entities found: entity {entity}; feature {feature}; component {component}.");
            
            return entities.Single();
        }
    }
}
