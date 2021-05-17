using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework
{
    public class FrameworkSystem : Singleton<FrameworkSystem>
    {
        private FrameworkSystem()
        {

        }

        ~FrameworkSystem()
        {

        }

        public RESULT Startup(uint events)
        {
            RESULT ret = RESULT.OK;

            do
            {
                // 缓存
                ObjectsPools.CreateInstance();
                // 插件
                PluginManager.CreateInstance();
                // 事件
                EventManager.CreateInstance();
                EventManager.Instance.MaxEvents = events;
                // 时间工具
                TimeUtils.CreateInstance();
                // 定时器
                TimerManager.CreateInstance();
                TimerManager.Instance.Startup();
            } while (false);

            return ret;
        }

        public void Update()
        {
            // 先调用定时器更新，可以让到时的定时器能在这一帧可以马上在EventManager执行
            TimerManager.Instance.Update();
            EventManager.Instance.Update();
            ObjectsPools.Instance.Update();
        }

        public RESULT Shutdown()
        {
            RESULT ret = RESULT.OK;

            do
            {
                TimerManager.Instance.Shutdown();
                TimerManager.DestroyInstance();

                EventManager.DestroyInstance();

                PluginManager.Instance.UnloadAllPlugins();
                PluginManager.DestroyInstance();

                ObjectsPools.DestroyInstance();

                System.GC.Collect();
            } while (false);

            return ret;
        }
    }
}
