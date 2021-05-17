using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    public class Gravity : Force
    {
        public Gravity(Vector3 val)
        {
            value.x = val.x;
            value.y = val.y;
            value.z = val.z;
        }
    }
}

