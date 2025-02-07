using System;
using System.Linq;

namespace ApiGenerator.Utility
{
    [Serializable]
    public class Entity
    {
        public string ComponentType { get; set; }
        public string ComponentName { get; set; }
        public string ComponentFeature { get; set; }
        
        public string EntityName { get; set; }

        public string CollectionSlug { get; set; }
        public string CollectionKey { get; set; }

        public string StorageStructure { get; set; }
        public string StorageSchema { get; set; }
        public string StorageTable { get; set; }
        public string StorageKey { get; set; }

        public string FutureStorageTable { get; set; }

        public int StorageKeySize => StorageKey.Count(x => x == ',');

        public string StorageStructures
            => Inflector.PluralizeNoun(StorageStructure);
        
        public string EntityNames
            => Inflector.PluralizeNoun(EntityName);
    }
}
