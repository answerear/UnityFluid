using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnifiedParticlePhysX
{
    using EntityID = System.Int32;

    internal sealed class SolverGPU : Solver
    {
        public override EntityID CreateRigidbody(List<Vector3> positions, float mass)
        {
            throw new NotImplementedException();
        }

        public override EntityID CreateFluid(List<Vector3> positions, float mass)
        {
            throw new NotImplementedException();
        }

        public override void DestroyEntity(EntityID entityID)
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Step(float dt, int substeps)
        {
            throw new NotImplementedException();
        }

        public override List<Vector3> GetEntityPositions(int entity)
        {
            throw new NotImplementedException();
        }
    }
}
