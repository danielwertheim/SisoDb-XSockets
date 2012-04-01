using System.ComponentModel.Composition;
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
      public override IXBaseSocket NewInstance()
      {
          return new SisoDbHandler();
      }

      [HandlerEvent("Ping")]
      public void Ping(PingCommand command)
      {
          this.AsyncSend(new PingResult { Message = command.Message }, "OnPinged");
      }
  }  
}
