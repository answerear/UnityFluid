using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 备份数据抽象类
    /// </summary>
    public abstract class PluginData
    {
        /// <summary>
        /// 获取跟当前插件数据有关的插件名称
        /// </summary>
        public abstract string GetPluginName();
    }
}
