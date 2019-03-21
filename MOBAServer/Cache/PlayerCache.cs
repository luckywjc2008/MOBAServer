using System;
using System.Collections.Generic;
using MOBAServer.Model;
using ExitGames.Threading;
using System.Collections.Concurrent;

namespace MOBAServer.Cache
{
    public class PlayerCache
    {
        #region 数据
        /// <summary>
        /// 玩家ID和玩家数据的映射
        /// </summary>
        private ConcurrentDictionary <int,PlayerModel> idModelDict = new ConcurrentDictionary<int, PlayerModel>();
        /// <summary>
        /// 账号ID对应的玩家ID
        /// </summary>
        private SynchronizedDictionary<int,int> accPlayerDict = new SynchronizedDictionary<int, int>();

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="name"></param>
        /// <param name="accId"></param>
        public void Create(string name, int accId)
        {
            PlayerModel model = new PlayerModel();
            model.Name = name;
            model.AccountId = accId;

            MobaApplication.LogInfo("创建玩家 name = " + name + "accId = " + accId);

            //返回值是自增 键值
            model.Id = model.Add();

            accPlayerDict.TryAdd(accId, model.Id);
            idModelDict.TryAdd(model.Id, model);
        }

        /// <summary>
        /// 判断是否存在角色
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public bool Has(int accountId)
        {
            if (accPlayerDict.ContainsKey(accountId))
            {
                return true;
            }

            PlayerModel model = new PlayerModel();
            if (!model.ExistsByAccId(accountId))
            {
                return false;
            }

            //如果数据库存在
            model.GetModelByAccId(accountId);
            //添加到内存
            accPlayerDict.TryAdd(accountId, model.Id);
            idModelDict.TryAdd(model.Id, model);

            return true;
        }

        /// <summary>
        /// 是否有名称相同
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasNameSame(string name)
        {
            foreach (PlayerModel item in idModelDict.Values)
            {
                if (item.Name.Equals(name))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 获取角色Id
        /// </summary>
        /// <param name="accId"></param>
        /// <returns></returns>
        public int GetId(int accId)
        {
            if (accPlayerDict.ContainsKey(accId))
            {
                return accPlayerDict[accId];
            }

            PlayerModel model = new PlayerModel();
            if (!model.ExistsByAccId(accId))
            {
                return -1;
            }

            //如果数据库存在
            model.GetModelByAccId(accId);
            //添加到内存
            accPlayerDict.TryAdd(model.AccountId, model.Id);
            idModelDict.TryAdd(model.Id, model);
            return model.Id;
        }
        /// <summary>
        /// 获取在线角色Id
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public int GetId(MobaClient client)
        {
            int retId = -1;
            clientIdDict.TryGetValue(client, out retId);
            return retId;
        }
        /// <summary>
        /// 获得角色数据
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public PlayerModel GetModel(int playerId)
        {
            if (idModelDict.ContainsKey(playerId))
            {
                return idModelDict[playerId];
            }

            PlayerModel model = new PlayerModel();
            if (!model.Exists(playerId))
            {
                return null;
            }

            //如果数据库存在
            model.GetModel(playerId);
            //添加到内存
            accPlayerDict.TryAdd(model.AccountId, model.Id);
            idModelDict.TryAdd(model.Id, model);
            return model;
        }
        /// <summary>
        /// 获得角色数据
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public PlayerModel GetModel(string name)
        {
            foreach (PlayerModel iteModel in idModelDict.Values)
            {
                if (iteModel.Name.Equals(name))
                {
                   return iteModel; 
                }
            }

            PlayerModel model = new PlayerModel();
            if (!model.ExistsByName(name))
            {
                return null;
            }

            //如果数据库存在
            model.GetModelByName(name);
            //添加到内存
            accPlayerDict.TryAdd(model.AccountId, model.Id);
            idModelDict.TryAdd(model.Id, model);
            return model;
        }

        /// <summary>
        /// 获得角色数据
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public PlayerModel GetModel(MobaClient client)
        {
            int playerId = -1;
            if (clientIdDict.TryGetValue(client, out playerId))
            {
                return idModelDict[playerId];
            }
            return null;
        }
        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="playerID"></param>
        public void AddFriend(int playerId,int friendId)
        {
            PlayerModel model = idModelDict[playerId];
            if (model.FriendIdList.Equals(""))
            {
                model.FriendIdList += friendId.ToString();
            }
            else
            {
                model.FriendIdList +=',' + friendId.ToString();
            }
            model.Update();


            PlayerModel model2 = idModelDict[friendId];
            if (model2.FriendIdList.Equals(""))
            {
                model2.FriendIdList += playerId.ToString();
            }
            else
            {
                model2.FriendIdList += ',' + playerId.ToString();
            }
            model2.Update();
        }
        /// <summary>
        /// 更新模型数据
        /// </summary>
        /// <param name="playerModel"></param>
        /// <param name="result">0胜利 1失败 2逃跑</param>
        public void UpdateModel(PlayerModel playerModel,int result)
        {
            switch (result)
            {
                case 0: // 胜利
                    playerModel.WinCount++;
                    playerModel.Exp += 100;
                    playerModel.Power += 100;
                    break;
                case 1://失败
                    playerModel.LoseCount++;
                    playerModel.Exp += 20;
                    playerModel.Power -= 100;
                    break;
                case 2://逃跑
                    playerModel.RunCount++;
                    playerModel.Power -= 200;
                    break;
                default:
                    break;

            }
            //升级
            if (playerModel.Exp >= playerModel.Lv * 100)
            {
                playerModel.Lv++;
                playerModel.Exp = 0;
            }

            //保存最新model
            idModelDict[playerModel.Id] = playerModel;

            playerModel.Update();
        }

        #endregion

        #region 在线
        private SynchronizedDictionary<MobaClient, int> clientIdDict = new SynchronizedDictionary<MobaClient, int>();
        private SynchronizedDictionary<int, MobaClient> IdClientDict = new SynchronizedDictionary<int, MobaClient>();
        /// <summary>
        /// 上线
        /// </summary>
        /// <param name="client"></param>
        /// <param name="accId"></param>
        public void OnLine(MobaClient client,int PlayerId)
        {
            clientIdDict.TryAdd(client, PlayerId);
            IdClientDict.TryAdd(PlayerId, client);
        }

        /// <summary>
        /// 下线
        /// </summary>
        /// <param name="client"></param>
        /// <param name="accId"></param>
        public void OffLine(MobaClient client)
        {
            if (!clientIdDict.ContainsKey(client))
            {
                return;
            }

            int id = clientIdDict[client];
            if (clientIdDict.ContainsKey(client))
            {
                clientIdDict.Remove(client);
            }
            if (IdClientDict.ContainsKey(id))
            {
                IdClientDict.Remove(id);
            }
        }
        /// <summary>
        /// 角色是否在线
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool IsOnLine(MobaClient client)
        {
            return clientIdDict.ContainsKey(client);
        }

        /// <summary>
        /// 角色是否在线
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool IsOnLine(int playerId)
        {
            return IdClientDict.ContainsKey(playerId);
        }

        /// <summary>
        /// 获取对应玩家的连接对象
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public MobaClient GetClient(int playerID)
        {
            return IdClientDict[playerID];
        }

        #endregion

    }
}
