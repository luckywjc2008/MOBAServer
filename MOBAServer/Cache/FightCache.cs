using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using MobaCommon.Dto;
using MOBAServer.Room;

namespace MOBAServer.Cache
{
    public class FightCache : RoomCacheBase<FightRoom>
    {
        /// <summary>
        /// 创建战斗房间
        /// </summary>
        /// <param name="team1"></param>
        /// <param name="team2"></param>
        public void CreatRoom(List<SelectModel> team1, List<SelectModel> team2)
        {
            FightRoom room = null;
            //检查有没有重用房间
            if (!roomQue.TryDequeue(out room))
            {
                room = new FightRoom(index++,team1.Count+team2.Count);
            }
            //初始化房间数据
            room.Init(team1,team2);
            //添加映射关系
            foreach (SelectModel item in team1)
            {
                playerRoomDict.TryAdd(item.playerId, room.Id);
            }
            foreach (SelectModel item in team2)
            {
                playerRoomDict.TryAdd(item.playerId, room.Id);
            }

            idRoomDict.TryAdd(room.Id, room);
        }

        /// <summary>
        /// 销毁房间
        /// </summary>
        /// <param name="roomId"></param>
        public void Destroy(int roomId)
        {
            FightRoom room = null;

            //移除房间id和房间的关系
            if (!idRoomDict.TryRemove(roomId, out room))
            {
                return;
            }
            //移除玩家ID和房间id的关系
            foreach (HeroModel item in room.Heros)
            {
                playerRoomDict.TryRemove(item.Id, out roomId);
            }

            //清空房间内的数据
            room.Clear();
            //入重用队列
            roomQue.Enqueue(room);
        }

        public FightRoom GetRoom(int playerId, MobaClient client)
        {
            int roomId = -1;
            if (!playerRoomDict.TryGetValue(playerId,out roomId))
            {
                return null;
            }
            FightRoom room = null;
            if (!idRoomDict.TryGetValue(roomId,out room))
            {
                return null;
            }

            return room;
        }

        /// <summary>
        /// 进入战斗
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public FightRoom Enter(int playerId, MobaClient client)
        {
            FightRoom room = GetRoom(playerId, client);
            if (room == null)
            {
                return null;
            }

            room.Enter(client);
            return room;
        }
        /// <summary>
        /// 客户端下线
        /// </summary>
        /// <param name="client"></param>
        /// <param name="playerId"></param>
        public void OffLine(MobaClient client, int playerId)
        {
            FightRoom room = GetRoom(playerId, client);
            if (room == null)
            {
                return;
            }
            //调用离开方法
            room.Leave(client);
            //判断房间还有没有人，没有就销毁
            if (!room.IsAllLeave)
            {
                return;
            }
            //销毁房间
            Destroy(room.Id);
        }

    }
}
