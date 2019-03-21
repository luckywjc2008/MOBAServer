using System.Runtime.CompilerServices;
using LitJson;
using MobaCommon.Code;
using MobaCommon.Dto;
using MOBAServer.Cache;
using MOBAServer.Model;
using MOBAServer.Room;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MOBAServer.Logic
{ 
    public class PlayerHandler : SingleSend,IOpHandler
    {
        public Action<List<int>, List<int>> StartSelectEvent;

        /// <summary>
        /// 账号缓存
        /// </summary>
        private AccountCache accountCache = Caches.Account;
        /// <summary>
        /// 账号缓存
        /// </summary>
        private PlayerCache playerCache = Caches.Player;
        /// <summary>
        /// 匹配缓存
        /// </summary>
        private MatchCache matchCache = Caches.Match;
        public void OnDisconnect(MobaClient client)
        {
            //每次下线的时候要通知好友显示离线状态
            PlayerModel model = playerCache.GetModel(client);
            if (model != null)
            {
                string[] friends = model.FriendIdList.Split(',');
                foreach (string item in friends)
                {
                    if (item.Equals(""))
                    {
                        continue;
                    }
                    int itemId = int.Parse(item);
                    if (!playerCache.IsOnLine(itemId))
                    {
                        continue;
                    }
                    MobaClient tempClient = playerCache.GetClient(itemId);
                    Send(tempClient, OpCode.PlayerCode, OpPlayer.FriendOffLine, 0, "此玩家下线", model.Id);
                }

                matchCache.OffLine(client, playerCache.GetId(client));
                playerCache.OffLine(client);
            }
        }

        public void OnRequest(MobaClient client, byte subCode, OperationRequest request)
        {
            switch (subCode)
            {
                case OpPlayer.GetInfo:
                    OnGetInfo(client);
                    break;
                case OpPlayer.Create:
                    string name = request[0].ToString();
                    OnCreate(client, name);
                    break;
                case OpPlayer.OnLine:
                    OnLine(client);
                    break;
                case OpPlayer.RequestAdd:
                    string addName = request[0].ToString();
                    OnAdd(client, addName);
                    break;
                case OpPlayer.ToClientAdd:
                    OnToClientAdd(client,(bool) request[0],(int)request[1]);
                    break;
                case OpPlayer.StartMatch:
                    OnStartMatch(client,(int)request[0]);
                    break;
                case OpPlayer.StopMatch:
                    OnStopMatch(client);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 离开匹配
        /// </summary>
        private void OnStopMatch(MobaClient client)
        {
            bool result = matchCache.StopMatch(client, playerCache.GetId(client));
            if (result == true)
            {
                Send(client,OpCode.PlayerCode,OpPlayer.StopMatch,0,"离开成功");
            }
        }

        /// <summary>
        /// 开始匹配
        /// </summary>
        private void OnStartMatch(MobaClient client,int playerId)
        {
            if (playerCache.GetId(client) != playerId)
            {
                return;
            }
            MatchRoom room = matchCache.StartMatch(client, playerId);
            Send(client, OpCode.PlayerCode, OpPlayer.StartMatch, 0, "开始匹配");
            //如果房间满了就开始选人
            if (room.IsFull == true)
            {
                //开始选人
                StartSelectEvent(room.Team1IdList, room.Team2IdList);
                //发起是否进入选人请求
                room.Brocast(OpCode.PlayerCode, OpPlayer.MatchComplete, 0, "是否进入选人界面(10s内)");
                matchCache.DestroyRoom(room);
            }
        }

        /// <summary>
        /// 响应客户端添加结果
        /// </summary>
        private void OnToClientAdd(MobaClient client,bool result,int requestId)
        {
            MobaClient requestClient = playerCache.GetClient(requestId);
            if (result == true)
            {
               //同意了保存数据
                int playerId = playerCache.GetId(client);
                playerCache.AddFriend(playerId, requestId);
                Send(client, OpCode.PlayerCode, OpPlayer.ToClientAdd, 1, "添加成功",JsonMapper.ToJson(ToDto(playerCache.GetModel(playerId))));
                Send(requestClient, OpCode.PlayerCode, OpPlayer.ToClientAdd, 1, "添加成功", JsonMapper.ToJson(ToDto(playerCache.GetModel(requestId))));
            }
            else
            {
                //拒绝了，回传原来的客户端不同意
                Send(requestClient, OpCode.PlayerCode, OpPlayer.ToClientAdd, -1, "拒绝添加好友");
            }
        }

        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="addName"></param>
        private void OnAdd(MobaClient client, string addName)
        {
            PlayerModel model = playerCache.GetModel(addName);
            if (model == null)
            {
                Send(client, OpCode.PlayerCode, OpPlayer.RequestAdd, -1, "没有此角色");
                return;
            }

            //不允许添加自身
            if (playerCache.GetModel(client).Id == model.Id)
            {
                Send(client, OpCode.PlayerCode, OpPlayer.RequestAdd, -3, "不能添加自己");
                return;
            }
            //已经是好友不需要添加
            string friendIdList = playerCache.GetModel(client).FriendIdList;
            if (!friendIdList.Equals(""))
            {
                string[] friends = friendIdList.Split(',');
                foreach (var item in friends)
                {
                    if (model.Id == int.Parse(item))
                    {
                        Send(client, OpCode.PlayerCode, OpPlayer.RequestAdd, -4, "该玩家已经是好友");
                        return;
                    }
                }
            }

            //如果能获取数据模型，先判断是否在线，不在线回传不在线，在线，发消息，有人向他加好友
            bool isOnLine = playerCache.IsOnLine(model.Id);
            if (!isOnLine)
            {
                Send(client, OpCode.PlayerCode, OpPlayer.RequestAdd, -2, "此玩家不在线");
                return;
            }
            MobaClient toClient = playerCache.GetClient(model.Id);
            PlayerModel curModel = playerCache.GetModel(client);
            Send(toClient, OpCode.PlayerCode, OpPlayer.ToClientAdd, 0, "是否添加好友",JsonMapper.ToJson(ToDto(curModel)));
        }

        /// <summary>
        /// 获取角色信息
        /// </summary>
        private void OnGetInfo(MobaClient client)
        {
            int accId = accountCache.GetId(client);
            if (accId == -1)
            {
                Send(client,OpCode.PlayerCode,OpPlayer.GetInfo,-1,"非法登录");
                return;
            }
            if (playerCache.Has(accId))
            {
                Send(client, OpCode.PlayerCode, OpPlayer.GetInfo, 0, "存在角色");
                return;
            }
            else
            {
                Send(client, OpCode.PlayerCode, OpPlayer.GetInfo, -2, "没有角色");
                return;
            }
            
        }
        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="client"></param>
        /// <param name="name"></param>
        private void OnCreate(MobaClient client,string name)
        {
            int accId = accountCache.GetId(client);
            if (playerCache.Has(accId))
            {
                return;
            }
            if (playerCache.HasNameSame(name))
            {
                Send(client, OpCode.PlayerCode, OpPlayer.Create, -1, "已有相同名称的角色");
            }
            //创建角色
            playerCache.Create(name,accId);
            Send(client,OpCode.PlayerCode,OpPlayer.Create,0,"创建成功");
        }
        /// <summary>
        /// 上线
        /// </summary>
        /// <param name="client"></param>
        private void OnLine(MobaClient client)
        {
            int accId = accountCache.GetId(client);
            int playerId = playerCache.GetId(accId);
            //防止重复在线
            if (playerCache.IsOnLine(client))
            {
                return;
            }
            //上线
            playerCache.OnLine(client,playerId);

            //每次上线的时候要通知好友显示上线状态
            PlayerModel myModel = playerCache.GetModel(client);
            if (myModel != null)
            {
                string[] friends = myModel.FriendIdList.Split(',');
                foreach (string item in friends)
                {
                    if (item.Equals(""))
                    {
                       continue;
                    }
                    int itemId = int.Parse(item);
                    if (!playerCache.IsOnLine(itemId))
                    {
                        continue;
                    }
                    MobaClient tempClient = playerCache.GetClient(itemId);
                    Send(tempClient, OpCode.PlayerCode, OpPlayer.FriendOnLine, 0, "此玩家上线", myModel.Id);
                }
            }

            PlayerModel model = playerCache.GetModel(playerId);
            PlayerDto dto = ToDto(model);

            Send(client,OpCode.PlayerCode,OpPlayer.OnLine,0,"角色上线成功",JsonMapper.ToJson(dto));
        }

        private PlayerDto ToDto(PlayerModel model)
        {
            PlayerDto dto = new PlayerDto()
            {
                id = model.Id,
                exp = model.Exp,
                loseCount = model.LoseCount,
                lv = model.Lv,
                name = model.Name,
                power = model.Power,
                runCount = model.RunCount,
                winCount = model.WinCount,
            };

            //赋值英雄列表
            string[] heroIds = model.HeroIdList.Split(',');
            dto.heroIds = new int[heroIds.Length];
            for (int i = 0; i < heroIds.Length; i++)
            {
                dto.heroIds[i] = int.Parse(heroIds[i]);
            }

            //赋值好友列表
            string friendIdList = model.FriendIdList;
            if (!friendIdList.Equals(""))
            {
                string[] friendIds = friendIdList.Split(',');
                dto.friends = new Friend[friendIds.Length];
                for (int i = 0; i < friendIds.Length; i++)
                {
                    int id = int.Parse(friendIds[i]);
                    PlayerModel otherModel = playerCache.GetModel(id);
                    bool onLine = playerCache.IsOnLine(id);
                    Friend friend = new Friend(otherModel.Id, otherModel.Name, onLine);
                    dto.friends[i] = friend;
                }
            }
            else
            {
                dto.friends = new Friend[0];
            }

            return dto;
        }
    }
}

