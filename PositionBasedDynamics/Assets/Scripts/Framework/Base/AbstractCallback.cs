using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework
{
    public abstract class AbstractCallback : IPoolObject
    {
        public abstract Delegate Handler
        {
            get;
            set;
        }

        public void OnRecycle()
        {
        }

        public abstract void Run();
    }

    public class Callback : AbstractCallback
    {
        private Action mAction;

        public override Delegate Handler
        {
            get { return mAction; }
            set { mAction = value as Action; }
        }

        public override void Run()
        {
            mAction();
        }
    }

    public class Callback<T> : AbstractCallback
    {
        private Action<T> mAction;

        public T Arg1 { get; set; }

        public override Delegate Handler
        {
            get { return mAction; }
            set { mAction = value as Action<T>; }
        }

        public override void Run()
        {
            mAction(Arg1);
        }
    }

    public class Callback<T1, T2> : AbstractCallback
    {
        private Action<T1, T2> mAction;

        public T1 Arg1 { get; set; }

        public T2 Arg2 { get; set; }

        public override Delegate Handler
        {
            get { return mAction; }
            set { mAction = value as Action<T1, T2>; }
        }

        public override void Run()
        {
            mAction(Arg1, Arg2);
        }
    }

    public class Callback<T1, T2, T3> : AbstractCallback
    {
        private Action<T1, T2, T3> mAction;

        public T1 Arg1 { get; set; }

        public T2 Arg2 { get; set; }

        public T3 Arg3 { get; set; }

        public override Delegate Handler
        {
            get { return mAction; }
            set { mAction = value as Action<T1, T2, T3>; }
        }

        public override void Run()
        {
            mAction(Arg1, Arg2, Arg3);
        }
    }
    public class Callback<T1, T2, T3, T4> : AbstractCallback
    {
        private Action<T1, T2, T3, T4> mAction;

        public T1 Arg1 { get; set; }

        public T2 Arg2 { get; set; }

        public T3 Arg3 { get; set; }

        public T4 Arg4 { get; set; }

        public override Delegate Handler
        {
            get { return mAction; }
            set { mAction = value as Action<T1, T2, T3, T4>; }
        }

        public override void Run()
        {
            mAction(Arg1, Arg2, Arg3, Arg4);
        }
    }

    public class Callback<T1, T2, T3, T4, T5> : AbstractCallback
    {
        private Action<T1, T2, T3, T4, T5> mAction;

        public T1 Arg1 { get; set; }

        public T2 Arg2 { get; set; }

        public T3 Arg3 { get; set; }

        public T4 Arg4 { get; set; }

        public T5 Arg5 { get; set; }

        public override Delegate Handler
        {
            get { return mAction; }
            set { mAction = value as Action<T1, T2, T3, T4, T5>; }
        }

        public override void Run()
        {
            mAction(Arg1, Arg2, Arg3, Arg4, Arg5);
        }
    }
}
