using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnifiedParticlePhysX
{
    using EntityID = System.Int32;

    internal sealed class SolverCPU : Solver
    {
        public List<Entity> entities = new List<Entity>();

        public List<Particle> particles = new List<Particle>();

        #region 公有接口
        public override EntityID CreateRigidbody(List<Vector3> positions, float mass)
        {
            return CreateEntity<RigidbodyEntity>(positions, mass, Phase.kRigidbody);
        }

        public override EntityID CreateFluid(List<Vector3> positions, float mass)
        {
            return CreateEntity<FluidEntity>(positions, mass, Phase.kFluid);
        }

        public override void DestroyEntity(EntityID entityID)
        {

        }

        public override void Step(float dt, int substeps)
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
