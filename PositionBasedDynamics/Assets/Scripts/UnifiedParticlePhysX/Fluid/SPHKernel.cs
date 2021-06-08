using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    internal class SPHKernel : Kernel
    {
        protected float H2;

        protected float H6;

        protected float H9;

        public SPHKernel(float h)
            : base(h)
        {
            H2 = h * h;
            H6 = H2 * H2 * H2;
            H9 = H6 * H2 * H;

            ZERO = W(0.0f, 0.0f, 0.0f);
        }

        public override float W(Vector3 r)
        {
            return Poly6(r);
        }

        public override float W(float x, float y, float z)
        {
            return Poly6(new Vector3(x, y, z));
        }

        public override Vector3 GradW(float x, float y, float z)
        {
            return GradSpiky(new Vector3(x, y, z));
        }

        public override Vector3 GradW(Vector3 r)
        {
            return GradSpiky(r);
        }

        protected float Poly6(Vector3 r)
        {
            float w = 0.0f;

            float d = r.magnitude;
            float d2 = d * d;

            if (d <= H)
            {
                float diff = H2 - d2;
                w = (315.0f / (64.0f * Mathf.PI * H9)) * (diff * diff * diff);
            }

            return w;
        }

        protected Vector3 GradSpiky(Vector3 r)
        {
            Vector3 w = Vector3.zero;

            float d = r.magnitude;

            if (d <= H)
            {
                float diff = H - d;
                w = -r.normalized * (45.0f / Mathf.PI * H6) * diff * diff;
            }

            return w;
        }
    }
}
