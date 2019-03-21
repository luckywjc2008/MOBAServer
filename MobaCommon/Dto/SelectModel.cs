using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobaCommon.Dto
{
    /// <summary>
    /// 选人数据模型
    /// </summary>
    public class SelectModel
    {
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int playerId;
        /// <summary>
        /// 选择英雄Id
        /// </summary>
        public int heroId;
        /// <summary>
        /// 玩家名字
        /// </summary>
        public string playerName;
        /// <summary>
        /// 是否进入
        /// </summary>
        public bool isEnter;
        /// <summary>
        /// 是否准备
        /// </summary>
        public bool isReady;

        public SelectModel()
        {
            this.heroId = -1;
            this.isEnter = false;
            this.isReady = false;
        }

        public SelectModel(int playerId, string name)
        {
            this.playerId = playerId;
            this.playerName = name;
            this.heroId = -1;
            this.isEnter = false;
            this.isReady = false;
        }

    }
}
