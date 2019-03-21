using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;
using LitJson;
using MobaCommon.Code;
using MobaCommon.Config;
using MobaCommon.Dto;
using MobaCommon.Dto.Skill;
using Photon.SocketServer;
using MOBAServer.Cache;
using MOBAServer.Model;
using MOBAServer.Room;

namespace MOBAServer.Logic
{  
    public class FightHandler :SingleSend,IOpHandler
    {
        #region 缓存层

        public FightCache fightCache
        {
            get { return Caches.Fight; }
        }

        public PlayerCache playerCache
        {
            get { return Caches.Player; }
        }

        #endregion

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="team1"></param>
        /// <param name="team2"></param>
        public void StartSelect(List<SelectModel> team1, List<SelectModel> team2)
        {
            //创建一个战斗房间
            fightCache.CreatRoom(team1,team2);
        }
        public void OnDisconnect(MobaClient client)
        {
            fightCache.OffLine(client,playerCache.GetId(client));
        }

        public void OnRequest(MobaClient client, byte subCode, OperationRequest request)
        {
            switch (subCode)
            {
                case OpFight.Enter:
                    OnEnter(client, (int) request[0]);
                    break;
                case OpFight.Walk:
                    OnWalk(client, (float)request[0], (float)request[1], (float)request[2]);
                    break;
                case OpFight.Skill:
                    OnSkill(client, (int)request[0], (int)request[1], (int)request[2], (float)request[3], (float)request[4], (float)request[5]);
                    break;
                case OpFight.Damage:
                    OnDamage(client, (int)request[0],(int)request[1], (int[])request[2]);
                    break;
                case OpFight.Buy:
                    OnBuy(client, (int)request[0]);
                    break;
                case OpFight.Sale:
                    OnSale(client, (int)request[0]);
                    break;
                case OpFight.SkillUp:
                    OnSkillUp(client, (int)request[0]);
                    break;
                default:
                    break;
            }
        }

        #region 功能函数
        /// <summary>
        /// 技能升级
        /// </summary>
        /// <param name="client"></param>
        /// <param name="SkillId"></param>
        private void OnSkillUp(MobaClient client,int SkillId)
        { 
            //1、获取房间
            int playerId = playerCache.GetId(client);
            FightRoom room = fightCache.GetRoom(playerId, client);
            if (room == null)
            {
                return;
            }
            //获取英雄模型
            HeroModel hero = room.GetHeroModel(playerId);
            if (hero == null)
            {
                return;
            }

            if (hero.SkillPoints <= 0)
            {
                return;
            }

            //可以加点

            for (int i = 0; i < hero.Skills.Length; i++)
            {
                SkillModel skill = hero.Skills[i];
                if (skill.Id != SkillId)
                {
                    continue;
                }
                 //玩家等级没有到达技能学习要求  或者技能已满级
                if (skill.LearnLevel > hero.Level || skill.LearnLevel == -1)
                {
                    return;
                }

                //先获取技能下一级的数据
                hero.SkillPoints--;
                skill.Level++;
                SkillLevelDataModel data = SkillData.GetSkllData(SkillId).LvModels[skill.Level];
                //修改技能
                skill.LearnLevel = data.LearnLv;
                skill.Distance = data.Distance;
                skill.CoolDown = data.CoolDown;

                 //广播谁更新了什么技能
                 room.Brocast(OpCode.FightCode,OpFight.SkillUp,0,"有人点技能了",null,playerId,JsonMapper.ToJson(skill));
                return;
            }

        }

        /// <summary>
         /// 卖装备
         /// </summary>
         /// <param name="client"></param>
         /// <param name="itemId"></param>
        private void OnSale(MobaClient client,int itemId)
        {
            ItemModel model = ItemData.GetItem(itemId);
            if (model == null)
            {
                return;
            }
            //1、获取房间
            int playerId = playerCache.GetId(client);
            FightRoom room = fightCache.GetRoom(playerId, client);
            if (room == null)
            {
                return;
            }
            //获取英雄模型
            HeroModel hero = room.GetHeroModel(playerId);

            //添加装备
            for (int i = 0; i < hero.Equipments.Length; i++)
            {
                // -1代表没有装备
                if (hero.Equipments[i] == itemId)
                {
                    hero.Money += model.Price;
                    //装备Id
                    hero.Equipments[i] = -1;
                    hero.ReduceItem(model);

                    //给房间内所有客户端发消息，谁了什么装备  发送heroModel;
                    room.Brocast(OpCode.FightCode, OpFight.Sale, 0, "有人卖装备了", null, JsonMapper.ToJson(hero));
                    return;
                }
            }

            //代表出售失败了
            Send(client,OpCode.FightCode,OpFight.Sale,-1,"出售失败");
            return;
        }

        /// <summary>
         /// 响应购买请求
         /// </summary>
         /// <param name="client"></param>
         /// <param name="buyId"></param>
        private void OnBuy(MobaClient client, int buyId)
         {
             ItemModel model = ItemData.GetItem(buyId);
             if (model == null)
             {
                 return;
             }
            //1、获取房间
            int playerId = playerCache.GetId(client);
            FightRoom room = fightCache.GetRoom(playerId, client);
             if (room == null)
             {
                 return;
             }
             //获取英雄模型
             HeroModel hero = room.GetHeroModel(playerId);
             if (hero.Money < model.Price)
             {
                Send(client,OpCode.FightCode,OpFight.Buy,-1,"金币不足");
                 return;
             }


              //开始购买装备
             hero.Money -= model.Price;
            //添加装备
             for (int i = 0; i < hero.Equipments.Length; i++)
             {
                // -1代表没有装备
                 if (hero.Equipments[i] == -1)
                 {
                    //装备Id
                     hero.Equipments[i] = model.Id;
                     hero.AddItem(model);

                    //给房间内所有客户端发消息，谁了什么装备  发送heroModel;
                    room.Brocast(OpCode.FightCode, OpFight.Buy, 0, "有人购买了装备", null, JsonMapper.ToJson(hero));
                    return;
                 }
             }

            //有没有格子
            if (hero.Equipments.Length == 6)
            {
                Send(client, OpCode.FightCode, OpFight.Buy, -2, "装备已满");
                return;
            }
        }

        /// <summary>
        /// 响应伤害请求
        /// </summary>
        /// <param name="request"></param>
        private void OnDamage(MobaClient client,int attackId,int skillId,int[] targetIds)
        {
            //1、获取房间
            int playerId = playerCache.GetId(client);
            FightRoom room = fightCache.GetRoom(playerId, client);
            //2、谁攻击谁 
            DogModel attackModel = null;
            if (attackId >= 0)
            {
                //英雄攻击
                attackModel = room.GetHeroModel(attackId);
            }
            else if (attackId <= -100 && attackId >-300 )
            {
                //防御塔的攻击
                attackModel = room.GetBuildModel(attackId);
            }
            else if ( attackId <= -1000)
            {
                //小兵的攻击
            }

            DogModel[] targetModels = new DogModel[targetIds.Length];
            for (int i = 0; i < targetIds.Length; i++)
            {
                int targetId = targetIds[i];
                if (targetId >= 0)
                {
                    //英雄攻击
                    targetModels[i] = room.GetHeroModel(targetId);
                }
                else if (targetId <= -100 && targetId > -300)
                {
                    //防御塔的攻击
                    targetModels[i] = room.GetBuildModel(targetId);
                }
                else if (targetId <= -1000)
                {
                    //小兵的攻击
                    targetModels[i] = room.GetDogModel(targetId);
                }
            }

            //3、根据技能id 普通攻击 特殊技能 
            ISkill skill = null;
            List<DamageModel> damages = null;
            if (skillId == 1)
            {
                //普通攻击
                skill = DamageData.GetSkill(skillId);
                //4、根据技能id获取damage计算伤害 
                damages = skill.Damage(skillId, 0, attackModel, targetModels);
                //5、检测死亡 给钱 给经验
            }
            else
            {
                //特殊技能 
                skill = DamageData.GetSkill(skillId);
                //4、根据技能id获取damage计算伤害 
                HeroModel hero = attackModel as HeroModel;
                SkillModel skillModel = hero.GetSkillModel(skillId);
                damages = skill.Damage(skillId, skillModel.Level, attackModel, targetModels);
                //5、检测死亡 给钱 给经验
            }

            //6、给房间内的客户端广播数据模型
            room.Brocast(OpCode.FightCode,OpFight.Damage,0,"有伤害产生",null,JsonMapper.ToJson(damages.ToArray()));

            //结算
            foreach (DogModel item in targetModels)
            {
                if (item.CurrHp == 0)
                {
                    switch (item.ModelType)
                    {
                        case ModelType.HERO:

                            #region 英雄 
                            if (attackModel.Id >= 0)
                            {
                                ((HeroModel) attackModel).Kill++;
                                ((HeroModel)attackModel).AddProprity(300, 50);
                                if (((HeroModel)attackModel).Exp > ((HeroModel)attackModel).Level * 100)
                                {
                                    ((HeroModel)attackModel).Level++;
                                    ((HeroModel)attackModel).SkillPoints++;
                                    ((HeroModel)attackModel).Exp = 0;

                                    HeroDataModel data = HeroData.GetHeroData(attackModel.Id);
                                    ((HeroModel)attackModel).Attack += data.GrowAttack;
                                    ((HeroModel)attackModel).Defense += data.GrowDefens;
                                    ((HeroModel)attackModel).MaxHp += data.GrowHp;
                                    ((HeroModel)attackModel).MaxMp += data.GrowMp;
                                }
                                //给客户端发送数据模型 attackModel
                                room.Brocast(OpCode.FightCode, OpFight.UpdateModel, 0, "更新数据模型", null, JsonMapper.ToJson(attackModel as HeroModel));
                            }

                            //目标英雄死亡
                            ((HeroModel)item).Dead++;
                            //开启定时任务 复活
                            room.StartSchedule(DateTime.UtcNow.AddSeconds(
                                (double)((HeroModel)item).Level * 5),
                                () =>
                                {
                                    ((HeroModel)item).CurrHp = ((HeroModel)item).MaxHp;
                                    //给客户端发送复活消息
                                    room.Brocast(OpCode.FightCode, OpFight.Resurge, 0, "有模型复活了", null, JsonMapper.ToJson(item as HeroModel));
                                });
                            #endregion
                            break;
                        case ModelType.BUILD:
                            #region 建筑
                            //判断是否英雄击杀
                            if (attackModel.Id >= 0)
                            {
                                ((HeroModel)attackModel).AddProprity(150, 0);
                                //给客户端发送数据模型 attackModel
                                room.Brocast(OpCode.FightCode, OpFight.UpdateModel, 0, "更新数据模型", null, JsonMapper.ToJson(attackModel as HeroModel));
                            }
                            //防御塔是否可以重生
                            if (((BuildModel) item).ReBirth)
                            {
                                //开启定时任务 复活塔
                                room.StartSchedule(DateTime.UtcNow.AddSeconds(
                                    (double) (((BuildModel) item).ReBirthTime)), 
                                    () =>
                                    {
                                        ((BuildModel) item).CurrHp = ((BuildModel) item).MaxHp;
                                        //给客户端发送复活消息
                                        room.Brocast(OpCode.FightCode, OpFight.Resurge, 1, "有模型复活了", null, JsonMapper.ToJson(item as BuildModel));
                                    });
                            }
                            else
                            {
                                room.RemoveBuild((BuildModel)item);
                            }
                            //游戏结束判断
                            if (item.Id == -100)
                            {
                                //队伍2赢了
                                OnGameOver(room, 2);
                            }
                            else if (item.Id == -200)
                            {
                                //队伍1赢了
                                OnGameOver(room, 1);
                            }
                            #endregion
                            break;
                        case ModelType.DOG:
                            #region 小兵
                            //英雄攻击
                            if (attackModel.Id >= 0)
                            {
                                ((HeroModel)attackModel).AddProprity(20,10);
                                if (((HeroModel)attackModel).Exp > ((HeroModel)attackModel).Level * 100)
                                {
                                    ((HeroModel) attackModel).Level++;
                                    ((HeroModel) attackModel).SkillPoints++;
                                    ((HeroModel) attackModel).Exp = 0;

                                    HeroDataModel data = HeroData.GetHeroData(attackModel.Id);
                                    ((HeroModel) attackModel).Attack += data.GrowAttack;
                                    ((HeroModel)attackModel).Defense += data.GrowDefens;
                                    ((HeroModel)attackModel).MaxHp += data.GrowHp;
                                    ((HeroModel)attackModel).MaxMp += data.GrowMp;
                                }
                                //给客户端发送数据模型 attackModel
                                room.Brocast(OpCode.FightCode,OpFight.UpdateModel,0,"更新数据模型",null,JsonMapper.ToJson(attackModel as HeroModel));
                            }
                             //移除小兵
                            room.RemoveDog(item);
                            #endregion
                            break;
                        default:
                            break;
                    }
                }

            }
        }
        /// <summary>
        /// 处理游戏结束
        /// </summary>
        private void OnGameOver(FightRoom room,int winTeam)
        {
            room.Brocast(OpCode.FightCode, OpFight.GameOver, 0, "游戏结束", null, winTeam);

            //更新玩家的数据
            foreach (MobaClient client in room.clientList)
            {
                //获取玩家数据模型
                PlayerModel playerModel = playerCache.GetModel(client);
                //检测是否逃跑
                if (room.leaveClient.Contains(client))
                {
                    //更新逃跑场次
                    playerCache.UpdateModel(playerModel,2);
                }
                //胜利失败判断
                HeroModel heroModel = room.GetHeroModel(playerModel.Id);
                if (heroModel.Team == winTeam)
                {
                    //赢了
                    playerCache.UpdateModel(playerModel, 0);
                }
                else
                {
                    //输了
                    playerCache.UpdateModel(playerModel, 1);
                }
            }
            //销毁战斗房间
            fightCache.Destroy(room.Id);
        }

        /// <summary>
        /// 使用技能
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="targetId"></param>
        private void OnSkill(MobaClient client, int skillId,int attackId, int targetId,float x,float y,float z)
        {
            int playerId = playerCache.GetId(client);
            FightRoom room = fightCache.GetRoom(playerId, client);
            //先判断是不是普通攻击
            if (skillId == 1)
            {
                //参数 1使用者，2目标者
                room.Brocast(OpCode.FightCode,OpFight.Skill,0,"有人进行普通攻击",null, attackId, targetId);
            }
            //从技能配置获取技能信息,在广播
            else
            {
                if (targetId == -1)
                {
                    //定点技能
                    room.Brocast(OpCode.FightCode, OpFight.Skill, 1, "定点释放技能", null,skillId, attackId,-1, x,y,z);
                }
                else
                {
                    //指定目标技能
                    room.Brocast(OpCode.FightCode, OpFight.Skill, 1, "选择目标释放技能", null,skillId, attackId, targetId);
                }
            }
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="client"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void OnWalk(MobaClient client, float x, float y, float z)
        {
            int playerId = playerCache.GetId(client);
            FightRoom room = fightCache.GetRoom(playerId, client);
            if (room == null)
            {
                return;
            }
            //给每个客户端发送信息，谁移动到哪
            room.Brocast(OpCode.FightCode,OpFight.Walk,0,"有玩家移动",null,playerId,x,y,z);
        }

        /// <summary>
        /// 玩家进入战斗
        /// </summary>
        /// <param name="playerId"></param>
        private void OnEnter(MobaClient client,int playerId)
        {
            FightRoom room = fightCache.Enter(playerId, client);
            if (room == null)
            {
                return;
            }

            //是否全部进入，保证竞技游戏的公平
            if (!room.IsAllEnter)
            {
                return;
            }

            //给每个客户端发送进入战斗信息
            string Builds = JsonMapper.ToJson(room.Builds);
            string Heros = JsonMapper.ToJson(room.Heros);
            room.Brocast(OpCode.FightCode,OpFight.GetInfo,0,"进入战斗",null,
                Heros, Builds);
        }


        #endregion
    }
}
