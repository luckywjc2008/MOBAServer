using System;
using System.Collections.Generic;
using System.Linq;
using Photon.SocketServer;
using MOBAServer.Cache;
using MobaCommon.Code;
using MOBAServer.Room;
using LitJson;
using MobaCommon.Dto;
using MOBAServer.Model;

namespace MOBAServer.Logic
{
    public class SelectHandler :SingleSend, IOpHandler
    {
        //开始战斗事件
        public Action<List<SelectModel>, List<SelectModel>> StartFightEvent;
        /// <summary>
        /// 账号缓存
        /// </summary>
        private SelectCache selectCache = Caches.Select;

        /// <summary>
        /// 账号缓存
        /// </summary>
        private PlayerCache playerCache = Caches.Player;


        /// <summary>
        /// 开始选人
        /// </summary>
        /// <param name="team1"></param>
        /// <param name="team2"></param>
        public void StartSelect(List<int> team1, List<int> team2)
        {
            //创建一个选人房间
            selectCache.CreatRoom(team1, team2);
        }

        public void OnDisconnect(MobaClient client)
        {
            //找到要解散的房间，通知房间解散
            selectCache.OffLine(client,playerCache.GetId(client));
        }

        public void OnRequest(MobaClient client, byte subCode, OperationRequest request)
        {
            switch (subCode)
            {
                case OpSelect.Enter:
                    OnEnter(client);
                    break;
                case OpSelect.Select:
                    OnSelect(client,(int)request[0]);
                    break;
                case OpSelect.Ready:
                    OnReady(client);
                    break;
                case OpSelect.Talk:
                    OnTalk(client,request[0].ToString());
                    break;
                default:
                    break;
            }
        }

        private void OnTalk(MobaClient client,string context)
        {
            //给房间内的当前客户端所有人发一条消息：context
            PlayerModel player = playerCache.GetModel(client);
            if (player == null)
            {
                return;
            }
            SelectRoom room = selectCache.GetRoom(player.Id);
            if (room == null)
            {
                return;
            }
            string str = string.Format("{0} : {1}", player.Name, context);
            room.Brocast(OpCode.SelectCode,OpSelect.Talk,0,"有玩家发言了",null, str);
        }

        /// <summary>
        /// 玩家确认英雄
        /// </summary>
        /// <param name="client"></param>
        private void OnReady(MobaClient client)
        {
            int playerId = playerCache.GetId(client);
            SelectRoom room = selectCache.OnReady(playerId);
            if (room == null)
            {
                this.Send(client,OpCode.SelectCode,OpSelect.Ready,-1,"确认失败");
            }
            else
            {
                room.Brocast(OpCode.SelectCode, OpSelect.Ready, 0, "有人确认选择了", null, playerId);
                //判断是否全部准备了，如果全部准备了，就开始战斗
                if (room.IsAllReady)
                {
                    //通知开始战斗
                    this.StartFightEvent(room.team1Dict.Values.ToList(), room.team2Dict.Values.ToList());
                    //给客户端发送准备战斗，切换场景
                    room.Brocast(OpCode.SelectCode,OpSelect.StartFight,0,"准备进入战斗");

                    //销毁房间
                    selectCache.Destroy(room.Id);
                }
            }
        }

        /// <summary>
        /// 玩家选择英雄
        /// </summary>
        private void OnSelect(MobaClient client,int heroId)
        {
            int playerId = playerCache.GetId(client);
            SelectRoom room = selectCache.Select(playerId, heroId);
            if (room == null)
            {
                this.Send(client,OpCode.SelectCode,OpSelect.Select,-1,"选择英雄失败");
                return;
            }
            //给房间的所有人发送一条谁选了谁(playerId,heroId)
            room.Brocast(OpCode.SelectCode,OpSelect.Select,0,"有人选择英雄了",null,playerId,heroId);
        }

        /// <summary>
        /// 进入选人
        /// </summary>
        private void OnEnter(MobaClient client)
        {
            SelectRoom room = selectCache.Enter(playerCache.GetId(client), client);
            if (room == null)
            {
                return;
            }
            //先给此客户端发一个房间模型
            this.Send(client, OpCode.SelectCode, OpSelect.GetInfo, 0, "获取房间模型",JsonMapper.ToJson(room.team1Dict.Values.ToArray()),JsonMapper.ToJson(room.team2Dict.Values.ToArray()));

            //再给客户端发一条消息: 有人进入了
            room.Brocast(OpCode.SelectCode, OpSelect.Enter, 0, "有玩家进入", client, playerCache.GetId(client));
        }

    }
}
