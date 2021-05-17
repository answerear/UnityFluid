using System.Collections;
using System.Collections.Generic;


namespace Framework
{
    /// <summary>
    /// 插件接口类，提供一般插件公有接口声明
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 获取插件名称
        /// </summary>
        /// <returns>返回插件名</returns>
        string Name();

        /// <summary>
        /// 安装插件，插件被加载的时候自动调用
        /// </summary>
        /// <returns>调用成功返回RESULT.OK</returns>
        RESULT Install();

        /// <summary>
        /// 启动插件
        /// </summary>
        /// <returns>调用成功返回RESULT.OK</returns>
        RESULT Startup(List<PluginData> transmission, PluginData recovery);

        /// <summary>
        /// 关闭插件
        /// </summary>
        /// <returns>调用成功返回RESULT.OK</returns>
        RESULT Shutdown();

        /// <summary>
        /// 卸载插件，插件被卸载的时候自动调用
        /// </summary>
        /// <returns>调用成功返回RESULT.OK</returns>
        RESULT Uninstall();
    }
}

