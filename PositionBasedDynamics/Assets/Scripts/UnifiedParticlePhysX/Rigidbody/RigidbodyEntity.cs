using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    public class RigidbodyEntity : Entity
    {
        public List<SDFData> sdf = new List<SDFData>();

        public SDFData GetSDFData(int index)
        {
            return sdf[index - particles[0]];
        }
    }
}
