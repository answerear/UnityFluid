using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework
{
    public class TimerEvent : Event
    {
        public override uint ID
        {
            get { return (uint)EVENT_ID.TIMER; }
            protected set { }
        }

        public uint TimerID { get; set; }
        public long Elapse { get; set; }
        public AbstractCallback Callback { get; set; }

        public TimerEvent()
        {
            TimerID = TimerManager.INVALID_TIMER_ID;
            Elapse = 0;
            Callback = null;
        }

        ~TimerEvent()
        {

        }
    }
}
