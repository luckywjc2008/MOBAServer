using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobaCommon.Dto.Skill
{
    public class LineSkill : ISkill
    {
        public List<DamageModel> Damage(int skillId, int level, DogModel from, params DogModel[] to)
        {
            List<DamageModel> list = new List<DamageModel>();
            //攻击者攻击力
            int attack = from.Attack;
            //循环计算伤害
            foreach (DogModel item in to)
            {
                //被攻击者防御力
                int defense = item.Defense;
                //计算伤害
                int damage = (int)(attack*1.5 - defense);
                //掉血
                item.CurrHp -= damage;
                //保证血大于等于0
                if (item.CurrHp <= 0)
                {
                    item.CurrHp = 0;
                }
                list.Add(new DamageModel(skillId, from.Id, item.Id, damage, item.CurrHp == 0));
            }

            return list;
        }
    }
}
