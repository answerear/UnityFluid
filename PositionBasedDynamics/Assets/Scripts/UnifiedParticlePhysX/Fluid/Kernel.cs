using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnifiedParticlePhysX
{
    internal abstract class Kernel
    {
        public float H { get; protected set; }

        public float ZERO { get; protected set; }

        public Kernel(float h)
        {
            H = h;
        }

        public abstract float W(Vector3 r);

        public abstract float W(float x, float y, float z);

        public abstract Vector3 GradW(Vector3 r);

        public abstract Vector3 GradW(float x, float y, float z);
    }
}
