
// #define USE_GRID_HASH

using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    internal class DensityConstraint : Constraint, IPoolObject
    {
        private const float RELAXATION = 0.01f;

        public FluidEntity entity { get; protected set; }

        public float restDensity { get; protected set; }

        public float H { get; protected set; }


        private float H2 = 0.0f;

        private float H6 = 0.0f;

        private float H9 = 0.0f;

        private float DQ = 0.0f;

        private float DQ2 = 0.0f;

#if USE_GRID_HASH
        private PointHashGridNeighborSearcher searcher = null;
#else
        private SimpleListNeighborSearcher searcher = null;
#endif

        private List<int>[] neighbors = null;

        private float[] lambdas = null;

        public DensityConstraint()
        {
            entity = null;
            restDensity = 1000.0f;
            searcher = null;
            lambdas = null;
            H = 0.0f;
        }

        ~DensityConstraint()
        {
            if (searcher != null)
            {
                ObjectsPools.Instance.ReleaseObject(searcher);
                searcher = null;
            }
        }

        public void Init(Solver s, FluidEntity body, float rho0, float h)
        {
            Init(s);

            entity = body;
            restDensity = rho0;
            H = h;
            H2 = h * h;
            H6 = H2 * H2 * H2;
            H9 = H6 * H2 * H;

            DQ = 0.2f * H;
            DQ2 = DQ * DQ;

            SolverCPU solver = s as SolverCPU;

#if USE_GRID_HASH
            searcher = ObjectsPools.Instance.AcquireObject<PointHashGridNeighborSearcher>();
            searcher.gridSpacing = solver.radius * 2;
            Vector3 diff = solver.bound.max - solver.bound.min;
            searcher.resolution.x = (int)(diff.x / searcher.gridSpacing) + 1;
            searcher.resolution.y = (int)(diff.y / searcher.gridSpacing) + 1;
            searcher.resolution.z = (int)(diff.z / searcher.gridSpacing) + 1;
#else
            searcher = ObjectsPools.Instance.AcquireObject<SimpleListNeighborSearcher>();
#endif

            List<Particle> particles = new List<Particle>(body.particles.Count);
            for (int i = 0; i < body.particles.Count; ++i)
            {
                particles.Add(solver.particles[body.particles[i]]);
            }

            searcher.particles = particles;

            lambdas = new float[entity.particles.Count];
            neighbors = new List<int>[entity.particles.Count];
            for (int i = 0; i < entity.particles.Count; ++i)
            {
                neighbors[i] = new List<int>();
            }
        }

        public void OnRecycle()
        {
            ObjectsPools.Instance.ReleaseObject(searcher);
            searcher = null;
        }

        public override void Project()
        {
            if (solver == null || !(solver is SolverCPU))
            {
                return;
            }

            SolverCPU s = solver as SolverCPU;

            searcher.Build(entity.particles);
            for (int i = 0; i < entity.particles.Count; ++i)
            {
                int index = entity.particles[i];
                Particle p1 = s.particles[index];

                neighbors[i].Clear();

                searcher.ForeachNearbyPoint(index, solver.radius,
                    (int neighborIndex, int j) =>
                    {
                        neighbors[neighborIndex].Add(j);
                    });
            }
            

            for (int i = 0; i < entity.particles.Count; ++i)
            {
                int index = entity.particles[i];
                Particle p1 = s.particles[index];

                float rho_i = 0.0f;
                float magnitude2 = 0.0f;
                Vector3 grad_i = Vector3.zero;

                for (int j = 0; j < neighbors[i].Count; ++j)
                {
                    if (index != j)
                    {
                        // 根据附近粒子的密度加权计算自己的密度
                        Particle p2 = s.particles[j];

                        if (p2.inverseMass == 0)
                        {
                            // 忽略不动的
                            continue;
                        }

                        Vector3 dist = p1.predictPosition - p2.predictPosition;
                        float r = dist.magnitude;
                        float r2 = dist.sqrMagnitude;
                        float rho_j = W(r, r2) / p2.inverseMass;
                        if (p2.phase == Phase.kRigidbody)
                        {
                            rho_j *= solver.solidPressure;
                        }

                        rho_i += rho_j;

                        Vector3 grad_j = -WGradient(dist, r, r2) / restDensity;
                        magnitude2 += grad_j.sqrMagnitude;
                        grad_i += (p2.phase == Phase.kRigidbody ? solver.solidPressure : 1.0f) * -grad_j;
                    }
                }

                // 计算自己的密度和别人的梯度和
                //searcher.ForeachNearbyPoint(index, solver.radius,
                //    (int neighborIndex, int j) =>
                //    {
                //        if (index != j)
                //        {
                //            // 根据附近粒子的密度加权计算自己的密度
                //            Particle p2 = s.particles[j];

                //            if (p2.inverseMass == 0)
                //            {
                //                // 忽略不动的
                //                return;
                //            }

                //            // 先缓存邻居粒子
                //            neighbors[i].Add(j);

                //            Vector3 dist = p1.predictPosition - p2.predictPosition;
                //            float r = dist.magnitude;
                //            float r2 = dist.sqrMagnitude;
                //            float rho_j = W(r, r2) / p2.inverseMass;
                //            if (p2.phase == Phase.kRigidbody)
                //            {
                //                rho_j *= solver.solidPressure;
                //            }

                //            rho_i += rho_j;

                //            Vector3 grad_j = -WGradient(dist, r, r2) / restDensity;
                //            magnitude2 += grad_j.sqrMagnitude;
                //            grad_i += (p2.phase == Phase.kRigidbody ? solver.solidPressure : 1.0f) * -grad_j;
                //        }
                //    });

                // 自己的梯度
                rho_i += W(0, 0) / p1.inverseMass;
                magnitude2 += grad_i.sqrMagnitude;

                // 计算拉格朗日乘子
                float lambda = -(rho_i / restDensity - 1) / (magnitude2 + RELAXATION);
                lambdas[i] = lambda;
            }

            // 计算修正位移
            for (int i = 0; i < entity.particles.Count; ++i)
            {
                int index = entity.particles[i];
                Particle p1 = s.particles[index];

                Vector3 grad = Vector3.zero;

                if (neighbors[i].Count > 0)
                {
                    for (int j = 0; j < neighbors[i].Count; ++j)
                    {
                        if (index == j)
                            continue;

                        Particle p2 = s.particles[neighbors[i][j]];
                        Vector3 dist = p1.predictPosition - p2.predictPosition;
                        float r = dist.magnitude;
                        float r2 = dist.sqrMagnitude;
                        Vector3 grad_j = WGradient(dist, r, r2);
                        float k = solver.surfaceTension;
                        float n = 4.0f;
                        float s_corr = -k * Mathf.Pow(W(r, r2) / W(DQ, DQ2), n);
                        grad += (lambdas[i] + lambdas[j] + s_corr) * grad_j;
                    }

                    Vector3 diff = grad / restDensity;
                    p1.predictPosition += diff / (neighbors[i].Count + p1.numberOfNeighbors);
                }
            }
        }

        public override void UpdateCounts()
        {
            // 密度约束不用计算粒子受约束的数量
        }

        private float W(float r, float r2)
        {
            return Poly6(r, r2);
        }

        private Vector3 WGradient(Vector3 dist, float r, float r2)
        {
            return SpikyGradient(dist, r);
        }

        private float Poly6(float r, float r2)
        {
            float w = 0.0f;

            if (r <= H)
            {
                float diff = H2 - r2;
                w = (315.0f / (64.0f * Mathf.PI * H9)) * (diff * diff * diff);
            }

            return w;
        }

        private Vector3 SpikyGradient(Vector3 dist, float r)
        {
            Vector3 ret = Vector3.zero;

            if (r <= H)
            {
                float diff = H - r;
                ret = -dist.normalized * (45.0f / Mathf.PI * H6) * diff * diff;
            }

            return ret;
        }
    }
}
