using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using System;

namespace UnifiedParticlePhysX
{
    /// <summary>
    /// 附近相邻查找器
    /// </summary>
    public abstract class PointNeighborSearcher
    {
        /// <summary>
        /// 根据粒子构建查找器
        /// </summary>
        /// <param name="particles">粒子集合</param>
        /// <param name="indices">使用到的粒子索引集合</param>
        public abstract void Build(List<int> indices);

        /// <summary>
        /// 查找附近相邻的点
        /// </summary>
        /// <param name="origin">原点索引</param>
        /// <param name="radius">搜索半径</param>
        /// <param name="handler">查找到回调，回调第一个参数是找到相邻粒子在相邻粒子集合索引，第二个参数是找到的相邻粒子在整个particles集合中的索引</param>
        public abstract void ForeachNearbyPoint(int origin, float radius, Action<int, int> handler);
    }
}
