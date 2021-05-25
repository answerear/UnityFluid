using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;


namespace UnifiedParticlePhysX
{
    using EntityID = System.Int32;

    /// <summary>
    /// 统一求解器
    /// </summary>
    public abstract class Solver
    {
        public const EntityID INVALID_ENTITY = -1;

        internal const int kStabilizationIterations = 2;

        /// <summary>
        /// 每个子步骤执行的求解器迭代次数
        /// </summary>
        public int numIterations = 2;

        /// <summary>
        /// 重力加速度
        /// </summary>
        public Vector3 gravity = new Vector3(0, -9.8f, 0);

        /// <summary>
        /// 最大交互半径
        /// </summary>
        public float radius = 0.1f;

        /// <summary>
        /// 非流体粒子要保持的距离，必须在 (0, radius] 范围内
        /// </summary>
        public float solidRestDistance;

        /// <summary>
        /// 流体粒子在静止密度时相互之间保持的距离，必须在 (0, radius] 范围内。
        /// 对于流体来说，通常应为 radius 的 50-70% ，对于缸体，这个可以简单的与
        /// 粒子半径相同
        /// </summary>
        public float fluidRestDistance;

        /// <summary>
        /// 松弛因子，控制并行求解器的收敛速度，默认值为：1，大于1的值可能会导致不稳定
        /// </summary>
        public float relaxationFactor = 1.0f;

        #region common
        /// <summary>
        /// 与有形状物体碰撞时使用的摩擦系数 
        /// </summary>
        public float dynamicFriction;

        /// <summary>
        /// 与有形状物体碰撞时使用的静态摩擦系数
        /// </summary>
        public float staticFriction;

        /// <summary>
        /// 粒子间碰撞时使用的摩擦系数
        /// </summary>
        public float particleFriction;

        /// <summary>
        /// 与有形状物体碰撞时使用的恢复系数，粒子碰撞始终时无弹性的。
        /// </summary>
        public float restitution;


        /// <summary>
        /// 附着力。 控制粒子在其撞击的表面上的附着的强度，默认为 0.0 ，范围为 [0.0, +∞]
        /// </summary>
        public float adhesion = 0.0f;

        /// <summary>
        /// 速度阈值。 当速度值小于阈值时，使用固定速度值
        /// </summary>
        public float sleepThreshold;

        /// <summary>
        /// 最大速度。 粒子速度超过该值时，直接被 clamp 在该值上
        /// </summary>
        public float maxSpeed;

        /// <summary>
        /// 最大加速度。 粒子加速度超过该值时，直接被 clamp 在该值上。 这避免由于大的互穿透而引起的震荡
        /// </summary>
        public float maxAcceleration;

        /// <summary>
        /// 基于固定参考点高度上，人为减少粒子质量，这使得 stack 和 pile 更快收敛
        /// </summary>
        public float shockPropagation;

        /// <summary>
        /// 损耗，基于粒子接触的数量来降低粒子速度
        /// </summary>
        public float dissipation;

        /// <summary>
        /// 阻尼，粘性阻力，施加与速度成反比的力
        /// </summary>
        public float damping;

        #endregion

        #region cloth
        /// <summary>
        /// 风力
        /// </summary>
        public Vector3 wind;

        /// <summary>
        /// 拖拽力，跟速度平方和面积乘积成正比，方向是速度反方向
        /// </summary>
        public float drag;

        /// <summary>
        /// 上升力，跟速度平方和面积乘积成正比，方向是垂直于速度方向
        /// </summary>
        public float lift;
        #endregion

        #region fluid
        /// <summary>
        /// 凝聚因子，控制粒子批次紧密结合的强度，默认值：0.025，取值范围 [0, +∞]
        /// </summary>
        public float cohesion;

        /// <summary>
        /// 表面张力，控制粒子尝试最小化表面积的强度，默认值：0，取值范围 [0, +∞]
        /// </summary>
        public float surfaceTension;

        /// <summary>
        /// 黏度，使用 XSPH 黏度平滑粒子速度
        /// </summary>
        public float viscosity;

        /// <summary>
        /// 涡旋限制，通过对粒子施加旋转力来增加涡旋度
        /// </summary>
        public float viscosityConfinement;

        /// <summary>
        /// 各向异性缩放值，控制要渲染的椭圆体中存在多少各向异性，如果是0则不计算各向异性
        /// </summary>
        public float anisotropyScale;

        /// <summary>
        /// 
        /// </summary>
        public float anisotropyMin;

        public float anisotropyMax;

        public float smoothing;

        public float solidPressure;

        public float freeSurfaceDrag;

        public float buoyancy;
        #endregion

        #region diffuse
        public float diffuseThreshold;

        public float diffuseBuoyancy;

        public float diffuseDrag;

        public float diffuseLifetime;
        #endregion

        #region collision
        public float collisionDistance;

        public float particleCollisionMargin;

        public float shapeCollisionMargin;

        public float[,] planes = null;

        public int numPlanes = 0;
        #endregion

        internal Bounds bound;

        #region 公有接口
        public abstract EntityID CreateRigidbody(List<Vector3> positions, float mass);

        public abstract EntityID CreateFluid(List<Vector3> positions, float mass);

        public abstract void DestroyEntity(EntityID entityID);

        public abstract void Start();

        public abstract void Step(float dt, int substeps);
        #endregion
    }
}
