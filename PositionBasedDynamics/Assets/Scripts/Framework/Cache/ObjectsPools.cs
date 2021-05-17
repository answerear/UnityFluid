using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Framework
{
    public class ObjectsPool
    {
        private class PoolObjectPriorityComp : Comparer<PoolObject>
        {
            public override int Compare(PoolObject x, PoolObject y)
            {
                if (x.RecycleTimestamp < y.RecycleTimestamp)
                    return 1;
                else if (x.RecycleTimestamp > y.RecycleTimestamp)
                    return -1;
                else
                    return 0;
            }
        }

        private PriorityQueue<PoolObject> mFreeObjects;
        private List<PoolObject> mUsedObjects;

        public Type Type { get; private set; }
        public long Life { get; set; }

        public ObjectsPool(Type type, long life = 30)
        {
            Type = type;
            Life = life;
            mFreeObjects = new PriorityQueue<PoolObject>();
            mFreeObjects.Comparer = new PoolObjectPriorityComp();
            mUsedObjects = new List<PoolObject>();
        }

        ~ObjectsPool()
        {

        }

        public IPoolObject AcquireObject()
        {
            IPoolObject obj = null;

            if (mFreeObjects.Count == 0)
            {
                // 缓存没有空余的对象，新分配一个
                PoolObject wrapObj = new PoolObject();
                obj = Activator.CreateInstance(Type, false) as IPoolObject;
                wrapObj.ObjectImpl = obj;
                mUsedObjects.Add(wrapObj);
            }
            else
            {
                // 有空余对象，从空余对象列表获取回来
                PoolObject wrapObj = mFreeObjects.Top();
                mFreeObjects.Pop();
                obj = wrapObj.ObjectImpl;
                mUsedObjects.Add(wrapObj);
            }

            return obj;
        }

        public RESULT ReleaseObject(IPoolObject obj)
        {
            RESULT ret = RESULT.OK;

            do
            {
                if (obj == null)
                {
                    ret = RESULT.INVALID_PARAM;
                    break;
                }

                // 告诉对象，被回收了
                obj.OnRecycle();
                PoolObject wrapObj = mUsedObjects.Find(
                    (PoolObject o) =>
                    {
                        return (obj == o.ObjectImpl);
                    });

                // 放回缓存
                if (wrapObj == null)
                {
                    ret = RESULT.ITEM_NOT_FOUND;
                    break;
                }

                mUsedObjects.Remove(wrapObj);
                long timestamp = TimerManager.Instance.GetMillisecondsSinceStartup();
                wrapObj.Recycle(timestamp);
                ret = mFreeObjects.Add(wrapObj);
            } while (false);

            return ret;
        }

        public RESULT ReleaseAll()
        {
            mFreeObjects.Clear();
            return RESULT.OK;
        }

        internal void Update(long timestamp)
        {
            // 检查是否有超过缓存时间，需要释放的
            bool isExpired = true;

            while (isExpired && mFreeObjects.Count > 0)
            {
                PoolObject obj = mFreeObjects.Top();
                isExpired = (timestamp - obj.RecycleTimestamp >= Life);

                if (isExpired)
                {
                    // 过期，淘汰
                    mFreeObjects.Pop();
                }
            }
        }
    }

    public sealed class ObjectsPools : Singleton<ObjectsPools>
    {
        private Dictionary<Type, ObjectsPool> mObjectsPools;

        private ObjectsPools()
        {
            mObjectsPools = new Dictionary<Type, ObjectsPool>();
        }

        ~ObjectsPools()
        {

        }

        public T AcquireObject<T>() where T : IPoolObject
        {
            Type type = typeof(T);
            
            Debug.Assert(type.GetConstructor(Type.EmptyTypes) != null, "no default constructor");
            if (!mObjectsPools.ContainsKey(type))
            {
                mObjectsPools.Add(type, new ObjectsPool(type));
            }

            return (T)mObjectsPools[type].AcquireObject();
        }

        public RESULT ReleaseObject(IPoolObject obj)
        {
            RESULT ret = RESULT.OK;

            do
            {
                if (obj == null)
                {
                    ret = RESULT.INVALID_PARAM;
                    break;
                }

                Type type = obj.GetType();
                //Debug.Assert(type.IsSubclassOf(typeof(IPoolObject)), "wrong type");
                if (!mObjectsPools.ContainsKey(type))
                {
                    mObjectsPools.Add(type, new ObjectsPool(type));
                }

                ret = mObjectsPools[type].ReleaseObject(obj);
            } while (false);

            return ret;
        }

        public RESULT ReleaseAll(Type type)
        {
            RESULT ret = RESULT.OK;

            do
            {
                if (!mObjectsPools.ContainsKey(type))
                {
                    ret = RESULT.ITEM_NOT_FOUND;
                    break;
                }

                ret = mObjectsPools[type].ReleaseAll();
                mObjectsPools.Remove(type);
            } while (false);

            return ret;
        }

        public void ReleaseAll()
        {
            foreach (var pool in mObjectsPools)
            {
                pool.Value.ReleaseAll();
            }

            mObjectsPools.Clear();
        }

        internal void Update()
        {
            long current = TimerManager.Instance.GetMillisecondsSinceStartup();

            foreach (var pool in mObjectsPools)
            {
                pool.Value.Update(current);
            }
        }
    }
}
