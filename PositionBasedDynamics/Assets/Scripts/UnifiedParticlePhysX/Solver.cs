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
    public class Solver : Singleton<Solver>
    {
        public const EntityID INVALID_ENTITY = -1;

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

        #region cloth
        /// <summary>
        /// 风力
        /// </summary>
        public Vector3 wind { get; set; }

        /// <summary>
        /// 拖拽力，跟速度平方和面积乘积成正比，方向是速度反方向
        /// </summary>
        public float drag { get; set; }

        /// <summary>
        /// 上升力，跟速度平方和面积乘积成正比，方向是垂直于速度方向
        /// </summary>
        public float lift { get; set; }
        #endregion

        #region fluid
        /// <summary>
        /// 凝聚因子，控制粒子批次紧密结合的强度，默认值：0.025，取值范围 [0, +∞]
        /// </summary>
        public float cohesion { get; set; }

        /// <summary>
        /// 表面张力，控制粒子尝试最小化表面积的强度，默认值：0，取值范围 [0, +∞]
        /// </summary>
        public float surfaceTension { get; set; }

        /// <summary>
        /// 黏度，使用 XSPH 黏度平滑粒子速度
        /// </summary>
        public float viscosity { get; set; }

        /// <summary>
        /// 涡旋限制，通过对粒子施加旋转力来增加涡旋度
        /// </summary>
        public float viscosityConfinement { get; set; }

        /// <summary>
        /// 各向异性缩放值，控制要渲染的椭圆体中存在多少各向异性，如果是0则不计算各向异性
        /// </summary>
        public float anisotropyScale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float anisotropyMin { get; set; }

        public float anisotropyMax { get; set; }

        public float smoothing { get; set; }

        public float solidPressure { get; set; }

        public float freeSurfaceDrag { get; set; }

        public float buoyancy { get; set; }
        #endregion

        #region diffuse
        public float diffuseThreshold { get; set; }

        public float diffuseBuoyancy { get; set; }

        public float diffuseDrag { get; set; }

        public float diffuseLifetime { get; set; }
        #endregion

        #region collision
        public float collisionDistance { get; set; }

        public float particleCollisionMargin { get; set; }

        public float shapeCollisionMargin { get; set; }

        public float [,] planes { get; set; }

        public int numPlanes;
        #endregion

        private List<Entity> entities = new List<Entity>();

        private List<Particle> particles = new List<Particle>();

        #region 公有接口
        public EntityID CreateRigidbody(List<Vector3> positions, float mass)
        {
            return CreateEntity<RigidbodyEntity>(positions, mass, Phase.kRigidbody);
        }

        public EntityID CreateFluid(List<Vector3> positions, float mass)
        {
            return CreateEntity<FluidEntity>(positions, mass, Phase.kFluid);
        }

        public void DestroyEntity(EntityID entityID)
        {
            
        }

        public void Step(float dt, int substeps)
        {
            float t = dt / substeps;

            for (int i = 0; i < substeps; ++i)
            {
                Update(t);
            }
        }
        #endregion

        #region 私有接口
        private EntityID CreateEntity<T>(List<Vector3> positions, float mass, Phase phase) where T : Entity
        {
            EntityID entityID = INVALID_ENTITY;

            // 创建内部实体对象，并添加到对象列表中
            Entity entity = Activator.CreateInstance<T>();
            entityID = entities.Count;
            entities.Add(entity);

            float invMass = 1.0f / (mass);

            // 创建实体对应的粒子
            for (int i = 0; i < particles.Count; ++i)
            {
                Particle particle = new Particle(positions[i], invMass, phase, entityID);
                int index = particles.Count;
                entity.particles.Add(index);
                particles.Add(particle);
            }

            // 所有 entity 都受重力影响，所以默认添加重力进去
            Gravity g = new Gravity(gravity);
            entity.AddExternalForce<Gravity>(g);

            return entityID;
        }

        private void Update(float dt)
        {
            EntityID entityID = INVALID_ENTITY;
            Entity entity = null;
            Force externalForce = null;

            // 1: for all particles $i$ do
            for (int i = 0; i < particles.Count; ++i)
            {
                Particle particle = particles[i];

                if (entityID != particle.body)
                {
                    // 由于 particle 存储是连续的，获取新的 entity 并计算一次外力合力
                    GetEntityAndExternalForce(particle, out entity, out externalForce);
                }

                // 2: apply external forces $\vec{v}_i \Leftarrow \vec{v}_i + \Delta{t} \vec{f}_{ext}(\vec{x}_i)$
                externalForce.ApplyToParticle(dt, particle);

                // 3: predict position $\vec{x}_^* \Leftarrow \vec{x}_i + \Delta{t} \vec{v}_i$
                particle.predictPosition = particle.position + particle.velocity * dt;

                // 4: apply mass scaling $m_i^* = m_i e^{-kh(\vec{x}_i^*)}$
                float s = entity.GetMassScale(particle, shockPropagation);
                particle.inverseMass = (s == 1.0f ? particle.inverseMass : 1.0f / ((1.0f / particle.inverseMass) * s));
            }
            // 5: end for

            // 6: for all particles $i$ do
            // 7: find neighboring particles $N_i(\vec{x}_i^*)$
            // 8: find solid contacts
            // 9: end for
            // 10: while iter < stabilizationIterations do
            // 11: ∆x⇐0,n⇐0
            // 12: solve contact constraints for∆x,n
            // 13: updatexi⇐xi+ ∆x/n
            // 14: updatex∗⇐x∗+ ∆x/n
            // 15: end while
            // 16: while iter < solverIterations do
            // 17: for each constraint group G do
            // 18: ∆x⇐0,n⇐0
            // 19: solve all constraints in G for∆x,n
            // 20: updatex∗⇐x∗+ ∆x/n
            // 21: end for
            // 22: end while
            // 23: for all particles $i$ do
            // 24: update velocityvi⇐1∆t(x∗i−xi)
            // 25: advect diffuse particles
            // 26: apply internal forces fdrag,fvort
            // 27: update positionsxi⇐x∗ior apply sleeping
            // 28: end for
        }

        private void GetEntityAndExternalForce(Particle particle, out Entity entity, out Force force)
        {
            entity = entities[particle.body];

            force = new Force();

            for (int i = 0; i < entity.externalForces.Count; ++i)
            {
                force.value += entity.externalForces[i].value;
            }
        }
        #endregion
    }
}
