using System;
using System.Collections.Generic;


namespace MobaCommon.Config
{
    /// <summary>
    /// 建筑的类型
    /// </summary>
    public enum BuildType
    {
        //基地
        Main = 1,
        //兵营
        Camp = 2,
        //炮塔
        Turret = 3,
    }

    /// <summary>
    /// 建筑的数据
    /// </summary>
    public class BuildData
    {
        /// <summary>
        ///  类型和模型的映射
        /// </summary>
        static Dictionary<int,BuildDataModel> idBuildDataDict = new Dictionary<int, BuildDataModel>();

        static BuildData()
        {
            creatBuild((int)BuildType.Main,5000,-1,100,-1,"主基地",false,false,-1);
            creatBuild((int)BuildType.Camp, 3000, -1, 100, -1, "兵营", false, true, 300);
            creatBuild((int)BuildType.Turret, 5000, 200, 50, 15, "炮塔", true, false, -1);
        }

        private static void creatBuild(int typeId, int hp, int attack, int defense, double attackDistance, string name, bool agressire, bool rebirth, int rebirthTime)
        {
            BuildDataModel model = new BuildDataModel(typeId, hp, attack, defense, attackDistance, name, agressire, rebirth, rebirthTime);
            idBuildDataDict.Add(model.TypeId,model);
        }
        /// <summary>
        /// 根据类型获取数据
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static BuildDataModel GetBuildData(int typeId)
        {
            BuildDataModel model;
            idBuildDataDict.TryGetValue(typeId, out model);
            return model;
        }

    }
    /// <summary>
    /// 建筑的数据模型
    /// </summary>
    public class BuildDataModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int TypeId { get; set; }
        /// <summary>
        /// 血量
        /// </summary>
        public int Hp { get; set; }
        /// <summary>
        /// 攻击
        /// </summary>
        public int Attack { get; set; }
        /// <summary>
        /// 防御
        /// </summary>
        public int Defense { get; set; }
        /// <summary>
        /// 攻击距离
        /// </summary>
        public double AttackDistance { get; set; }
        /// <summary>
        /// 是否攻击
        /// </summary>
        public bool Agressire { get; set; }
        /// <summary>
        /// 是否重生
        /// </summary>
        public bool ReBirth { get; set; }
        /// <summary>
        /// 重生时间
        /// </summary>
        public int ReBirthTime { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        public BuildDataModel()
        {
        }

        public BuildDataModel(int typeId,int hp,int attack,int defense,double attackDistance,string name,bool agressire,bool rebirth,int rebirthTime)
        {
            this.TypeId = typeId;
            this.Hp = hp;
            this.Attack = attack;
            this.Defense = defense;
            this.AttackDistance = attackDistance;
            this.Name = name;
            this.Agressire = agressire;
            this.ReBirth = rebirth;
            this.ReBirthTime = rebirthTime;
        }
    }
}
