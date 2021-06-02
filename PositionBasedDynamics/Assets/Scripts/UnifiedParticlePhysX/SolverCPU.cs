
// #define USE_STABILIZATION

using Framework;
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

        private List<List<Constraint>> constraints = new List<List<Constraint>>();

        public SolverCPU()
        {
            int i = 0;
            for (i = 0; i < (int)Constraint.Group.kMax; ++i)
            {
                constraints.Add(new List<Constraint>());
            }
        }

        #region 公有接口
        public override EntityID CreateRigidbody(List<Vector3> positions, float mass)
        {
            return CreateEntity<RigidbodyEntity>(positions, mass, Phase.kRigidbody);
        }

        public override EntityID CreateFluid(List<Vector3> positions, float density)
        {
            float diameter = radius * 2.0f;
            float mass = 0.8f * diameter * diameter * diameter * density;
            EntityID id = CreateEntity<FluidEntity>(positions, mass, Phase.kFluid);

            if (id != INVALID_ENTITY)
            {
                DensityConstraint constraint = ObjectsPools.Instance.AcquireObject<DensityConstraint>();
                FluidEntity entity = entities[id] as FluidEntity;
                constraint.Init(this, entity, density, radius);
                constraints[(int)Constraint.Group.kStandard].Add(constraint);
            }

            return id;
        }

        public override void DestroyEntity(EntityID entityID)
        {

        }

        public override void Start()
        {
            // 计算个平面包围空间的边界，没有的情况下，默认给一个非常远的边界
#if true
            bound.center = Vector3.zero;
            bound.extents = new Vector3(500.0f, 500.0f, 500.0f);
#else
            float minX, minY, minZ, maxX, maxY, maxZ;
            minX = minY = minZ = -1000.0f;
            maxX = maxY = maxZ = 1000.0f;

            Vector3[] minPoints = new Vector3[numPlanes];
            Vector3[] maxPoints = new Vector3[numPlanes];

            for (int i = 0; i < numPlanes; ++i)
            {
                if (planes[i, 0] != 0.0f)
                {
                    minPoints[i].x = (minY * planes[i, 1] + minZ * planes[i, 2] + planes[i, 3]) / planes[i, 0];
                    maxPoints[i].x = (maxY * planes[i, 1] + maxZ * planes[i, 2] + planes[i, 3]) / planes[i, 0];
                }
                else
                {
                    minPoints[i].x = minX;
                    maxPoints[i].x = maxX;
                }

                if (planes[i, 1] != 0.0f)
                {
                    minPoints[i].y = (minX * planes[i, 0] + minZ * planes[i, 2] + planes[i, 3]) / planes[i, 1];
                    maxPoints[i].y = (maxX * planes[i, 0] + maxZ * planes[i, 2] + planes[i, 3]) / planes[i, 1];
                }
                else
                {
                    minPoints[i].y = minY;
                    maxPoints[i].y = maxY;
                }

                if (planes[i, 2] != 0.0f)
                {
                    minPoints[i].z = (minX * planes[i, 0] + minY * planes[i, 1] + planes[i, 3]) / planes[i, 2];
                    maxPoints[i].z = (maxX * planes[i, 0] + maxY * planes[i, 1] + planes[i, 3]) / planes[i, 2];
                }
                else
                {
                    minPoints[i].z = minZ;
                    maxPoints[i].z = maxZ;
                }
            }

            //minX = minY = minZ = float.MinValue;
            //maxX = maxY = maxZ = float.MaxValue;

            Vector3 minSize = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxSize = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = 0; i < numPlanes; ++i)
            {
                //if (minPoints[i].x > minX)
                //{
                //    minX = minPoints[i].x;
                //}
                //else if (maxPoints[i].x < maxX)
                //{
                //    maxX = maxPoints[i].x;
                //}

                //if (minPoints[i].y > minY)
                //{
                //    minY = minPoints[i].y;
                //}
                //else if (maxPoints[i].y < maxY)
                //{
                //    maxY = maxPoints[i].y;
                //}

                //if (minPoints[i].z > minZ)
                //{
                //    minZ = minPoints[i].z;
                //}
                //else if (maxPoints[i].z < maxZ)
                //{
                //    maxZ = maxPoints[i].z;
                //}

                if (minPoints[i].x != minX && minPoints[i].x < minSize.x)
                {
                    minSize.x = minPoints[i].x;
                }
                else if (maxPoints[i].x != maxX && maxPoints[i].x > maxSize.x)
                {
                    maxSize.x = maxPoints[i].x;
                }

                if (minPoints[i].y != minY && minPoints[i].y < minSize.y)
                {
                    minSize.y = minPoints[i].y;
                }
                else if (maxPoints[i].y != maxY && maxPoints[i].y > maxSize.y)
                {
                    maxSize.y = maxPoints[i].y;
                }

                if (minPoints[i].z != minZ && minPoints[i].z < minSize.z)
                {
                    minSize.z = minPoints[i].z;
                }
                else if (maxPoints[i].z != maxZ && maxPoints[i].z > maxSize.z)
                {
                    maxSize.z = maxPoints[i].z;
                }
            }

            bound.min = minSize;
            bound.max = maxSize;
#endif
        }

        public override void Step(float dt, int substeps)
        {
            float t = dt / substeps;

            for (int i = 0; i < substeps; ++i)
            {
                Update(t);
            }
        }

        public override List<Vector3> GetEntityPositions(EntityID id)
        {
            List<Vector3> positions = new List<Vector3>();

            Entity entity = entities[id];
            for (int i = 0; i < entity.particles.Count; ++i)
            {
                positions.Add(particles[entity.particles[i]].position);
            }

            return positions;
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
            for (int i = 0; i < positions.Count; ++i)
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
                particle.FrameInit();

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
            for (int i = 0; i < particles.Count; ++i)
            {
                Particle p1 = particles[i];

                // 7: find neighboring particles $N_i(\vec{x}_i^*)$
                FindNeighboringParticles(constraints, p1, i);

                // 8: find solid contacts
                FindSolidContacts(constraints, p1, i);
            }
            // 9: end for

            // Update the number of constraints affecting each particle
            for (int i = 0; i < (int)Constraint.Group.kMax; ++i)
            {
                for (int j = 0; j < constraints[i].Count; ++j)
                {
                    constraints[i][j].UpdateCounts();
                }
            }

#if USE_STABILIZATION
            // 10: for iter < stabilizationIterations do
            for (int i = 0; i < kStabilizationIterations; ++i)
            {
                // 11: ∆x⇐0, n⇐0
                // 12: solve contact constraints for ∆x,n
                // 13: update xi ⇐ xi + ∆x/n
                // 14: update x∗ ⇐ x∗ + ∆x/n
                List<Constraint> stableConstraints = constraints[(int)Constraint.Group.kStabilization];
                for (int j = 0; j < stableConstraints.Count; ++j)
                {
                    stableConstraints[j].Project();
                }
            }
            // 15: end for
#endif

            // 16: while iter < solverIterations do
            for (int i = 0; i < numIterations; ++i)
            {
                // 17: for each constraint group G do
                for (int j = 1; j < (int)Constraint.Group.kMax; ++j)
                {
                    List<Constraint> tmpConstraints = constraints[j];
                    for (int k = 0; k < tmpConstraints.Count; ++k)
                    {
                        // 18: ∆x⇐0, n⇐0
                        // 19: solve all constraints in G for ∆x,n
                        // 20: update x∗ ⇐ x∗ + ∆x/n
                        tmpConstraints[k].Project();
                    }
                }
                // 21: end for
            }
            // 22: end while

            // 23: for all particles $i$ do
            for (int i = 0; i < particles.Count; ++i)
            {
                Particle p = particles[i];

                // 24: update velocity vi ⇐ 1∆t(x∗i − xi)
                p.velocity = (p.predictPosition - p.position) / dt;

                // 25: advect diffuse particles

                // 26: apply internal forces fdrag, fvort

                // 27: update positions xi ⇐ x∗i or apply sleeping
                if ((p.predictPosition - p.position).magnitude < sleepThreshold)
                {
                    p.velocity = Vector3.zero;
                    p.position = p.predictPosition;
                }
                else
                {
                    p.position = p.predictPosition;
                }
            }
            // 28: end for

            List<Constraint> contactConstraints = constraints[(int)Constraint.Group.kContact];
            contactConstraints.Clear();

            List<Constraint> stabiliationConstraints = constraints[(int)Constraint.Group.kStabilization];
            for (int i = 0; i < stabiliationConstraints.Count; ++i)
            {
                BoundaryConstraint constraint = stabiliationConstraints[i] as BoundaryConstraint;
                ObjectsPools.Instance.ReleaseObject(constraint);
            }

            stabiliationConstraints.Clear();
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

        private void FindNeighboringParticles(List<List<Constraint>> constraints, Particle p, int index)
        {
            int j = 0;
            for (j = index + 1; j < particles.Count; ++j)
            {
                Particle p2 = particles[j];

                if (p.inverseMass == 0 && p2.inverseMass == 0)
                {
                    // 两个粒子均为静态物体
                    continue;
                }

                if (p.phase == Phase.kRigidbody && p2.phase == Phase.kRigidbody && p.body == p2.body && p.body != -1)
                {
                    // 两个粒子均属于同一个固体
                    continue;
                }

                float d = Vector3.Distance(p.predictPosition, p2.predictPosition);

                if (d < 2 * radius - UnifiedParticleSystem.kEpsilon)
                {
                    // 两个粒子有重叠接触，表示发生碰撞

                    if (p.phase == Phase.kRigidbody && p2.phase == Phase.kRigidbody)
                    {
                        // 两个都是刚体，用刚体接触约束。 这里还包含了摩擦力的计算
                        RigidContactConstraint constraint = ObjectsPools.Instance.AcquireObject<RigidContactConstraint>();
                        RigidbodyEntity body = entities[p.body] as RigidbodyEntity;
#if USE_STABILIZATION
                        constraint.Init(this, body, index, j, true);
#else
                        constraint.Init(this, body, index, j, false);
#endif
                        constraints[(int)Constraint.Group.kContact].Add(constraint);
#if USE_STABILIZATION
                        constraints[(int)Constraint.Group.kStabilization].Add(constraint);
#endif
                    }
                    else if (p.phase == Phase.kRigidbody || p2.phase == Phase.kRigidbody)
                    {
                        // 其中之一是刚体，使用普通接触约束
                        ContactConstraint constraint = ObjectsPools.Instance.AcquireObject<ContactConstraint>();
#if USE_STABILIZATION
                        constraint.Init(this, index, j, true);
#else
                        constraint.Init(this, index, j, false);
#endif
                        constraints[(int)Constraint.Group.kContact].Add(constraint);
#if USE_STABILIZATION
                        constraints[(int)Constraint.Group.kStabilization].Add(constraint);
#endif
                    }
                }
            }
        }

        private void FindSolidContacts(List<List<Constraint>> constraints, Particle p, int index)
        {
#if false
            for (int j = 0; j < numPlanes; ++j)
            {
                Vector3 n = new Vector3(planes[j, 0], planes[j, 1], planes[j, 2]);
                float d = planes[j, 3];
                Vector3 pos = p.predictPosition - radius * n;
                float ret = n.x * pos.x + n.y * pos.y + n.z * pos.z + d;
                if (ret < 0)
                {
                    // 超出边界
                    BoundaryConstraint constraint = ObjectsPools.Instance.AcquireObject<BoundaryConstraint>();
                    //constraint.Init(this, index, n, d, false);
#if USE_STABILIZATION
                    BoundaryConstraint.Boundary[] bound = new BoundaryConstraint.Boundary[6]
                    {
                        BoundaryConstraint.Boundary.kLeft, BoundaryConstraint.Boundary.kRight,
                        BoundaryConstraint.Boundary.kTop, BoundaryConstraint.Boundary.kBottom,
                        BoundaryConstraint.Boundary.kForward, BoundaryConstraint.Boundary.kBack,
                    };

                    float[] vals = new float[6] 
                    { 
                        -Mathf.Abs(d), Mathf.Abs(d), 
                        Mathf.Abs(d), -Mathf.Abs(d), 
                        Mathf.Abs(d), -Mathf.Abs(d) 
                    };

                    constraint.Init(this, index, bound[j], vals[j], false);
#else
#endif
                    constraints[(int)Constraint.Group.kContact].Add(constraint);
#if USE_STABILIZATION
                    constraints[(int)Constraint.Group.kStabilization].Add(constraint);
#endif
                }
            }
#else
            if (p.predictPosition.x < -Mathf.Abs(planes[0, 3]) + radius)
            {
                // Out of left
                BoundaryConstraint constraint = ObjectsPools.Instance.AcquireObject<BoundaryConstraint>();
#if USE_STABILIZATION
                constraint.Init(this, index, BoundaryConstraint.Boundary.kLeft, -Mathf.Abs(planes[0, 3]), true);
#else
                constraint.Init(this, index, BoundaryConstraint.Boundary.kLeft, -Mathf.Abs(planes[0, 3]), false);
#endif
                constraints[(int)Constraint.Group.kContact].Add(constraint);
#if USE_STABILIZATION
                 constraints[(int)Constraint.Group.kStabilization].Add(constraint);
#endif
            }
            else if (p.predictPosition.x > Mathf.Abs(planes[1, 3]) - radius)
            {
                // Out of right
                BoundaryConstraint constraint = ObjectsPools.Instance.AcquireObject<BoundaryConstraint>();
#if USE_STABILIZATION
                constraint.Init(this, index, BoundaryConstraint.Boundary.kRight, Mathf.Abs(planes[1, 3]), true);
#else
                constraint.Init(this, index, BoundaryConstraint.Boundary.kRight, Mathf.Abs(planes[1, 3]), false);
#endif
                constraints[(int)Constraint.Group.kContact].Add(constraint);
#if USE_STABILIZATION
                 constraints[(int)Constraint.Group.kStabilization].Add(constraint);
#endif
            }
            else if (p.predictPosition.y > Mathf.Abs(planes[2, 3]) - radius)
            {
                // Out of top
                BoundaryConstraint constraint = ObjectsPools.Instance.AcquireObject<BoundaryConstraint>();
#if USE_STABILIZATION
                constraint.Init(this, index, BoundaryConstraint.Boundary.kTop, Mathf.Abs(planes[2, 3]), true);
#else
                constraint.Init(this, index, BoundaryConstraint.Boundary.kTop, Mathf.Abs(planes[2, 3]), false);
#endif
                constraints[(int)Constraint.Group.kContact].Add(constraint);
#if USE_STABILIZATION
                 constraints[(int)Constraint.Group.kStabilization].Add(constraint);
#endif
            }
            else if (p.predictPosition.y < -Mathf.Abs(planes[3, 3]) + radius)
            {
                // Out of bottom
                BoundaryConstraint constraint = ObjectsPools.Instance.AcquireObject<BoundaryConstraint>();
#if USE_STABILIZATION
                constraint.Init(this, index, BoundaryConstraint.Boundary.kBottom, -Mathf.Abs(planes[3, 3]), true);
#else
                constraint.Init(this, index, BoundaryConstraint.Boundary.kBottom, -Mathf.Abs(planes[3, 3]), false);
#endif
                constraints[(int)Constraint.Group.kContact].Add(constraint);
#if USE_STABILIZATION
                 constraints[(int)Constraint.Group.kStabilization].Add(constraint);
#endif
            }
            else if (p.predictPosition.z > Mathf.Abs(planes[4, 3]) - radius)
            {
                // Out of forward
                BoundaryConstraint constraint = ObjectsPools.Instance.AcquireObject<BoundaryConstraint>();
#if USE_STABILIZATION
                constraint.Init(this, index, BoundaryConstraint.Boundary.kForward, Mathf.Abs(planes[4, 3]), true);
#else
                constraint.Init(this, index, BoundaryConstraint.Boundary.kForward, Mathf.Abs(planes[4, 3]), false);
#endif
                constraints[(int)Constraint.Group.kContact].Add(constraint);
#if USE_STABILIZATION
                 constraints[(int)Constraint.Group.kStabilization].Add(constraint);
#endif
            }
            else if (p.predictPosition.z < -Math.Abs(planes[5, 3]) + radius)
            {
                // Out of back
                BoundaryConstraint constraint = ObjectsPools.Instance.AcquireObject<BoundaryConstraint>();
#if USE_STABILIZATION
                constraint.Init(this, index, BoundaryConstraint.Boundary.kBack, -Mathf.Abs(planes[5, 3]), true);
#else
                constraint.Init(this, index, BoundaryConstraint.Boundary.kBack, -Mathf.Abs(planes[5, 3]), false);
#endif
                constraints[(int)Constraint.Group.kContact].Add(constraint);
#if USE_STABILIZATION
                 constraints[(int)Constraint.Group.kStabilization].Add(constraint);
#endif
            }
#endif
        }
        #endregion
    }
}
