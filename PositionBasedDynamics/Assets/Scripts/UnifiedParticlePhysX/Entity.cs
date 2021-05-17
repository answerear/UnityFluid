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
        public List<Force> externalForces = new List<Force>();

        /// <summary>
        /// 系统内力
        /// </summary>
        public List<Force> internalForces = new List<Force>();

        /// <summary>
        /// 粒子索引
        /// </summary>
        public List<int> particles = new List<int>();

        /// <summary>
        /// 实体质量
        /// </summary>
        public float mass = 0.0f;

        /// <summary>
        /// 质心位置
        /// </summary>
        public float massCenter = 0.0f;

        public Phase phase = Phase.kNone;

        public Entity()
        {
            externalForces = new List<Force>();
            internalForces = new List<Force>();
        }

        public bool AddExternalForce<T>(T force) where T : Force
        {
            return AddForce<T>(externalForces, force);
        }

        public void RemoveExternalForce<T>() where T : Force
        {
            RemoveForce<T>(externalForces);
        }

        public bool AddInternalForce<T>(T force) where T : Force
        {
            return AddForce<T>(internalForces, force);
        }

        public void RemoveInternalForce<T>() where T : Force
        {
            RemoveForce<T>(internalForces);
        }

        public float GetMassScale(Particle particle, float coeff)
        {
            float rval = 1.0f;

            if (phase == Phase.kRigidbody)
            {
                rval = Mathf.Exp(- coeff * particle.position.y);
            }

            return rval;
        }

        private bool AddForce<T>(List<Force> forces, Force force) where T : Force
        {
            bool found = false;

            for (int i = 0; i < forces.Count; ++i)
            {
                if (typeof(T) == forces[i].GetType())
                {
                    // 同种力只有一种
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                forces.Add(force);
            }

            return !found;
        }

        private void RemoveForce<T>(List<Force> forces) where T : Force
        {
            for (int i = 0; i < externalForces.Count; ++i)
            {
                if (typeof(T) == externalForces[i].GetType())
                {
                    forces.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
