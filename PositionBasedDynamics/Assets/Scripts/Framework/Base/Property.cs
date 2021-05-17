using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework
{
    public class Property<T>
    {
        public T Value
        {
            get
            {
                return mValue;
            }

            set
            {
                if (!value.Equals(mValue))
                {
                    mValue = value;
                    NotifyValueChanged();
                }
            }
        }

        protected T mValue = default(T);
        protected Delegate mCallbacks = null;

        public void AddValueChangedCallback(Action<T> handler)
        {
            mCallbacks = (Action<T>)mCallbacks + handler;
        }

        public void RemoveValueChangedCallback(Action<T> handler)
        {
            mCallbacks = (Action<T>)mCallbacks - handler;
        }

        protected void NotifyValueChanged()
        {
            if (mCallbacks == null)
                return;

            Callback<T> handler = ObjectsPools.Instance.AcquireObject<Callback<T>>();

            var callbacks = mCallbacks.GetInvocationList();
            for (int i = 0; i < callbacks.Length; i++)
            {
                Delegate d = callbacks[i];
                Action<T> callback = d as Action<T>;
                if ((d.Target != null) && (d.Target is UnityEngine.Object) && d.Target.Equals(null))
                {
                    RemoveValueChangedCallback(callback);
                    continue;
                }

                if (callback == null)
                {
                    Log.Warning("Property", "NotifyValueChanged error: types of parameters are not match.");
                    break;
                }

                handler.Handler = callback;
                handler.Arg1 = mValue;
                handler.Run();
            }

            ObjectsPools.Instance.ReleaseObject(handler);
        }
    }

    public class PropertyInt : Property<int>
    {

    }

    public class PropertyLong : Property<long>
    {

    }

    public class PropertyString : Property<string>
    {

    }

    public class PropertyFloat : Property<float>
    {

    }

    public class PropertyBool : Property<bool>
    {

    }
}
