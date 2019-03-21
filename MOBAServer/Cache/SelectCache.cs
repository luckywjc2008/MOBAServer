using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using MOBAServer.Room;
using MobaCommon.Code;
using MobaCommon.Dto;

namespace MOBAServer.Cache
{
    public class SelectCache : RoomCacheBase<SelectRoom>
    {
        /// <summary>
        /// 玩家下线
        /// </summary>
        /// <param name="client"></param>
        public void OffLine(MobaClient client,int playerId)
        {
            SelectRoom room = GetRoom(playerId);
            if (room != null)
            {
                //移除退出的客户端
                room.clientList.Remove(client);
                //给剩余的客户端发一个消息，有人退出了 房间解散 回到主界面
                room.Brocast(OpCode.SelectCode, OpSelect.Destroy, 0, "有人退出");
                Destroy(room.Id);
            }
        }

        /// <summary>
        /// 开始选人
        /// </summary>
        public void CreatRoom(List<int> team1,List<int> team2)
        {
            SelectRoom room = null;
            if (!roomQue.TryDequeue(out room))
            {
                room = new SelectRoom(index++,team1.Count + team2.Count);
            }

            room.InitRoom(team1,team2);
            //绑定玩家Id和房间Id
            foreach (int item in team1)
            {
                playerRoomDict.TryAdd(item, room.Id);
            }
            foreach (int item in team2)
            {
                playerRoomDict.TryAdd(item, room.Id);
            }
            //绑定房间id和房间
            idRoomDict.TryAdd(room.Id, room);
            //创建房间成功
            //开启一个定时任务，通知玩家10s内进入房间，否则房间自动销毁
            room.StartSchedule(DateTime.UtcNow.AddSeconds(10d), () =>
            {
                //销毁房间
                if (!room.IsAllEnter)
                {  
                    room.Brocast(OpCode.SelectCode, OpSelect.Destroy, 0, "有人未进入 解散当前选人");
                    Destroy(room.Id);
                }

            });
        }
        /// <summary>
        /// 销毁房间
        /// </summary>
        /// <param name="roomId"></param>
        public void Destroy(int roomId)
        {
            SelectRoom room =null;

            //移除房间id和房间的关系
            if (!idRoomDict.TryRemove(roomId, out room))
            {
                return;
            }
            //移除玩家ID和房间id的关系
            foreach (int item in room.team1Dict.Keys)
            {
                playerRoomDict.TryRemove(item, out roomId);
            }
            foreach (int item in room.team2Dict.Keys)
            {
                playerRoomDict.TryRemove(item, out roomId);
            }

            //清空房间内的数据
            room.team1Dict.Clear();
            room.team2Dict.Clear();
            room.clientList.Clear();
            room.enterCount = 0;
            room.readyCount = 0;
            //入重用队列
            roomQue.Enqueue(room);
        }
        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="client"></param>
        public SelectRoom Enter(int playerId, MobaClient client)
        {
            int roomId = -1;
            if (!playerRoomDict.TryGetValue(playerId,out roomId))
            {
                return null;
            }
            SelectRoom room = null;
            if (!idRoomDict.TryGetValue(roomId,out room))
            {
                return null; 
            }

            room.Enter(playerId, client);
            return room;
        }
        /// <summary>
        /// 获得房间模型
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public SelectRoom GetRoom(int playerId)
        {
            int roomId = -1;
            if (!playerRoomDict.TryGetValue(playerId, out roomId))
            {
                return null;
            }
            SelectRoom room = null;
            if (!idRoomDict.TryGetValue(roomId, out room))
            {
                return null;
            }

            return room;
        }

        public SelectModel GetSelectModel(int playerId)
        {
            SelectRoom room = GetRoom(playerId);
            SelectModel model = null;
            if (room != null)
            {
                room.team1Dict.TryGetValue(playerId, out model);
                if (model == null)
                {
                    room.team2Dict.TryGetValue(playerId, out model);
                    return model;
                }
                else
                {
                    return model;
                }
            }
            return null;
        }
        /// <summary>
        /// 选择英雄
        /// </summary>
        /// <returns></returns>
        public SelectRoom Select(int playerId,int heroId)
        {
            int roomId = -1;
            if (!playerRoomDict.TryGetValue(playerId, out roomId))
            {
                return null;
            }
            SelectRoom room = null;
            if (!idRoomDict.TryGetValue(roomId, out room))
            {
                return null;
            }

            if (room.Select(playerId, heroId))
            {
                return room;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 确认选择
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public SelectRoom OnReady(int playerId)
        {
            int roomId = -1;
            if (!playerRoomDict.TryGetValue(playerId, out roomId))
            {
                return null;
            }
            SelectRoom room = null;
            if (!idRoomDict.TryGetValue(roomId, out room))
            {
                return null;
            }

            if (!room.Ready(playerId))
            {
                return null;
            }
            else
            {
                return room;
            }
        }

    }
}
