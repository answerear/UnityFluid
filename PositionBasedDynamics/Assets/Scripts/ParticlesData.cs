using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PositionBasedDynamics
{
    public class ParticlesData
    {
        public List<Vector3> positions { get; protected set; }

        public float radius { get; set; }

        public float diameter { get { return radius * 2.0f; } }

        public ParticlesData(float r)
        {
            radius = r;
        }
    }
}
