using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework
{
    public sealed class EventInstance
    {
        internal const int INVALID_INDEX = -2;
        internal const int BROADCAST_INDEX = -1;
        public static readonly EventInstance INVALID = new EventInstance(INVALID_INDEX, null);
        public static readonly EventInstance BROADCAST = new EventInstance(BROADCAST_INDEX, null);

        /// <summary>
        /// 索引，在实例集合中的位置索引
        /// </summary>
        internal int Index;
        /// <summary>
        /// 实际对象
        /// </summary>
        internal object Obj;
        /// <summary>
        /// 注册过的事件列表，方便注销使用
        /// </summary>
        internal readonly List<uint> Events;

        public EventInstance()
        {
            Index = -1;
            Obj = null;
            Events = new List<uint>();
        }

        public EventInstance(int index, object obj)
        {
            Index = index;
            Obj = obj;
        }
    }
}
