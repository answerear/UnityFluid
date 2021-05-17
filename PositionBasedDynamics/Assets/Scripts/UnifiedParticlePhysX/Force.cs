using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    public class Force
    {
        public Vector3 value;

        public Force()
        {
            value.x = value.y = value.z = 0.0f;
        }

        public virtual void ApplyToParticle(float dt, Particle particle)
        {
            particle.velocity = particle.velocity + value * dt;
        }
    }
}
