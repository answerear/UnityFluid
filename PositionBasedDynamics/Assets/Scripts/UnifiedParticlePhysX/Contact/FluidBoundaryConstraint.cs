using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    internal class FluidBoundaryConstraint : Constraint, IPoolObject
    {
        public int index = -1;

        public void Init(Solver s, int idx)
        {
            Init(s);

            index = idx;
        }

        public void OnRecycle()
        {
            Init(null, -1);
        }

        public override void Project()
        {
            if (solver == null || !(solver is SolverCPU) || index == -1)
            {
                return;
            }
        }

        public override void UpdateCounts()
        {
            
        }
    }
}
