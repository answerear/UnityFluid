using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework
{
    internal class TimerTask : IPoolObject
    {
        public uint TimerID { get; set; }
        public uint Interval { get; set; }
        public Int64 Expired { get; set; }
        public bool Loop { get; set; }
        public bool Alive { get; set; }
        public AbstractCallback Callback { get; set; }

        public void OnRecycle()
        {
            if (Callback != null)
            {
                ObjectsPools.Instance.ReleaseObject(Callback);
                Callback = null;
            }

            TimerID = TimerManager.INVALID_TIMER_ID;
            Interval = 0;
            Expired = 0;
            Loop = false;
        }
    }
}
