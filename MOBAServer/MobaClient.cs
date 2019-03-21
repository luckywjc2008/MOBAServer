using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobaCommon.Code;
using MOBAServer.Logic;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;

namespace MOBAServer
{
    public class MobaClient : ClientPeer
    {
        //账号逻辑
        private IOpHandler Account;
        private PlayerHandler Player;
        private SelectHandler Select;
        private FightHandler Fight;
        public MobaClient(InitRequest initRequest) : base(initRequest)
        {
            Account = new AccountHandler();
            Player = new PlayerHandler();
            Select = new SelectHandler();
            Fight = new FightHandler();

            Player.StartSelectEvent = Select.StartSelect;
            Select.StartFightEvent = Fight.StartSelect;
        }
        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="reasonCode"></param>
        /// <param name="reasonDetail"></param>
        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            Fight.OnDisconnect(this);
            Select.OnDisconnect(this);
            Player.OnDisconnect(this);
            Account.OnDisconnect(this);
        }
        /// <summary>
        /// 客户端请求
        /// </summary>
        /// <param name="operationRequest"></param>
        /// <param name="sendParameters"></param>
        protected override void OnOperationRequest(OperationRequest request, SendParameters sendParameters)
        {
            MobaApplication.LogInfo(request.ToString());
            byte opCode = request.OperationCode;
            byte SubCode = (byte)request[80];
            switch (opCode)
            {
                case OpCode.AccountCode:
                    Account.OnRequest(this, SubCode, request);
                    break;
                case OpCode.PlayerCode:
                    Player.OnRequest(this, SubCode, request);
                    break;
                case OpCode.SelectCode:
                    Select.OnRequest(this, SubCode, request);
                    break;
                case OpCode.FightCode:
                    Fight.OnRequest(this, SubCode, request);
                    break;
                default:
                    break;
            }
        }
    }
}
