using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    /// <summary>
    /// 抽象约束类
    /// </summary>
    internal abstract class Constraint
    {
        internal enum Group
        {
            kStabilization = 0,
            kContact,
            kStandard,
            kShape,
            kMax
        }

        public Solver solver = null;

        public void Init(Solver s)
        {
            solver = s;
        }

        /// <summary>
        /// 约束投影
        /// </summary>
        public abstract void Project();

        /// <summary>
        /// 更新粒子受约束影响的数量
        /// </summary>
        public abstract void UpdateCounts();
    }
}
