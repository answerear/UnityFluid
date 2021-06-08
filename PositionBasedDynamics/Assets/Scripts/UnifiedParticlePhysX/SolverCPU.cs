
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
                FluidEntity fluid = entities[id] as FluidEntity;
                constraint.Init(this, fluid, density, radius);
                constraints[(int)Constraint.Group.kStandard].Add(constraint);

                fluid.lambdas = new float[fluid.particles.Count];
                fluid.deltaPositions = new Vector3[fluid.particles.Count];

                fluid.neighbors = new List<int>[fluid.particles.Count];
                for (int i = 0; i < fluid.particles.Count; ++i)
                {
                    fluid.neighbors[i] = new List<int>();
                }
            }

            return id;
        }

        public override void DestroyEntity(EntityID entityID)
        {

        }

        public override void Start()
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
        protected override void OnSetPlanes(float[,] planes)
        {
            // 计算个平面包围空间的边界，没有的情况下，默认给一个非常远的边界
            float minX, minY, minZ, maxX, maxY, maxZ;
            minX = minY = minZ = -1000.0f;
            maxX = maxY = maxZ = 1000.0f;

            // Left
            Vector3 n = new Vector3(planes[0, 0], planes[0, 1], planes[0, 2]);
            Vector3 p = -n * Mathf.Abs(planes[0, 3]);
            minX = p.x;

            // Right
            n = new Vector3(planes[1, 0], planes[1, 1], planes[1, 2]);
            p = -n * Mathf.Abs(planes[1, 3]);
            maxX = p.x;

            // Top
            n = new Vector3(planes[2, 0], planes[2, 1], planes[2, 2]);
            p = -n * Mathf.Abs(planes[2, 3]);
            maxY = p.y;

            // Bottom
            n = new Vector3(planes[3, 0], planes[3, 1], planes[3, 2]);
            p = -n * Mathf.Abs(planes[3, 3]);
            minY = p.y;

            // Forward
            n = new Vector3(planes[4, 0], planes[4, 1], planes[4, 2]);
            p = -n * Mathf.Abs(planes[4, 3]);
            maxZ = p.z;

            // Backward
            n = new Vector3(planes[5, 0], planes[5, 1], planes[5, 2]);
            p = -n * Mathf.Abs(planes[5, 3]);
            minZ = p.z;

            bound.min = new Vector3(minX, minY, minZ);
            bound.max = new Vector3(maxX, maxY, maxZ);

            boundary = CreateBoundary();
        }

        private EntityID CreateEntity<T>(List<Vector3> positions, float mass, Phase phase) where T : Entity
        {
            float invMass = 1.0f / (mass);

            Entity entity = CreateEntityInternal<T>(positions, invMass, phase);
            EntityID entityID = entities.Count;
            entities.Add(entity);

            // 所有 entity 都受重力影响，所以默认添加重力进去
            Gravity g = new Gravity(gravity);
            entity.AddExternalForce<Gravity>(g);

            return entityID;
        }

        private Entity CreateEntityInternal<T>(List<Vector3> positions, float invMass, Phase phase) where T : Entity
        {
            // 创建内部实体对象，并添加到对象列表中
            Entity entity = Activator.CreateInstance<T>();

            EntityID entityID = entities.Count;

            // 创建实体对应的粒子
            for (int i = 0; i < positions.Count; ++i)
            {
                Particle particle = new Particle(positions[i], invMass, phase, entityID);
                int index = particles.Count;
                entity.particles.Add(index);
                particles.Add(particle);
            }

            return entity;
        }

        private EntityID CreateBoundary()
        {
            BoundaryEntity boundary = Activator.CreateInstance<BoundaryEntity>();

            EntityID entity = entities.Count;
            entities.Add(boundary);

            PointsFromBound source = new PointsFromBound();
            List<Vector3> points = source.CreatePoints(this, bound);

            float density = 1000.0f;
            float diameter = radius * 2.0f;
            float mass = 0.8f * diameter * diameter * diameter * density;
            float invMass = 1.0f / mass;

            for (int i = 0; i < points.Count; ++i)
            {
                Particle p = new Particle(points[i], invMass, Phase.kBoundary, entity);
                int index = particles.Count;
                boundary.particles.Add(index);
                particles.Add(p);
            }

            SPHKernel kernel = new SPHKernel(radius);
            SimpleListNeighborSearcher searcher = new SimpleListNeighborSearcher();
            searcher.Build(particles);

            boundary.psi = new float[boundary.particles.Count];

            for (int i = 0; i < boundary.particles.Count; ++i)
            {
                int index = boundary.particles[i];
                Particle p1 = particles[index];
                float delta = kernel.ZERO;

                searcher.ForeachNearbyPoint(index, radius,
                    (int neighborIndex, int j) =>
                    {
                        if (i != j)
                        {
                            Particle p2 = particles[j];
                            delta += kernel.W(p1.position - p2.position);
                        }
                    });

                float volume = 1.0f / delta;
                boundary.psi[i] = density * volume;
            }

            return entity;
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

                if (particle.phase == Phase.kBoundary)
                {
                    // 流体边界是固定粒子，直接跳过，不受力影响
                    continue;
                }

                if (entityID != particle.body)
                {
                    // 由于 particle 存储是连续的，获取新的 entity 并计算一次外力合力
                    GetEntityAndExternalForce(particle, out entity, out externalForce);
                }

                // 2: apply external forces $\vec{v}_i \Leftarrow \vec{v}_i + \Delta{t} \vec{f}_{ext}(\vec{x}_i)$
                externalForce.ApplyToParticle(dt, particle);

                // 3: predict position $\vec{x}_^* \Leftarrow \vec{x}_i + \Delta{t} \vec{v}_i$
                particle.predictPosition = particle.position + particle.velocity * dt;

                //Log.Debug("PBD", "particle #", i, " prediction position ", particle.predictPosition.ToString(), " position ", particle.position.ToString(), " velocity ", particle.velocity.ToString());

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
            float invDt = 1.0f / dt;
            for (int i = 0; i < particles.Count; ++i)
            {
                Particle p = particles[i];
                if (p.phase == Phase.kBoundary)
                {
                    continue;
                }

                // 24: update velocity vi ⇐ 1∆t(x∗i − xi)
                p.velocity = (p.predictPosition - p.position) * invDt;

                //Log.Debug("PBD", "particle #", i, " prediction position ", p.predictPosition.ToString(), " position ", p.position.ToString(), " velocity ", p.velocity.ToString());

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

        private void FindNeighboringParticles(List<List<Constraint>> constraints, Particle p, int i)
        {
#if false
            SimpleListNeighborSearcher searcher = ObjectsPools.Instance.AcquireObject<SimpleListNeighborSearcher>();
            searcher.Build(particles);

            Particle pi = particles[i];
            if (pi.phase == Phase.kBoundary)
            {
                // 流体边界，不用查找邻居粒子了
                ObjectsPools.Instance.ReleaseObject(searcher);
                return;
            }

            FluidEntity fluid = entities[pi.body] as FluidEntity;
            int idx = i - fluid.particles[0];
            fluid.neighbors[idx].Clear();

            searcher.ForeachNearbyPoint(i, radius,
                (int neighborIndex, int j) =>
                {
                    pi.numberOfNeighbors++;

                    Particle pj = particles[j];

                    if (pi.phase == Phase.kFluid)
                    {
                        // 流体，把邻居粒子记录下来
                        fluid.neighbors[idx].Add(j);
                    }
                });
#else
            Particle pi = particles[i];
            if (pi.phase == Phase.kBoundary)
            {
                // 流体边界，不用查找邻居粒子了
                return;
            }

            FluidEntity fluid = entities[pi.body] as FluidEntity;
            int idx = i - fluid.particles[0];
            fluid.neighbors[idx].Clear();

            float diameter = radius * 2.0f;
            float d2 = diameter * diameter;

            for (int j = i; j < particles.Count; ++j)
            {
                Particle pj = particles[j];
                Vector3 dist = pi.predictPosition - pj.predictPosition;
                if (dist.sqrMagnitude < d2)
                {
                    pi.numberOfNeighbors++;
                    fluid.neighbors[idx].Add(j);

                    if (pj.phase == Phase.kFluid)
                    {
                        pj.numberOfNeighbors++;
                        int index = j - fluid.particles[0];
                        fluid.neighbors[index].Add(i);
                    }
                }
            }
#endif
        }

        private void FindSolidContacts(List<List<Constraint>> constraints, Particle p, int index)
        {
            
        }
#endregion
    }
}
