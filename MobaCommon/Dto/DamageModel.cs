using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobaCommon.Dto
{
    /// <summary>
    /// 伤害数据模型
    /// </summary>
    public class DamageModel
    {
        /// <summary>
        /// 使用者Id
        /// </summary>
        public int fromId;
        /// <summary>
        /// 被攻击者Id
        /// </summary>
        public int toId;
        /// <summary>
        /// 技能Id
        /// </summary>
        public int skillId;
        /// <summary>
        /// 伤害
        /// </summary>
        public int damage;
        /// <summary>
        /// 是否死亡
        /// </summary>
        public bool isDead;

        public DamageModel()
        {
        }

        public DamageModel(int skillId,int fromId,int toId,int damage,bool isDead)
        {
            this.skillId = skillId;
            this.fromId = fromId;
            this.toId = toId;
            this.damage = damage;
            this.isDead = isDead;
        }
    }
}
