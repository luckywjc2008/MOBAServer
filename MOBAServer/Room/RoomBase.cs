using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Threading;
using Photon.SocketServer;

namespace MOBAServer.Room
{
    /// <summary>
    /// 房间的一个基类
    /// </summary>
    public class RoomBase<TClient> where TClient : ClientPeer
    {
        /// <summary>
        /// 房间Id
        /// </summary>
        public int Id;
        /// <summary>
        /// 连接对象集合
        /// </summary>
        public List<TClient> clientList;
        /// <summary>
        /// 房间的容量
        /// </summary>
        public int Count;
        /// <summary>
        /// 定时器
        /// </summary>
        public Timer timer;
        /// <summary>
        /// 定时任务的Id
        /// </summary>
        public Guid GUID;

        public RoomBase(int id,int count)
        {
            this.Id = id;
            this.Count = count;
            clientList = new List<TClient>(count);
            GUID = new Guid();
            timer = new Timer();
            timer.Start();
        }
        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="client"></param>
        protected bool EnterRoom(TClient client)
        {
            if (clientList.Contains(client))
            {
                return false;
            }
            clientList.Add(client);
            return true;
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="client"></param>
        protected bool LeaveRoom(TClient client)
        {
            if (!clientList.Contains(client))
            {
                return false;
            }
            clientList.Remove(client);
            return true;
        }
        /// <summary>
        /// 开启定时任务
        /// </summary>
        /// <param name="utcTime"></param>
        /// <param name="callBack"></param>
        public void StartSchedule(DateTime utcTime, Action callBack)
        {
            this.GUID = timer.AddAction(utcTime, callBack);
        }

        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="client">收响应客户端</param>
        /// <param name="opCode">操作码</param>
        /// <param name="subCode">子操作</param>
        /// <param name="parameters">参数</param>
        public void Brocast(byte opCode, byte subCode, short reCode, string mess,TClient exClient = null, params object[] parameters)
        {
            OperationResponse response = new OperationResponse();
            response.OperationCode = opCode;
            response.Parameters = new Dictionary<byte, object>();
            response[80] = subCode;
            for (int i = 0; i < parameters.Length; i++)
            {
                response[(byte)i] = parameters[i];
            }

            response.ReturnCode = reCode;
            response.DebugMessage = mess;

            foreach (TClient client in clientList)
            {
                if (client == exClient)
                {
                    continue;
                }
                client.SendOperationResponse(response, new SendParameters());
            }

        }
    }
}
