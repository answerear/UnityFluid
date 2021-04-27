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
        /// 重力加速度
        /// </summary>
        public Vector3 gravity { get; set; }

        /// <summary>
        /// 最大交互半径
        /// </summary>
        public float radius { get; set; }

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
        /// 松弛因子，控制并行求解器的收敛速度，默认值为：1，大于1的值可能会导致不稳定
        /// </summary>
        public float relaxationFactor { get; set; }

        #region common
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
        /// 附着力。 控制粒子在其撞击的表面上的附着的强度，默认为 0.0 ，范围为 [0.0, +∞]
        /// </summary>
        public float adhesion { get; set; }

        /// <summary>
        /// 速度阈值。 当速度值小于阈值时，使用固定速度值
        /// </summary>
        public float sleepThreshold { get; set; }

        /// <summary>
        /// 最大速度。 粒子速度超过该值时，直接被 clamp 在该值上
        /// </summary>
        public float maxSpeed { get; set; }

        /// <summary>
        /// 最大加速度。 粒子加速度超过该值时，直接被 clamp 在该值上。 这避免由于大的互穿透而引起的震荡
        /// </summary>
        public float maxAcceleration { get; set; }

        /// <summary>
        /// 基于固定参考点高度上，人为减少粒子质量，这使得 stack 和 pile 更快收敛
        /// </summary>
        public float shockPropagation { get; set; }

        /// <summary>
        /// 损耗，基于粒子接触的数量来降低粒子速度
        /// </summary>
        public float dissipation { get; set; }

        /// <summary>
        /// 阻尼，粘性阻力，施加与速度成反比的力
        /// </summary>
        public float damping { get; set; }

        #endregion

        #region 布料
        public Vector3 wind { get; set; }

        public float drag { get; set; }

        public float lift { get; set; }
        #endregion

        #region 流体
        public float cohesion { get; set; }

        public float surfaceTension { get; set; }

        public float viscosity { get; set; }

        public float viscosityConfinement { get; set; }

        public float anisotropyScale { get; set; }

        public float anisotropyMin { get; set; }

        public float anisotropyMax { get; set; }

        public float smoothing { get; set; }

        public float solidPressure { get; set; }

        public float freeSurfaceDrag { get; set; }

        public float buoyancy { get; set; }
        #endregion

        #region 散射
        public float diffuseThreshold { get; set; }

        public float diffuseBuoyancy { get; set; }

        public float diffuseDrag { get; set; }

        public float diffuseLifetime { get; set; }
        #endregion

        #region 碰撞
        public float collisionDistance { get; set; }

        public float particleCollisionMargin { get; set; }

        public float shapeCollisionMargin { get; set; }

        public float [,] planes { get; set; }

        public int numPlanes;
        #endregion

        public void AddEntity(Entity entity)
        {

        }

        public void Step(float dt)
        {

        }
    }
}
