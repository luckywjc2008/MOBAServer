using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using MOBAServer.Cache;
using MOBAServer.Model;
using MobaCommon.Dto;

namespace MOBAServer.Room
{ 
    public class SelectRoom :RoomBase<MobaClient>
    {
        /// <summary>
        /// 队伍1玩家Id对应的选择模型
        /// </summary>
        public ConcurrentDictionary<int, SelectModel> team1Dict;
        /// <summary>
        /// 队伍2玩家Id对应的选择模型
        /// </summary>
        public ConcurrentDictionary<int, SelectModel> team2Dict;
        /// <summary>
        /// 进入的数量
        /// </summary>
        public int enterCount;

        /// <summary>
        /// 是否全部进入
        /// </summary>
        public bool IsAllEnter
        {
            get { return enterCount >= Count; }
        }

        /// <summary>
        /// 准备的数量
        /// </summary>
        public int readyCount;
        /// <summary>
        /// 是否全部准备
        /// </summary>
        public bool IsAllReady
        {
            get { return readyCount >= Count; }
        }

        public SelectRoom(int id, int count) : base(id, count)
        {
            this.team1Dict = new ConcurrentDictionary<int, SelectModel>();
            this.team2Dict = new ConcurrentDictionary<int, SelectModel>();
            this.enterCount = 0;
            this.readyCount = 0;
        }

        /// <summary>
        /// 初始化房间
        /// </summary>
        /// <param name="team1"></param>
        /// <param name="team2"></param>
        public void InitRoom(List<int> team1, List<int> team2)
        {
            //初始化房间
            PlayerModel pm;
            SelectModel model;
            foreach (int item in team1)
            {
                pm = Caches.Player.GetModel(item);
                model = new SelectModel(pm.Id,pm.Name);
                //添加映射
                team1Dict.TryAdd(pm.Id, model);
            }
            foreach (int item in team2)
            {
                pm = Caches.Player.GetModel(item);
                model = new SelectModel(pm.Id, pm.Name);
                //添加映射
                team2Dict.TryAdd(pm.Id, model);
            }
        }
        /// <summary>
        /// 进入房间
        /// </summary>
        public void Enter(int playerId,MobaClient client)
        {
            if (team1Dict.ContainsKey(playerId))
            {
                team1Dict[playerId].isEnter = true;
            }
            else if (team2Dict.ContainsKey(playerId))
            {
                team2Dict[playerId].isEnter = true;
            }
            else
            {
                return;
            }
            //添加房间内连接对象
            clientList.Add(client);
            //更新房间内进入的人数
            enterCount++;
        }
        /// <summary>
        /// 选择英雄
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="heroId"></param>
        public bool Select(int playerId, int heroId)
        {
            //队友有没有选择该英雄
            if (team1Dict.ContainsKey(playerId))
            {
                foreach(SelectModel item in team1Dict.Values)
                {
                    if (item.heroId == heroId)
                    {
                        return false;
                    }
                }
                //自己可以选择该英雄
                team1Dict[playerId].heroId = heroId;
            }
            else if (team2Dict.ContainsKey(playerId))
            {
                foreach (SelectModel item in team2Dict.Values)
                {
                    if (item.heroId == heroId)
                    {
                        return false;
                    }
                }
                //自己可以选择该英雄
                team2Dict[playerId].heroId = heroId;
            }
            return true;
        }
        /// <summary>
        /// 玩家确认选择
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public bool Ready(int playerId)
        {
            if (team1Dict.ContainsKey(playerId))
            {
                SelectModel model = team1Dict[playerId];
                if (model.heroId == -1)
                {
                    return false;
                }
                else
                {
                    model.isReady = true;
                    //更新准备的人数
                    readyCount++;
                }
            }else if (team2Dict.ContainsKey(playerId))
            {
                SelectModel model = team2Dict[playerId];
                if (model.heroId == -1)
                {
                    return false;
                }
                else
                {
                    model.isReady = true;
                    //更新准备的人数
                    readyCount++;
                }
            }
            return true;
        }
    }
}
