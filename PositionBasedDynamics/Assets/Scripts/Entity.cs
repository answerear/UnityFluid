using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    public enum Phase
    {
        kNone = 0,
        kRigidbody,
        kFluid
    }

    /// <summary>
    /// 实体类
    /// </summary>
    public class Entity 
    {
        /// <summary>
        /// 系统外力
        /// </summary>
        public List<Force> externalForces { get; private set; }

        /// <summary>
        /// 系统内力
        /// </summary>
        public List<Force> internalForces { get; private set; }

        /// <summary>
        /// 粒子索引
        /// </summary>
        public List<int> particles { get; set; }

        /// <summary>
        /// 实体质量
        /// </summary>
        public float mass { get; set; }

        /// <summary>
        /// 质心位置
        /// </summary>
        public float massCenter { get; set; }

        public void AddExternalForce<T>(T force) where T : Force
        {

        }

        public void RemoveExternalForce<T>() where T : Force
        {

        }

        public void AddInternalForce<T>(T force) where T : Force
        {

        }

        public void RemoveInternalForce<T>() where T : Force
        {

        }
    }
}
