using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    internal class SimpleListNeighborSearcher : PointNeighborSearcher, IPoolObject
    {
        public List<Particle> particles;

        public List<int> indices;

        public SimpleListNeighborSearcher()
        {
            indices = new List<int>();
        }

        public override void Build(List<Particle> points)
        {
            indices.Clear();

            if (points.Count == 0)
            {
                return;
            }

            particles = points;
        }

        public override void ForeachNearbyPoint(int origin, float radius, Action<int, int> handler)
        {
            Particle p1 = particles[origin];
            float r2 = radius * radius;

            for (int i = 0; i < indices.Count; ++i)
            {
                Particle p2 = particles[i];
                float d2 = (p2.predictPosition - p1.predictPosition).sqrMagnitude;

                if (d2 < r2)
                {
                    Callback<int, int> callback = ObjectsPools.Instance.AcquireObject<Callback<int, int>>();
                    callback.Arg1 = i;
                    callback.Arg2 = indices[i];
                    callback.Handler = handler;
                    callback.Run();
                    ObjectsPools.Instance.ReleaseObject(callback);
                }
            }
        }

        public void OnRecycle()
        {
            
        }
    }
}
