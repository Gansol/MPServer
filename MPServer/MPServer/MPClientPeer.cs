using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExitGames.Logging;
using MPProtocol;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;

namespace MPServer
{
    public class MPClientPeer : ClientPeer
    {
        protected MPServerApplication _server;

        public MPClientPeer(InitRequest initRequest, MPServerApplication serverApplication)
            : base(initRequest)
        {
            _server = serverApplication;
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            throw new NotImplementedException();
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            throw new NotImplementedException();
        }
    }
}
