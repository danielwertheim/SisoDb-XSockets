using System;
using System.ComponentModel.Composition;
using Core;
using PineCone.Structures;
using PineCone.Structures.Schemas;
using SisoDb;
using SisoDb.Dynamic;
using XSocketHandler.Commands;
using XSocketHandler.Results;
using XSockets.Core.Globals;
using XSockets.Core.XSocket;
using XSockets.Core.XSocket.Event.Attributes;
using XSockets.Core.XSocket.Helpers;
using XSockets.Core.XSocket.Interface;

namespace XSocketHandler
{
    [Export(typeof(IXBaseSocket))]
    [XBaseSocketMetadata("SisoDbHandler", Constants.GenericTextBufferSize)]
    public class SisoDbHandler : XBaseSocket
    {
        private readonly ISisoDatabase _db;

        public SisoDbHandler()
        {
            _db = Runtime.Resources.DbResolver();
        }

        public override IXBaseSocket NewInstance()
        {
            return new SisoDbHandler();
        }

        [HandlerEvent("Ping")]
        public void Ping(PingCommand command)
        {
            this.AsyncSend(new PingResult { Message = command.Message }, "OnPinged");
        }

        [HandlerEvent("Insert")]
        public void Insert(InsertCommand command)
        {
            var structureType = Runtime
                .Resources
                .StructureTypeResolver(command.StructureName);

            var result = new InsertResult
            {
                StructureName = command.StructureName,
                Json = _db.UseOnceTo().InsertJson(structureType, command.Json)
            };

            this.AsyncSend(result, "OnInserted");
        }

        [HandlerEvent("DeleteById")]
        public void DeleteById(DeleteByIdCommand command)
        {
            var structureType = Runtime
                .Resources
                .StructureTypeResolver(command.StructureName);
            var structureSchema = GetStructureSchema(structureType);

            var id = ConvertId(command.Id, structureSchema);

            _db.UseOnceTo().DeleteById(structureType, id);

            var result = new DeleteByIdResult
            {
                StructureName = command.StructureName,
                Id = command.Id
            };

            this.AsyncSend(result, "OnDeletedById");
        }

        [HandlerEvent("GetById")]
        public void GetById(GetByIdCommand command)
        {
            var structureType = Runtime
                .Resources
                .StructureTypeResolver(command.StructureName);
            var structureSchema = GetStructureSchema(structureType);

            var id = ConvertId(command.Id, structureSchema);
            var result = new GetByIdResult
            {
                StructureName = command.StructureName,
                Id = command.Id,
                Json = _db.UseOnceTo().GetByIdAsJson(structureType, id)
            };

            this.AsyncSend(result, "OnGetById");
        }

        [HandlerEvent("Update")]
        public void Update(UpdateCommand command)
        {
            var structureType = Runtime
                .Resources
                .StructureTypeResolver(command.StructureName);
            var structure = _db.Serializer.Deserialize(structureType, command.Json);
            var structureSchema = GetStructureSchema(structureType);

            _db.UseOnceTo().Update(structureType, structure);
            
            var result = new UpdateResult
            {
                StructureName = command.StructureName,
                Id = structureSchema.IdAccessor.GetValue(structure).Value.ToString()
            };

            this.AsyncSend(result, "OnUpdated");
        }

        [HandlerEvent("Query")]
        public void Query(QueryCommand command)
        {
            var structureType = Runtime
                .Resources
                .StructureTypeResolver(command.StructureName);
            
            var result = new QueryResult
            {
                StructureName = command.StructureName
            };

            using (var session = _db.BeginSession())
            {
                result.Json = session.Query(structureType).Where(command.Predicate).ToArrayOfJson();
            }

            this.AsyncSend(result, "OnQuery");
        }

        private IStructureSchema GetStructureSchema(Type structureType)
        {
            return _db.StructureSchemas.GetSchema(structureType);
        }

        private static object ConvertId(string id, IStructureSchema structureSchema)
        {
            return StructureId.Create(id, structureSchema.IdAccessor.IdType).Value;
        }
    }
}
