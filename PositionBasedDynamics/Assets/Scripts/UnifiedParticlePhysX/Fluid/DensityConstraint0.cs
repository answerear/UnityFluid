
// #define USE_GRID_HASH

using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    internal class DensityConstraint0 : Constraint, IPoolObject
    {
        private const float RELAXATION = 0.01f;

        public FluidEntity entity { get; protected set; }

        public float restDensity { get; protected set; }

        private float DQ = 0.0f;

        private float DQ2 = 0.0f;

        private Kernel kernel = null;

#if USE_GRID_HASH
        private PointHashGridNeighborSearcher searcher = null;
#else
        private SimpleListNeighborSearcher searcher = null;
#endif

        private List<int>[] neighbors = null;

        private float[] lambdas = null;

        public DensityConstraint0()
        {
            entity = null;
            restDensity = 1000.0f;
            searcher = null;
            lambdas = null;
        }

        ~DensityConstraint0()
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

            kernel = new SPHKernel(h);

            DQ = 0.2f * kernel.H;
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

#if true
        public override void Project()
        {
            if (solver == null || !(solver is SolverCPU))
            {
                return;
            }

            SolverCPU s = solver as SolverCPU;
            searcher.Build(s.particles);

            for (int i = 0; i < entity.particles.Count; ++i)
            {
                int index = entity.particles[i];
                Particle p1 = s.particles[index];

                neighbors[i].Clear();

                searcher.ForeachNearbyPoint(index, solver.radius,
                    (int neighborIndex, int j) =>
                    {
                        neighbors[i].Add(j);
                    });
            }
        }
#else
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
            Vector3[] deltas = new Vector3[entity.particles.Count];

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

                    deltas[i] = grad / restDensity;
                }
            }

            for (int i = 0; i < entity.particles.Count; ++i)
            {
                int index = entity.particles[i];
                Particle p = s.particles[index];
                Log.Debug("PBD", "density particle #", i, " ", p.predictPosition.ToString(), " Delta X ", deltas[i].ToString());
                p.predictPosition += deltas[i] / (neighbors[i].Count + p.numberOfNeighbors);
            }
        }
#endif

        public override void UpdateCounts()
        {
            // 密度约束不用计算粒子受约束的数量
        }

#if false
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
#endif

        private float ComputePBFDensity(List<Particle> particles, FluidEntity fluid, int i)
        {
            float density = fluid.mass * kernel.ZERO;

            Particle pi = particles[fluid.particles[i]];

            for (int j = 0; j < neighbors[i].Count; ++i)
            {
                int index = neighbors[i][j];

                if (i != index)
                {
                    Particle pj = particles[index];
                    float mj = 1.0f / pj.inverseMass;

                    if (pj.phase == Phase.kRigidbody || pj.phase == Phase.kBoundary)
                    {
                        density += solver.solidPressure * mj * kernel.W(pi.predictPosition - pj.predictPosition);
                    }
                    else if (pj.phase == Phase.kFluid)
                    {
                        density += mj * kernel.W(pi.predictPosition - pj.predictPosition);
                    }
                }
            }

            float densityErr = Mathf.Max(density, restDensity) - restDensity;
            return density;
        }

        private void ComputePBFLagrangeMultiplier(FluidEntity fluid, Particle pi, int i, float di)
        {
            SolverCPU s = solver as SolverCPU;

            float lambda = 0.0f;
            float eps = 1.0e-6f;
            float C = Mathf.Max(di / restDensity - 1.0f, 1.0f);

            if (C != 0.0f)
            {
                float sumGradC2 = 0.0f;
                Vector3 gradCi = Vector3.zero;

                for (int j = 0; j < neighbors[i].Count; ++j)
                {
                    int index = neighbors[i][j];
                    Particle pj = s.particles[index];

                    if (i != index)
                    {
                        // $ \nabla_{\vec{p}_k} C_i = - nabla_{\vec{p}_k} W(\vec{p}_i - \vec{p}_j, h) / \rho_0 \quad \text{if} k = j $
                        Vector3 gradCj = Vector3.zero;
                        if (pj.phase == Phase.kRigidbody || pj.phase == Phase.kBoundary)
                        {
                            gradCj = -1.0f * solver.solidPressure * pj.inverseMass * kernel.GradW(pi.predictPosition - pj.predictPosition) / restDensity;
                            sumGradC2 += gradCj.sqrMagnitude;
                        }
                        else if (pj.phase == Phase.kFluid)
                        {
                            gradCj = -1.0f * pj.inverseMass * kernel.GradW(pi.predictPosition - pj.predictPosition) / restDensity;
                            sumGradC2 += gradCj.sqrMagnitude;
                        }
                        // $ \nabla_{\vec{p}_k} C_i = \sum_j \nabla_{\vec{p}_k} W(\vec{p}_i - \vec{p}_j, h) \quad \text{if} k = i $
                        gradCi -= gradCj;
                    }
                }

                // 最后把所有邻居梯度方累加进去，当成 k = i 的情况
                sumGradC2 += gradCi.sqrMagnitude;

                // 计算 lambda
                lambda = -C / (sumGradC2 + eps);
            }
            else 
            {
                lambda = 0.0f;
            }

            pi.lambda = lambda;
        }

        private Vector3 SolveDensityConstraint(FluidEntity fluid, Particle pi, int i)
        {
            SolverCPU s = solver as SolverCPU;

            Vector3 deltaPi = Vector3.zero;
            float invDensity = 1.0f / fluid.density;

            for (int j = 0; j < neighbors[i].Count; ++j)
            {
                int index = neighbors[i][j];
                Particle pj = s.particles[index];

            }

            return deltaPi;
        }
    } 
}
