using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobaCommon.Dto;

namespace MobaCommon.Config
{
    /// <summary>
    /// 英雄数据
    /// </summary>
    public class DogData
    {
        static Dictionary<int, DogDataModel> idModelDict = new Dictionary<int, DogDataModel>();

        static DogData()
        {
            //1 英雄的id范围
            CreateDog(1, 300, 50, 10, 4, "小狗");
        }

        public static DogDataModel GetDogData(int dogTypeId)
        {
            DogDataModel dataModel = null;
            idModelDict.TryGetValue(dogTypeId, out dataModel);
            return dataModel;
        }


        /// <summary>
        /// 创建Dog
        /// </summary>
        /// <param name="id"></param>
        /// <param name="typeId"></param>
        /// <param name="team"></param>
        /// <param name="maxHp"></param>
        /// <param name="attack"></param>
        /// <param name="defense"></param>
        /// <param name="attackDistance"></param>
        /// <param name="name"></param>
        private static void CreateDog(int typeId, int maxHp, int attack, int defense, double attackDistance, string name)
        {
            DogDataModel dogData = new DogDataModel(typeId, maxHp, attack, defense, attackDistance, name);
            //保存英雄数据
            idModelDict.Add(dogData.TypeId, dogData);
        }
    }
    /// <summary>
    /// Dog数据模型
    /// </summary>
    public class DogDataModel
    {
        /// <summary>
        /// 类型
        /// </summary>
        public int TypeId { get; set; }
        /// <summary>
        /// 模型类型
        /// </summary>
        public ModelType ModelType { get; set; }
        /// <summary>
        /// 当前血量
        /// </summary>
        public int CurrHp { get; set; }
        /// <summary>
        /// 最大血量
        /// </summary>
        public int MaxHp { get; set; }
        /// <summary>
        /// 攻击力
        /// </summary>
        public int Attack { get; set; }
        /// <summary>
        /// 防御力
        /// </summary>
        public int Defense { get; set; }
        /// <summary>
        /// 攻击距离
        /// </summary>
        public double AttackDistance { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        public DogDataModel()
        {
        }

        public DogDataModel(int typeId, int maxHp, int attack, int defense, double attackDistance, string name)
        {
            this.TypeId = typeId;
            this.CurrHp = maxHp;
            this.MaxHp = maxHp;
            this.Attack = attack;
            this.Defense = defense;
            this.AttackDistance = attackDistance;
            this.Name = name;
        }
    }
}
