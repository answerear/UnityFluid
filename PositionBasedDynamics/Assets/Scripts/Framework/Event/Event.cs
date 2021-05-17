using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework
{
    public abstract class Event : EventArgs, IPoolObject
    {
        public abstract uint ID { get; protected set; }

        public void OnRecycle()
        {

        }
    }

    public class Event0 : Event
    {
        public Event0(uint id) { this.id = id; }

        private readonly uint id = 0;
        public override uint ID
        {
            get { return id; }
            protected set { }
        }
    }

    public class Event1<T1> : Event
    {
        public Event1(uint id) { this.id = id; }
        public Event1(uint id, T1 value1) { this.id = id; Value1 = value1; }

        private readonly uint id = 0;
        public override uint ID
        {
            get { return id; }
            protected set { }
        }

        public T1 Value1;
    }

    public class Event2<T1, T2> : Event
    {
        public Event2(uint id) { this.id = id; }
        public Event2(uint id, T1 value1, T2 value2) { this.id = id; Value1 = value1; Value2 = value2; }

        private readonly uint id = 0;
        public override uint ID
        {
            get { return id; }
            protected set { }
        }

        public T1 Value1;
        public T2 Value2;
    }

    public class Event3<T1, T2, T3> : Event
    {
        public Event3(uint id) { this.id = id; }
        public Event3(uint id, T1 value1, T2 value2=default, T3 value3=default) { this.id = id; Value1 = value1; Value2 = value2; Value3 = value3; }

        private readonly uint id = 0;
        public override uint ID
        {
            get { return id; }
            protected set { }
        }

        public T1 Value1;
        public T2 Value2;
        public T3 Value3;
    }

    public class Event4<T1, T2, T3, T4> : Event
    {
        public Event4(uint id) { this.id = id; }
        public Event4(uint id, T1 value1, T2 value2 = default, T3 value3 = default, T4 value4 = default)
        { this.id = id; Value1 = value1; Value2 = value2; Value3 = value3; Value4 = value4; }

        private readonly uint id = 0;
        public override uint ID
        {
            get { return id; }
            protected set { }
        }

        public T1 Value1;
        public T2 Value2;
        public T3 Value3;
        public T4 Value4;
    }
}
