using System;
using System.Collections.Generic;
using MobaCommon.Dto.Skill;

namespace MobaCommon.Config
{
    /// <summary>
    /// 伤害的数据
    /// </summary>
    public class DamageData
    {
        /// <summary>
        /// 技能Id和技能的映射关系
        /// </summary>
        static Dictionary<int,ISkill> idSkillDict = new Dictionary<int, ISkill>();

        static DamageData()
        {
            //添加普通攻击映射
            idSkillDict.Add(1,new AttackSkill());
            idSkillDict.Add(1003,new LineSkill());
        }
        /// <summary>
        /// 根据id获取技能对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ISkill GetSkill(int id)
        {
            if (!idSkillDict.ContainsKey(id))
            {
                return null;
            }
            return idSkillDict[id];
        }
    }
}
