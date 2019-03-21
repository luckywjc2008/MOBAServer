using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using MOBAServer.Room;

namespace MOBAServer.Cache
{
    public class RoomCacheBase<TRoom> where TRoom:RoomBase<MobaClient>
    {
        /// <summary>
        /// 房间Id对应的房间数据
        /// </summary>
        protected ConcurrentDictionary<int, TRoom> idRoomDict = new ConcurrentDictionary<int, TRoom>();

        /// <summary>
        /// 玩家Id对应房间Id
        /// </summary>
        protected ConcurrentDictionary<int, int> playerRoomDict = new ConcurrentDictionary<int, int>();
        /// <summary>
        /// 房间的重用队列
        /// </summary>
        protected ConcurrentQueue<TRoom> roomQue = new ConcurrentQueue<TRoom>();
        /// <summary>
        /// 主键Id
        /// </summary>
        protected int index = 0;
    }
}
