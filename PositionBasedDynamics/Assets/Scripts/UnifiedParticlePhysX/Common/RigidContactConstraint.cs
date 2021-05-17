using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    public class RigidContactConstraint : ContactConstraint
    {
        private RigidbodyEntity rigidbody;

        public RigidContactConstraint(List<Particle> points, float restDist, RigidbodyEntity entity)
            : base(points, restDist)
        {
            rigidbody = entity;
        }

        public override void project()
        {
            if (particleIndex1 == particleIndex2)
            {
                return;
            }

            Particle p1 = particles[particleIndex1];
            Particle p2 = particles[particleIndex2];

            SDFData sdf1 = rigidbody.GetSDFData(particleIndex1);
            SDFData sdf2 = rigidbody.GetSDFData(particleIndex2);

            if (sdf1.distance < 0 || sdf2.distance < 0)
            {
                Vector3 p12 = p2.predictPosition - p1.predictPosition;
                float d = restDistance - p12.magnitude;
            }
        }
    }
}
