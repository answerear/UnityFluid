using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    public abstract class Force
    {
        public abstract void ApplyToEntity(double dt, Entity entity);
    }
}
