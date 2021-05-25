using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    public class UnifiedParticleSystem : Singleton<UnifiedParticleSystem>
    {
        public const float kEpsilon = 0.0001f;

        private List<Solver> solvers = new List<Solver>();

        public UnifiedParticleSystem()
        {

        }

        public void Startup()
        {
            ObjectsPools.CreateInstance();
        }

        public void Shutdown()
        {
            ObjectsPools.DestroyInstance();
        }

        public Solver CreateSolver(bool isCPUSolver = true)
        {
            Solver solver = null;

            if (isCPUSolver)
            {
                solver = new SolverCPU();
            }
            else
            {
                solver = new SolverGPU();
            }

            solvers.Add(solver);

            return solver;
        }

        public void DestroySolver(Solver solver)
        {
            
        }
    }
}
