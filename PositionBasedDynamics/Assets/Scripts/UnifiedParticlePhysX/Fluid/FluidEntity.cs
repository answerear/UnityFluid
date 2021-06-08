using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    using EntityID = System.Int32;

    internal class FluidEntity : Entity
    {
        /// <summary>
        /// 流体密度
        /// </summary>
        public float density = 0.0f;

        public float[] lambdas = null;

        public Vector3[] deltaPositions = null;

        public List<int>[] neighbors = null;
    }
}
