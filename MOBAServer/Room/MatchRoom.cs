﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOBAServer.Room
{
    public class MatchRoom : RoomBase<MobaClient>
    {
        /// <summary>
        /// 队伍1玩家ID
        /// </summary>
        public List<int> Team1IdList;
        /// <summary>
        /// 队伍2玩家ID
        /// </summary>
        public List<int> Team2IdList;
        public MatchRoom(int id, int count) : base(id, count)
        {
            int teamCount = count/2;
            Team1IdList = new List<int>(teamCount);
            Team2IdList = new List<int>(teamCount);
        }
        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="client"></param>
        /// <param name="playerId"></param>
        public bool Enter(MobaClient client,int playerId)
        {
            if (Team1IdList.Capacity > Team1IdList.Count && !Team1IdList.Contains(playerId))
            {
                Team1IdList.Add(playerId);
                base.EnterRoom(client);
                return true;
            }
            else if (Team2IdList.Capacity > Team2IdList.Count && !Team2IdList.Contains(playerId))
            {
                Team2IdList.Add(playerId);
                base.EnterRoom(client);
                return true;
            }

            return false;
        }

        public bool IsFull
        {
            get { return base.clientList.Capacity == clientList.Count; }
        }

        public bool Leave(MobaClient client, int playerId)
        {
            if (Team1IdList.Contains(playerId))
            {
                Team1IdList.Remove(playerId);
                base.LeaveRoom(client);
                return true;
            }
            else if (Team2IdList.Contains(playerId))
            {
                Team2IdList.Remove(playerId);
                base.LeaveRoom(client);
                return true;
            }

            return false;
        }
        /// <summary>
        /// 房间是否空
        /// </summary>
        public bool IsEmpty
        {
            get { return clientList.Count == 0; }
        }
    }
}
