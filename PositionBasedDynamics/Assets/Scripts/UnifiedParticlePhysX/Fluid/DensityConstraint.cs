using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    internal class DensityConstraint : Constraint, IPoolObject
    {
        private const float RELAXATION = 1.0e-6f;

        public FluidEntity fluid { get; protected set; }

        public float restDensity { get; protected set; }

        private float DQ = 0.0f;

        private float DQ2 = 0.0f;

        private Kernel kernel = null;

        private SimpleListNeighborSearcher searcher = null;

        //private List<int>[] neighbors = null;

        public DensityConstraint()
        {
            
        }

        public void Init(Solver s, FluidEntity body, float rho0, float h)
        {
            Init(s);

            fluid = body;
            restDensity = rho0;

            float d = h * 2.0f;
            //kernel = new SPHKernel(d);
            kernel = new CubicKernel(d);
            DQ = 0.2f * kernel.H;
            DQ2 = DQ * DQ;

            //SolverCPU solver = s as SolverCPU;

            //searcher = ObjectsPools.Instance.AcquireObject<SimpleListNeighborSearcher>();

            //List<Particle> particles = new List<Particle>(body.particles.Count);
            //for (int i = 0; i < body.particles.Count; ++i)
            //{
            //    particles.Add(solver.particles[body.particles[i]]);
            //}

            //searcher.particles = particles;

            //neighbors = new List<int>[fluid.particles.Count];
            //for (int i = 0; i < fluid.particles.Count; ++i)
            //{
            //    neighbors[i] = new List<int>();
            //}
        }

        public void OnRecycle()
        {
            Init(null, null, 0.0f, 0.0f);
        }

        public override void Project()
        {
            if (solver == null || !(solver is SolverCPU))
            {
                return;
            }

            SolverCPU s = solver as SolverCPU;

            //searcher.Build(s.particles);
            //for (int i = 0; i < fluid.particles.Count; ++i)
            //{
            //    int index = fluid.particles[i];
            //    Particle p1 = s.particles[index];

            //    fluid.neighbors[i].Clear();

            //    searcher.ForeachNearbyPoint(index, solver.radius,
            //        (int neighborIndex, int j) =>
            //        {
            //            neighbors[i].Add(j);
            //        });
            //}

            for (int i = 0; i < fluid.particles.Count; ++i)
            {
                int index = fluid.particles[i];
                float density = ComputePBFDensity(index);
                ComputePBFLagrangeMultiplier(index, density);
            }

            for (int i = 0; i < fluid.particles.Count; ++i)
            {
                int index = fluid.particles[i];
                fluid.deltaPositions[i] = SolveDensityConstraint(index);
            }

            for (int i = 0; i < fluid.particles.Count; ++i)
            {
                int index = fluid.particles[i];
                Particle pi = s.particles[index];
                pi.predictPosition += fluid.deltaPositions[i];
            }
        }

        public override void UpdateCounts()
        {
            
        }

        private float ComputePBFDensity(int i)
        {
            SolverCPU s = solver as SolverCPU;
            Particle pi = s.particles[i];
            float mi = 1.0f / pi.inverseMass;
            float density = mi * kernel.ZERO;

            int idx = i - fluid.particles[0];

            for (int j = 0; j < fluid.neighbors[idx].Count; ++j)
            {
                int index = fluid.neighbors[idx][j];

                if (i != index)
                {
                    Particle pj = s.particles[index];

                    if (pj.phase == Phase.kRigidbody)
                    {
                        // particle j is rigidbody
                        density += solver.solidPressure * 1.0f / pj.inverseMass * kernel.W(pi.predictPosition - pj.predictPosition);
                    }
                    else if (pj.phase == Phase.kBoundary)
                    {
                        // particle j is boundary
                        Debug.Assert(pi.phase != Phase.kBoundary);
                        BoundaryEntity boundary = s.entities[pj.body] as BoundaryEntity;
                        density += boundary.psi[index - boundary.particles[0]] * kernel.W(pi.predictPosition - pj.predictPosition);
                    }
                    else
                    {
                        // particle j is fluid
                        density += 1.0f / pj.inverseMass * kernel.W(pi.predictPosition - pj.predictPosition);
                    }
                }
            }

            float densityErr = Mathf.Max(density, restDensity) - restDensity;
            return density;
        }

        private float ComputePBFLagrangeMultiplier(int i, float density)
        {
            SolverCPU s = solver as SolverCPU;
            Particle pi = s.particles[i];

            float eps = RELAXATION;
            float C = Mathf.Max(density / restDensity - 1.0f, 0.0f);

            float lambda = 0.0f;
            float invDensity = 1.0f / restDensity;

            int idx = i - fluid.particles[0];

            if (C != 0.0f)
            {
                float sumGradC2 = 0.0f;
                Vector3 gradCi = Vector3.zero;

                for (int j = 0; j < fluid.neighbors[idx].Count; ++j)
                {
                    int index = fluid.neighbors[idx][j];

                    if (i != index)
                    {
                        Particle pj = s.particles[index];

                        if (pj.phase == Phase.kBoundary)
                        {
                            // boundary
                            Debug.Assert(pi.phase != Phase.kBoundary);
                            BoundaryEntity boundary = s.entities[pj.body] as BoundaryEntity;
                            Vector3 gradCj = -boundary.psi[index - boundary.particles[0]] * kernel.GradW(pi.predictPosition - pj.predictPosition) * invDensity;
                            sumGradC2 += gradCj.sqrMagnitude;
                            gradCi -= gradCj;
                        }
                        else
                        {
                            // fluid
                            Vector3 gradCj = - kernel.GradW(pi.predictPosition - pj.predictPosition) * invDensity / pj.inverseMass;
                            sumGradC2 += gradCj.sqrMagnitude;
                            gradCi -= gradCj;
                        }
                    }
                }

                sumGradC2 += gradCi.sqrMagnitude;

                lambda = -C / (sumGradC2 + eps);
            }

            fluid.lambdas[idx] = lambda;

            return lambda;
        }

        private Vector3 SolveDensityConstraint(int i)
        {
            SolverCPU s = solver as SolverCPU;
            Particle pi = s.particles[i];
            Vector3 corr = Vector3.zero;

            int idx = i - fluid.particles[0];
            float lambdai = fluid.lambdas[idx];
            float invDensity = 1.0f / restDensity;

            for (int j = 0; j < fluid.neighbors[idx].Count; ++j)
            {
                int index = fluid.neighbors[idx][j];
                Particle pj = s.particles[index];

                if (pj.phase == Phase.kBoundary)
                {
                    Debug.Assert(pi.phase != Phase.kBoundary);
                    BoundaryEntity boundary = s.entities[pj.body] as BoundaryEntity;
                    Vector3 gradCj = -boundary.psi[index - boundary.particles[0]] * invDensity * kernel.GradW(pi.predictPosition - pj.predictPosition);
                    corr -= (lambdai) * gradCj;
                }
                else
                {
                    Vector3 gradCj = -kernel.GradW(pi.predictPosition - pj.predictPosition) * invDensity / pj.inverseMass;
                    float lambdaj = fluid.lambdas[j];
                    corr -= (lambdai + lambdaj) * gradCj;
                }
            }

            return corr;
        }
    }
}
