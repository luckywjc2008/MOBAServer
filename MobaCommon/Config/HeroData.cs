using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobaCommon.Config
{ 
    /// <summary>
    /// 英雄数据
    /// </summary>
    public class HeroData
    {
       static Dictionary<int, HeroDataModel> idModelDict = new Dictionary<int, HeroDataModel>();

        static HeroData()
        {
           //1 英雄的id范围
            CreateHero(1, "战士", 60, 20, 300, 100, 10, 3, 50, 10, 4, new[] {1001, 1002, 1003, 1004});
            CreateHero(2, "弓箭手", 50, 10, 200, 80, 15, 2, 30, 5, 10, new[] { 2001, 2002, 2003, 2004 });
        }

        public static HeroDataModel GetHeroData(int heroId)
        {
            HeroDataModel dataModel = null;
            idModelDict.TryGetValue(heroId, out dataModel);
            return dataModel;
        }


        /// <summary>
        /// 创建英雄
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="baseAttack"></param>
        /// <param name="baseDefens"></param>
        /// <param name="hp"></param>
        /// <param name="mp"></param>
        /// <param name="growAttack"></param>
        /// <param name="growDefens"></param>
        /// <param name="growHp"></param>
        /// <param name="growMp"></param>
        /// <param name="attackDistance"></param>
        /// <param name="skillIds"></param>
        /// <returns></returns>
        private static void CreateHero(int id, string name, int baseAttack, int baseDefens, int hp, int mp, int growAttack, int growDefens, int growHp, int growMp, double attackDistance, int[] skillIds)
        {
            HeroDataModel heroData = new HeroDataModel(id, name, baseAttack, baseDefens, hp, mp, growAttack, growDefens, growHp, growMp, attackDistance, skillIds);
            //保存英雄数据
            idModelDict.Add(heroData.TypeId, heroData); 
        }
    }
    /// <summary>
    /// 英雄数据模型
    /// </summary>
    public class HeroDataModel
    {
        /// <summary>
        /// 英雄编号
        /// </summary>
        public int TypeId;
        /// <summary>
        /// 英雄名字
        /// </summary>
        public string Name;
        /// <summary>
        /// 基础攻击力
        /// </summary>
        public int BaseAttack;
        /// <summary>
        /// 基础防御力
        /// </summary>
        public int BaseDefens;
        /// <summary>
        /// 成长攻击力
        /// </summary>
        public int GrowAttack;
        /// <summary>
        /// 成长防御力
        /// </summary>
        public int GrowDefens;
        /// <summary>
        /// 生命值
        /// </summary>
        public int Hp;
        /// <summary>
        /// 成长生命值
        /// </summary>
        public int GrowHp;
        /// <summary>
        /// 魔法值
        /// </summary>
        public int Mp;
        /// <summary>
        /// 成长魔法值
        /// </summary>
        public int GrowMp;
        /// <summary>
        /// 攻击距离
        /// </summary>
        public double AttackDistance;
         /// <summary>
         /// 技能Id
         /// </summary>
        public int[] SkillIds;

        public HeroDataModel()
        {
        }

        public HeroDataModel(int typeId,string name,int baseAttack,int baseDefens,int hp,int mp,int growAttack,int growDefens,int growHp,int growMp, double attackDistance,int[] skillIds)
        {
            this.TypeId = typeId;
            this.Name = name;
            this.BaseAttack = baseAttack;
            this.BaseDefens = baseDefens;
            this.Hp = hp;
            this.Mp = mp;
            this.GrowAttack = growAttack;
            this.GrowDefens = growDefens;
            this.GrowHp = growHp;
            this.GrowMp = growMp;
            this.AttackDistance = attackDistance;
            this.SkillIds = skillIds;
        }
    }
}
