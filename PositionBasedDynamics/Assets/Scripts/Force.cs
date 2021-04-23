using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PositionBasedDynamics
{
    public abstract class Force
    {
        public abstract void ApplyForce(double dt, Entity entity);
    }
}
