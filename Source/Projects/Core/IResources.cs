using System;
using SisoDb;

namespace Core
{
    public interface IResources
    {
        Func<ISisoDatabase> DbResolver { get; set; }
        Func<string, Type> StructureTypeResolver { get; set; } 
    }
}