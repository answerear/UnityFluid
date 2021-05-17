using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework
{
    public sealed class EventManager : Singleton<EventManager>
    {
        private class HandlerItem : IPoolObject
        {
            public EventInstance Instance;
            public EventHandler<Event> Handler;

            public HandlerItem()
            {
                Instance = null;
                Handler = null;
            }

            public void OnRecycle()
            {
                Instance = null;
                Handler = null;
            }
        }

        private class EventItem : IPoolObject
        {
            public Event Evt;
            public EventInstance Sender;
            public EventInstance Receiver;

            public EventItem()
            {
                Evt = null;
                Sender = null;
                Receiver = null;
            }

            public void OnRecycle()
            {
                Evt = null;
                Sender = null;
                Receiver = null;
            }
        }

        #region 成员
        private uint mMaxEvents;                        // 事件数量上限
        private Dictionary<int, HandlerItem>[] mHandlers;          // 事件处理函数集合
        private readonly int mDefaultInstanceCapacity;  // 默认事件实例集合大小
        private int mInstancesSize;                     // 事件实例集合大小
        private int mInstancesCapacity;                 // 事件实例集合容量
        private EventInstance[] mInstances;             // 事件实例集合
        private int mCurrentQueue;                      // 当前处理的事件队列索引
        private Queue<EventItem>[] mEventsQueue;        // 待处理事件队列
        #endregion

        #region 属性
        /// <summary>
        /// 事件数量上限，注意：设置后会把之前设置的事件处理函数集合清空
        /// </summary>
        public uint MaxEvents
        {
            get { return mMaxEvents; }
            set
            {
                mMaxEvents = value;
                mHandlers = new Dictionary<int, HandlerItem>[mMaxEvents];
                for (int i = 0; i < mHandlers.Length; ++i)
                {
                    mHandlers[i] = new Dictionary<int, HandlerItem>();
                }
            }
        }
        #endregion

        #region 构造和析构
        /// <summary>
        /// 构造函数
        /// </summary>
        private EventManager()
        {
            mHandlers = null;
            mMaxEvents = 0;
            mDefaultInstanceCapacity = 512;
            mInstancesSize = 0;
            mInstancesCapacity = mDefaultInstanceCapacity;
            mInstances = new EventInstance[mInstancesCapacity];
            mCurrentQueue = 0;
            mEventsQueue = new Queue<EventItem>[2];

            int i = 0;
            for (i = 0; i < mInstances.Length; ++i)
            {
                mInstances[i] = new EventInstance();
            }

            for (i = 0; i < mEventsQueue.Length; ++i)
            {
                mEventsQueue[i] = new Queue<EventItem>();
            }
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~EventManager()
        {

        }
        #endregion

        #region 公有接口
        /// <summary>
        /// 注册对象，只有注册过的对象，才能收发事件
        /// </summary>
        /// <param name="obj">注册的对象</param>
        /// <returns>返回注册对象的事件实例句柄</returns>
        public EventInstance RegisterObject(object obj)
        {
            EventInstance instance = new EventInstance();

            int index = 0;

            if (mInstancesSize + 1 < mInstancesCapacity)
            {
                // 先在实例集合里查找一个空位，存放新来的对象
                int i = 0;
                for (i = 0; i < mInstancesCapacity; ++i)
                {
                    if (mInstances[i].Index == -1)
                    {
                        // 咦？空的，太好了，先占坑！
                        index = i;
                        break;
                    }
                }
            }
            else
            {
                // 集合空间不够，只能重新分配空间了，这就损耗不少性能了！！
                index = ExtendCapacity();
            }

            instance.Index = index;
            instance.Obj = obj;
            mInstances[index] = instance;

            mInstancesSize++;

            return instance;
        }

        /// <summary>
        /// 注销对象，注销后的对象，无法收发事件
        /// </summary>
        /// <param name="instance">要注销的事件实例句柄</param>
        /// <returns>调用成功返回RESULT.OK</returns>
        public RESULT UnregisterObject(EventInstance instance)
        {
            RESULT ret = RESULT.OK;

            do
            {
                if (instance.Index < 0)
                {
                    // 索引错误
                    ret = RESULT.INVALID_PARAM;
                    break;
                }

                // 避免外部没有取消事件订阅，这里兜一下
                UnsubscribeAllEvents(instance);

                int index = instance.Index;
                mInstances[index].Index = -1;
                mInstances[index].Obj = null;
                mInstancesSize--;

                if (mInstancesSize > mDefaultInstanceCapacity
                    && mInstancesSize < mInstancesCapacity / 2)
                {
                    // 符合一定条件，压缩实例集空间，这里会有性能损耗！
                    ShrinkCapacity();
                }
            } while (false);

            return ret;
        }

        public RESULT SubscribeEvent(uint eventID, EventInstance instance, EventHandler<Event> handler)
        {
            RESULT ret = RESULT.OK;

            do
            {
                if (instance.Index < 0)
                {
                    // 非法实例
                    ret = RESULT.INVALID_PARAM;
                    break;
                }

                if (eventID >= mMaxEvents)
                {
                    // 非法事件
                    ret = RESULT.INVALID_PARAM;
                    break;
                }

                if (handler == null)
                {
                    // 非法事件处理函数
                    ret = RESULT.INVALID_PARAM;
                    break;
                }

                HandlerItem item = ObjectsPools.Instance.AcquireObject<HandlerItem>();
                item.Instance = instance;
                item.Handler = handler;
                mHandlers[eventID].Add(instance.Index, item);
            } while (false);

            return ret;
        }

        public RESULT UnsubscribeEvent(uint eventID, EventInstance instance, EventHandler<Event> handler)
        {
            RESULT ret = RESULT.OK;

            do
            {
                if (instance.Index < 0)
                {
                    // 非法实例
                    ret = RESULT.INVALID_PARAM;
                    break;
                }

                if (eventID >= mMaxEvents)
                {
                    // 非法事件
                    ret = RESULT.INVALID_PARAM;
                    break;
                }

                if (handler == null)
                {
                    // 非法事件处理函数
                    ret = RESULT.INVALID_PARAM;
                    break;
                }

                // 删除
                if (mHandlers[eventID].ContainsKey(instance.Index))
                {
                    mHandlers[eventID].Remove(instance.Index);
                }
                //mHandlers[eventID].RemoveAll(
                //    (HandlerItem item) => 
                //    {
                //        return (instance == item.Instance
                //            && handler == item.Handler);
                //    });
            } while (false);

            return ret;
        }

        public RESULT PostEvent(Event evt, EventInstance sender, EventInstance receiver)
        {
            RESULT ret = RESULT.OK;

            do
            {
                if (evt.ID >= MaxEvents)
                {
                    // 非法事件
                    ret = RESULT.INVALID_PARAM;
                    break;
                }

                if (sender == null
                    || sender.Index < 0 || sender.Index > mInstancesCapacity)
                {
                    // 非法发送者
                    ret = RESULT.INVALID_PARAM;
                    break;
                }

                if (receiver == null
                    || receiver.Index <= EventInstance.INVALID_INDEX 
                    || receiver.Index > mInstancesCapacity)
                {
                    // 非法接受者
                    ret = RESULT.INVALID_PARAM;
                    break;
                }

                EventItem item = ObjectsPools.Instance.AcquireObject<EventItem>();
                item.Evt = evt;
                item.Sender = sender;
                item.Receiver = receiver;

                int current = mCurrentQueue;
                mEventsQueue[current].Enqueue(item);
            } while (false);
            return ret;
        }
        #endregion

        #region 程序集内部接口
        internal void Update()
        {
            Dispatch();
        }
        #endregion

        #region 私有接口
        private RESULT Dispatch()
        {
            RESULT ret = RESULT.OK;

            int current = mCurrentQueue;
            mCurrentQueue = (mCurrentQueue + 1) % 2;

            while (mEventsQueue[current].Count > 0)
            {
                EventItem item = mEventsQueue[current].Dequeue();
                HandleEvent(item.Evt, item.Sender, item.Receiver);
                ObjectsPools.Instance.ReleaseObject(item);
                item = null;
            }

            return ret;
        }

        private void UnsubscribeAllEvents(EventInstance instance)
        {
            foreach (var eventID in instance.Events)
            {
                var handlers = mHandlers[eventID];
                if (handlers.ContainsKey(instance.Index))
                {
                    handlers.Remove(instance.Index);
                }
                //handlers.RemoveAll(
                //    (HandlerItem item) =>
                //    {
                //        return (item.Instance == instance);
                //    });
            }
        }

        private void HandleEvent(Event evt, EventInstance sender, EventInstance receiver)
        {
            if (receiver.Index == EventInstance.BROADCAST_INDEX)
            {
                foreach (var kvp in mHandlers[evt.ID])
                {
                    var handler = kvp.Value;
                    if (handler.Instance.Index > EventInstance.INVALID_INDEX
                        && handler.Instance.Index < mInstancesCapacity)
                    {
                        handler.Handler(sender.Obj, evt);
                    }
                }
            }
            else
            {
                var handler = mHandlers[evt.ID][receiver.Index];
                if (handler != null 
                    && handler.Instance.Index == receiver.Index
                    && receiver.Equals(mInstances[receiver.Index]))
                {
                    handler.Handler(sender.Obj, evt);
                }
            }
        }

        private int ExtendCapacity()
        {
            int index = 0;

            // 扩大两倍
            int capacity = mInstancesCapacity * 2;
            EventInstance[] instances = new EventInstance[capacity];
            // 把原来的数据，复制到新空间里
            Array.Copy(mInstances, instances, mInstancesCapacity);
            // 新的空位
            index = mInstancesCapacity;
            // 指向新空间，调整新空间容量
            mInstances = instances;
            mInstancesCapacity = capacity;

            return index;
        }

        private void ShrinkCapacity()
        {
            // 压缩到原来的3/4空间
            int capacity = mInstancesCapacity * 3 / 4;
            EventInstance[] instances = new EventInstance[capacity];
            // 把原来的数据复制到新空间里
            Array.Copy(mInstances, instances, capacity);
            // 指向新空间，调整新空间容量
            mInstances = instances;
            mInstancesCapacity = capacity;
        }
        #endregion
    }
}
