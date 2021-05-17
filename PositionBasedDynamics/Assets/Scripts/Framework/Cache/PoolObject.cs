using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework
{
    internal class PoolObject
    {
        public long RecycleTimestamp { get; private set; }
        public IPoolObject ObjectImpl { get; set; }

        public PoolObject()
        {
            RecycleTimestamp = TimerManager.Instance.GetMillisecondsSinceStartup();
            ObjectImpl = null;
        }

        public void Recycle(long timestamp)
        {
            RecycleTimestamp = timestamp;
        }
    }
}
