using System;
using System.Collections.Generic;

namespace MobaCommon.Dto.Skill
{
    /// <summary>
    /// 技能
    /// </summary>
    public interface ISkill
    {
        /// <summary>
        /// 计算伤害
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="level"></param>
        /// <returns>返回伤害的数据模型</returns>
        List<DamageModel> Damage(int skillId, int level,DogModel from,params DogModel[] to);

    }
}
