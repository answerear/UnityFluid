using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    public abstract class Singleton<T> where T : Singleton<T>
    {
        /// <summary>
        /// 获取单例对象，无法设置单例对象
        /// </summary>
        public static T Instance { get; private set; }

        private readonly static object msLockObj = new object();

        /// <summary>
        /// 创建单例对象
        /// </summary>
        /// <returns>返回新建单例对象</returns>
        public static T CreateInstance()
        {
            if (Instance == null)
            {
                lock (msLockObj)
                {
                    Instance = Activator.CreateInstance(typeof(T), true) as T;
                }
            }

            return Instance;
        }

        /// <summary>
        /// 销毁单例对象（可重入）
        /// </summary>
        public static void DestroyInstance()
        {
            Instance = null;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public Singleton()
        {
            Instance = this as T;
        }

        /// <summary>
        /// 释放单例的引用，避免无法GC
        /// </summary>
        public void Release()
        {
            Instance = null;
        }
    }
}
