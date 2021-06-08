using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnifiedParticlePhysX
{
    internal class CubicKernel : Kernel
    {
        protected float invRadius;

        protected float K;

        protected float L;

        public CubicKernel(float h)
            : base(h)
        {
            invRadius = 1.0f / H;

            float H3 = H * H * H;
            K = 8.0f / (Mathf.PI * H3);
            L = 48.0f / (Mathf.PI * H3);

            ZERO = W(0.0f, 0.0f, 0.0f);
        }

        public override float W(Vector3 r)
        {
            return Cubic(r);
        }

        public override float W(float x, float y, float z)
        {
            return Cubic(new Vector3(x, y, z));
        }

        public override Vector3 GradW(Vector3 r)
        {
            return GradCubic(r);
        }

        public override Vector3 GradW(float x, float y, float z)
        {
            return GradCubic(new Vector3(x, y, z));
        }

        protected float Cubic(Vector3 r)
        {
            float w = 0.0f;
            float d = r.magnitude;
            float q = d * invRadius;

            if (q <= 1.0f)
            {
                if (q <= 0.5f)
                {
                    float q2 = q * q;
                    float q3 = q2 * q;
                    w = K * (6.0f * q3 - 6.0f * q2 + 1.0f);
                }
                else
                {
                    float factor = 1.0f - q;
                    w = K * 2.0f * (factor * factor * factor);
                }
            }

            return w;
        }

        protected Vector3 GradCubic(Vector3 r)
        {
            Vector3 w = Vector3.zero;
            float d = r.magnitude;
            float q = d * invRadius;

            if (q <= 1.0f)
            {
                if (d > 1.0e-6)
                {
                    Vector3 gradq = r * (1.0f / (d * H));
                    if (q <= 0.5f)
                    {
                        w = L * q * (3.0f * q - 2.0f) * gradq;
                    }
                    else
                    {
                        float factor = 1.0f - q;
                        w = L * (-factor * factor) * gradq;
                    }
                }
            }

            return w;
        }
    }
}
