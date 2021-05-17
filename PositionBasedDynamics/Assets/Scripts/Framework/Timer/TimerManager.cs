using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Framework
{
    public sealed class TimerManager : Singleton<TimerManager>
    {
        #region 内部结构
        /// <summary>
        /// 定时器任务小根堆比较类
        /// </summary>
        private class TaskPriorityComp : Comparer<TimerTask>
        {
            public override int Compare(TimerTask x, TimerTask y)
            {
                return (int)(x.Expired - y.Expired);
            }
        }
        #endregion

        #region 常量
        /// <summary>
        /// 无效定时器ID
        /// </summary>
        public const uint INVALID_TIMER_ID = 0;
        #endregion

        #region 成员
        private uint mTimerID;                      // 用于生成定时器ID的
        private Stopwatch mStopwatch;               // 计时器对象
        private PriorityQueue<TimerTask> mTasks;    // 小根堆
        private EventInstance mEventInstance;       // 定时器管理器用于收发事件的实例句柄，排队处理定时器事件
        #endregion

        #region 属性
        /// <summary>
        /// 自动生成的定时器ID
        /// </summary>
        public uint TimerID { get { return ++mTimerID; } }
        #endregion

        #region 构造和析构
        private TimerManager()
        {
            mTimerID = 0;
            mStopwatch = null;
            mTasks = null;
            mEventInstance = null;
        }

        ~TimerManager()
        {

        }
        #endregion

        #region 公有接口
        /// <summary>
        /// 启动定时器管理器服务
        /// </summary>
        /// <returns></returns>
        public RESULT Startup()
        {
            // 定时器小根堆
            mTasks = new PriorityQueue<TimerTask>();
            mTasks.Comparer = new TaskPriorityComp();

            // 注册对象和注册关注的事件
            mEventInstance = EventManager.Instance.RegisterObject(this);
            EventManager.Instance.SubscribeEvent((uint)EVENT_ID.TIMER, mEventInstance, HandleTimer);

            // 启动计时器
            mStopwatch = new Stopwatch();
            mStopwatch.Reset();
            mStopwatch.Start();

            return RESULT.OK;
        }

        /// <summary>
        /// 关闭定期是管理器服务
        /// </summary>
        public void Shutdown()
        {
            // 注销事件相关的
            EventManager.Instance.UnsubscribeEvent((uint)EVENT_ID.TIMER, mEventInstance, HandleTimer);
            EventManager.Instance.UnregisterObject(mEventInstance);
        }

        /// <summary>
        /// 获取定时器管理器从启动到当前流逝的毫秒级时间
        /// </summary>
        /// <returns>返回从启动到当前流逝的毫秒级时间</returns>
        public long GetMillisecondsSinceStartup()
        {
            Int64 value = 0;

            if (mStopwatch != null)
            {
                value = mStopwatch.ElapsedMilliseconds;
            }

            return value;
        }

        /// <summary>
        /// 新启动一个定时器
        /// </summary>
        /// <param name="interval">定时器的毫秒级时间间隔</param>
        /// <param name="loop">定时器是否循环</param>
        /// <param name="handler">定时器回调函数</param>
        /// <returns>调用成功返回一个有效的定时器ID</returns>
        public uint StartTimer(uint interval, bool loop, Action<uint> handler)
        {
            uint timerID = INVALID_TIMER_ID;

            Callback<uint> callback = ObjectsPools.Instance.AcquireObject<Callback<uint>>();
            callback.Handler = handler;

            long current = mStopwatch.ElapsedMilliseconds;

            TimerTask task = ObjectsPools.Instance.AcquireObject<TimerTask>();
            task.TimerID = TimerID;
            task.Interval = interval;
            task.Expired = current + interval;
            task.Loop = loop;
            task.Alive = true;
            task.Callback = callback;

            callback.Arg1 = task.TimerID;
            timerID = task.TimerID;
            mTasks.Add(task);

            return timerID;
        }

        /// <summary>
        /// 杀掉定时器
        /// </summary>
        /// <param name="timerID">启动定时器时返回的定时器ID</param>
        /// <returns>调用成功返回RESULT.OK</returns>
        public RESULT KillTimer(uint timerID)
        {
            RESULT ret = RESULT.OK;

            do
            {
                // 这里不能直接删掉，防止在对Task迭代访问中调用本接口导致数组迭代失效
                TimerTask task = mTasks.Find(
                    (TimerTask item) =>
                    {
                        return (item.TimerID == timerID);
                    });

                if (task == null)
                {
                    // 没找到
                    ret = RESULT.ITEM_NOT_FOUND;
                    break;
                }

                // 仅仅设置标志位，后面统一删除
                task.Alive = false;
            } while (false);

            return ret;
        }
        #endregion

        #region 程序集内部接口
        internal void Update()
        {
            // 删掉所有过期的定时器
            RemoveAllExpiredTimer();

            long current = mStopwatch.ElapsedMilliseconds;

            while (mTasks.Count > 0)
            {
                TimerTask task = mTasks.Top();
                if (current < task.Expired)
                {
                    // 最短时间的定时器都没有超时，直接跳出
                    break;
                }

                // 从小根堆弹出
                mTasks.Pop();

                long expired = task.Expired;

                // 放到队列再回调，顺序可控
                TimerEvent evt = ObjectsPools.Instance.AcquireObject<TimerEvent>();
                evt.TimerID = task.TimerID;
                evt.Elapse = current - expired + task.Interval;
                evt.Callback = task.Callback;
                EventManager.Instance.PostEvent(evt, mEventInstance, mEventInstance);

                if (task.Loop)
                {
                    // 循环定时器，调整超时时间后放回小根堆里
                    task.Expired = current + task.Interval;
                    mTasks.Add(task);
                }
                else
                {
                    // 非循环，直接干掉
                    task.Alive = false;
                }
            }
        }
        #endregion

        #region 私有接口
        private void HandleTimer(object sender, Event evt)
        {
            TimerEvent e = (TimerEvent)evt;

            // 回调
            if (e != null && e.Callback != null)
            {
                e.Callback.Run();
            }
        }

        private void RemoveAllExpiredTimer()
        {
            mTasks.RemoveAll(
                (task) =>
                {
                    bool ret = false;

                    if (!task.Alive)
                    {
                        // 这里没办法，直接在match函数先放回缓存池，否则没地方可以调用
                        ObjectsPools.Instance.ReleaseObject(task);
                        ret = true;
                    }

                    return ret;
                });
        }
        #endregion
    }
}
