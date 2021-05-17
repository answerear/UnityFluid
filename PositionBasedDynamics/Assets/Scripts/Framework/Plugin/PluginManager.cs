using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Framework
{
    /// <summary>
    /// 插件管理器，用于管理插件的加载、卸载
    /// </summary>
    public sealed class PluginManager : Singleton<PluginManager>
    {
        #region 成员
        private Dictionary<string, IPlugin> mPlugins;
        private StringBuilder mStrBuilder;
        #endregion

        #region 属性
        /// <summary>
        /// 插件所在路径
        /// </summary>
        public string PluginPath { get; set; }
        #endregion

        #region 构造和析构
        /// <summary>
        /// 构造函数
        /// </summary>
        private PluginManager()
        {
            mPlugins = new Dictionary<string, IPlugin>();
            mStrBuilder = new StringBuilder();
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~PluginManager()
        {
            UnloadAllPlugins();
        }
        #endregion

        #region 公有接口
        /// <summary>
        /// 加载插件
        /// </summary>
        /// <param name="pluginName">插件名</param>
        /// <param name="className">对应插件类名</param>
        /// <param name="plugin">返回的插件对象</param>
        /// <returns></returns>
        public RESULT LoadPlugin(string pluginName, string className, out IPlugin plugin)
        {
            RESULT ret = RESULT.OK;
            plugin = null;

            do
            {
                if (mPlugins.TryGetValue(pluginName, out plugin))
                {
                    // 已经存在了，直接抛出已经错在的错误码
                    ret = RESULT.ITEM_EXIST;
                    Log.Warning("Framework", "Plugin ", pluginName, " has already existed !");
                    break;
                }

                try
                {
                    // 非终端平台，先加载对应程序集
                    mStrBuilder.Length = 0;
                    mStrBuilder.Append(PluginPath);
                    mStrBuilder.Append("/");
                    mStrBuilder.Append(pluginName);
                    mStrBuilder.Append(".dll");
                    var assembly = Assembly.LoadFrom(mStrBuilder.ToString());
                    // 创建插件对象
                    Type type = assembly.GetType(className);
                    plugin = Activator.CreateInstance(type) as IPlugin;
                }
                catch (Exception e)
                {
                    ret = RESULT.FAIL;
                    Log.Warning("Framework", "Exception : ", e.Message, " When load plugin ", pluginName);
                    break;
                }

                // 告诉插件，加载了
                ret = plugin.Install();
                if (ret != RESULT.OK)
                {
                    break;
                }

                // 添加到插件管理器中
                mPlugins.Add(pluginName, plugin);
            } while (false);

            return ret;
        }

        /// <summary>
        /// 卸载插件
        /// </summary>
        /// <param name="pluginName">要卸载的插件名</param>
        /// <returns></returns>
        public RESULT UnloadPlugin(string pluginName)
        {
            RESULT ret = RESULT.OK;

            do
            {
                IPlugin plugin = null;
                if (!mPlugins.TryGetValue(pluginName, out plugin))
                {
                    // 对应名称的插件不存在，直接返回
                    ret = RESULT.ITEM_NOT_FOUND;
                    break;
                }

                // 告诉插件，卸载了
                ret = plugin.Uninstall();
                if (ret != RESULT.OK)
                {
                    break;
                }

                // 从插件管理器中移除
                if (mPlugins.Remove(pluginName))
                {
                    ret = 0;
                }
            } while (false);


            return ret;
        }

        /// <summary>
        /// 卸载所有插件
        /// </summary>
        public RESULT UnloadAllPlugins()
        {
            foreach (var plugin in mPlugins.Values)
            {
                plugin.Shutdown();
                plugin.Uninstall();
            }

            mPlugins.Clear();
            return RESULT.OK;
        }

        /// <summary>
        /// 获取指定名称的插件对象
        /// </summary>
        /// <param name="pluginName">插件名</param>
        /// <returns>返回对应的插件对象</returns>
        public IPlugin GetPlugin(string pluginName)
        {
            IPlugin plugin = null;

            do
            {
                mPlugins.TryGetValue(pluginName, out plugin);
            } while (false);

            return plugin;
        }
        #endregion
    }
}

