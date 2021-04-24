using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PositionBasedDynamics
{
    public class Solver
    {
        /// <summary>
        /// 每个子步骤执行的求解器迭代次数
        /// </summary>
        public int numIterations { get; set; }

        /// <summary>
        /// 非流体粒子要保持的距离，必须在 (0, radius] 范围内
        /// </summary>
        public float solidRestDistance { get; set; }

        /// <summary>
        /// 流体粒子在静止密度时相互之间保持的距离，必须在 (0, radius] 范围内。
        /// 对于流体来说，通常应为 radius 的 50-70% ，对于缸体，这个可以简单的与
        /// 粒子半径相同
        /// </summary>
        public float fluidRestDistance { get; set; }

        /// <summary>
        /// 与有形状物体碰撞时使用的摩擦系数 
        /// </summary>
        public float dynamicFriction { get; set; }

        /// <summary>
        /// 与有形状物体碰撞时使用的静态摩擦系数
        /// </summary>
        public float staticFriction { get; set; }

        /// <summary>
        /// 粒子间碰撞时使用的摩擦系数
        /// </summary>
        public float particleFriction { get; set; }

        /// <summary>
        /// 与有形状物体碰撞时使用的恢复系数，粒子碰撞始终时无弹性的。
        /// </summary>
        public float restitution { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float adhesion { get; set; }

        public float cohesion { get; set; }

        public float surfaceTension { get; set; }

        public float viscosity { get; set; }

        public float viscosityConfinement { get; set; }

        public void AddForce(Force force)
        {

        }

        public void AddEntity(Entity entity)
        {

        }

        public void Step(float dt, int substeps)
        {

        }
    }
}
