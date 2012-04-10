using System;
using System.Collections.Generic;
using Core;
using Model;
using SisoDb;
using SisoDb.Sql2012;
using System.Linq;

namespace Server
{
    public class Resources : IResources
    {
        private readonly ISisoDatabase _db;
        private readonly IDictionary<string, Type> _structureTypes;

        public Func<ISisoDatabase> DbResolver { get; set; }
        public Func<string, Type> StructureTypeResolver { get; set; }

        public Resources()
        {
            //Func should resolve the SAME ISisoDatabase each time
            _db = new Sql2012DbFactory().CreateDatabase(new Sql2012ConnectionInfo("Demo"));
            _db.CreateIfNotExists();
            DbResolver = () => _db;

            //Simple key-value map for acceptable structure types
            _structureTypes = GetStructureTypes().ToDictionary(t => t.Name);
            StructureTypeResolver = name => _structureTypes[name];
        }

        private IEnumerable<Type> GetStructureTypes()
        {
            //This determines what Entities/Structures that will be accessible
            yield return typeof(Article);
        }
    }
}