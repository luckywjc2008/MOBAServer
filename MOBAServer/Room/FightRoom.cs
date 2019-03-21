using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitJson;
using MobaCommon.Code;
using MobaCommon.Config;
using MobaCommon.Dto;

namespace MOBAServer.Room
{
    public class FightRoom : RoomBase<MobaClient>
    {
        #region 队伍1
        //英雄
        Dictionary<int,HeroModel> team1HeroModel = new Dictionary<int, HeroModel>();
        //小兵
        Dictionary<int ,DogModel> team1DogModel = new Dictionary<int, DogModel>();
        //塔
        Dictionary<int,BuildModel> team1BuildModel = new Dictionary<int, BuildModel>();
        #endregion

        #region 队伍2
        //英雄
        Dictionary<int, HeroModel> team2HeroModel = new Dictionary<int, HeroModel>();
        //小兵
        Dictionary<int, DogModel> team2DogModel = new Dictionary<int, DogModel>();
        //塔
        Dictionary<int, BuildModel> team2BuildModel = new Dictionary<int, BuildModel>();

        #endregion
        /// <summary>
        /// 逃跑的客户端
        /// </summary>
        public List<MobaClient> leaveClient = new List<MobaClient>();

        #region Property
        /// <summary>
        /// 是否全部进入
        /// </summary>
        public bool IsAllEnter
        {
            get { return clientList.Count >= Count; }
        }
        /// <summary>
        /// 房间是否还有人
        /// </summary>
        public bool IsAllLeave
        {
            get { return clientList.Count <= 0; }
        }
        /// <summary>
        /// 建筑
        /// </summary>
        public BuildModel[] Builds
        {
            get
            {
                List<BuildModel> list = new List<BuildModel>();
                list.AddRange(team1BuildModel.Values);
                list.AddRange(team2BuildModel.Values);
                return list.ToArray();
            }
        }
        /// <summary>
        /// 英雄
        /// </summary>
        public HeroModel[] Heros
        {
            get
            {
                List<HeroModel> list = new List<HeroModel>();
                list.AddRange(team1HeroModel.Values);
                list.AddRange(team2HeroModel.Values);
                return list.ToArray();
            }
        } 


        #endregion




        public FightRoom(int id, int count) : base(id, count)
        {

        }
        /// <summary>
        /// 初始化房间
        /// </summary>
        /// <param name="team1"></param>
        /// <param name="team2"></param>
        public void Init(List<SelectModel> team1, List<SelectModel> team2)
        {
            //初始英雄数据
            foreach (SelectModel item in team1)
            {
                team1HeroModel.Add(item.playerId,getHeroModel(item,1));
            }
            foreach (SelectModel item in team2)
            {
                team2HeroModel.Add(item.playerId, getHeroModel(item,2));
            }
            //初始化防御塔数据
            //队伍1 -100
            team1BuildModel.Add(-100, getBuildModel(-100,(int)BuildType.Main,1));
            team1BuildModel.Add(-101, getBuildModel(-101, (int)BuildType.Camp, 1));
            team1BuildModel.Add(-102, getBuildModel(-102, (int)BuildType.Turret, 1));
            //队伍2 -200
            team2BuildModel.Add(-200, getBuildModel(-200,(int)BuildType.Main,2));
            team2BuildModel.Add(-201, getBuildModel(-201, (int)BuildType.Camp, 2));
            team2BuildModel.Add(-202, getBuildModel(-202, (int)BuildType.Turret, 2));
            //开启出兵
            spwanDog();
        }

        private int dogId = -1000;

        private int DogId
        {
            get
            {
                dogId--;
                return dogId;
            }
        }

        /// <summary>
        /// 开启定时任务，30秒刷一波小兵
        /// </summary>
        private void spwanDog()
        {
            this.StartSchedule(DateTime.UtcNow.AddSeconds(30), delegate
            {
                List<DogModel> dogs = new List<DogModel>();
                DogDataModel dataModel = DogData.GetDogData(1);

                for (int i = 0; i < 1; i++)
                {
                    //产生小兵

                    DogModel dog = new DogModel(DogId,dataModel.TypeId,1,dataModel.MaxHp,dataModel.Attack,dataModel.Defense,dataModel.AttackDistance,dataModel.Name);
                    dog.ModelType = ModelType.DOG;
                    team1DogModel.Add(dog.Id, dog);
                    dogs.Add(dog);

                    dog = new DogModel(DogId, dataModel.TypeId, 2, dataModel.MaxHp, dataModel.Attack, dataModel.Defense, dataModel.AttackDistance, dataModel.Name);
                    dog.ModelType = ModelType.DOG;
                    dog.Team = 2;

                    team2DogModel.Add(dog.Id, dog);
                    dogs.Add(dog);
                }
                //给客户端发送现在出兵了dogs
                Brocast(OpCode.FightCode,OpFight.Dog,0,"双方产生小兵",null,JsonMapper.ToJson(dogs.ToArray()));
                
                //无限递归
                spwanDog();
            });
        }

        /// <summary>
        /// 根据英雄Id获得英雄数据
        /// </summary>
        /// <param name="heroId"></param>
        /// <returns></returns>
        private HeroModel getHeroModel(SelectModel model,int team)
        {
            //从静态配置表获取到英雄的数据
            HeroDataModel data = HeroData.GetHeroData(model.heroId);
            //英雄数据创建
            HeroModel hero = new HeroModel(model.playerId, data.TypeId, team, data.Hp, data.BaseAttack, data.BaseDefens,
                data.AttackDistance, data.Name, data.Mp, GetSkillModel(data.SkillIds));
            
            hero.ModelType = ModelType.HERO;
            return hero;
        }
         /// <summary>
         /// 根据技能id获取具体的数据实体
         /// </summary>
         /// <param name="skillIds"></param>
         /// <returns></returns>
        public SkillModel[] GetSkillModel(int[] skillIds)
        {
            SkillModel[] skillModels = new SkillModel[skillIds.Length];

            for (int i = 0; i < skillIds.Length; i++)
            {
                //获取技能数据
                SkillDataModel data = SkillData.GetSkllData(skillIds[i]);
                //初始化的时候就是最低级
                SkillLevelDataModel lvData = data.LvModels[0];
                //给技能模型赋值
                skillModels[i] = new SkillModel()
                {
                    Id = data.Id,
                    Level = 0,
                    LearnLevel = lvData.LearnLv,
                    CoolDown = lvData.CoolDown,
                    Name = data.Name,
                    Description = data.Description,
                    Distance = lvData.Distance,
                };
            }
            return skillModels;
        }

        /// <summary>
        /// 根据防御塔id获得防御塔数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private BuildModel getBuildModel(int id,int typeId,int team)
        {
            //获取配置数据
            BuildDataModel data = BuildData.GetBuildData(typeId);
            BuildModel model = new BuildModel(id,typeId,team,data.Hp,data.Attack,data.Defense,data.AttackDistance,data.Name,data.Agressire,data.ReBirth,data.ReBirthTime);
      
            model.ModelType = ModelType.BUILD;
            return model;
        }
        /// <summary>
        /// 进入房间
        /// </summary>
        public void Enter(MobaClient client)
        {
            EnterRoom(client);
        }
        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="client"></param>
        public void Leave(MobaClient client)
        {
            if (!LeaveRoom(client))
            {
                return;
            }

            if (!leaveClient.Contains(client))
            {
                leaveClient.Add(client);
            }

        }
        /// <summary>
        /// 清空房间数据
        /// </summary>
        public void Clear()
        {
            timer.ClearActions();

            team1HeroModel.Clear();
            team1BuildModel.Clear();
            team1DogModel.Clear();
            team2HeroModel.Clear();
            team2BuildModel.Clear();
            team2DogModel.Clear();

            leaveClient.Clear();

            clientList.Clear();
        }
        /// <summary>
        /// 获取英雄模型
        /// </summary>
        /// <returns></returns>
        public HeroModel GetHeroModel(int Id)
        {
            HeroModel model = null;
            if (team1HeroModel.TryGetValue(Id,out model))
            {
                return model;
            }
            if (team2HeroModel.TryGetValue(Id, out model))
            {
                return model;
            }
            return null;
        }

        /// <summary>
        /// 获取防御塔模型
        /// </summary>
        /// <returns></returns>
        public BuildModel GetBuildModel(int Id)
        {
            BuildModel model = null;
            if (team1BuildModel.TryGetValue(Id, out model))
            {
                return model;
            }
            if (team2BuildModel.TryGetValue(Id, out model))
            {
                return model;
            }
            return null;
        }


        /// <summary>
        /// 获取防御塔模型
        /// </summary>
        /// <returns></returns>
        public DogModel GetDogModel(int Id)
        {
            DogModel model = null;
            if (team1DogModel.TryGetValue(Id, out model))
            {
                return model;
            }
            if (team2DogModel.TryGetValue(Id, out model))
            {
                return model;
            }
            return null;
        }
        /// <summary>
        /// 移除小兵数据模型
        /// </summary>
        /// <param name="dog"></param>
        public void RemoveDog(DogModel dog)
        {
            if (dog.Team == 1)
            {
                team1DogModel.Remove(dog.Id);
            }else if (dog.Team == 2)
            {
                team2DogModel.Remove(dog.Id);
            }
        }

        /// <summary>
        /// 移除建筑数据模型
        /// </summary>
        /// <param name="dog"></param>
        public void RemoveBuild(BuildModel build)
        {
            if (build.Team == 1)
            {
                team1BuildModel.Remove(build.Id);
            }
            else if (build.Team == 2)
            {
                team2BuildModel.Remove(build.Id);
            }
        }
    }
}
