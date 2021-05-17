using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    public class SDFData
    {
        /// <summary>
        /// 点的梯度方向
        /// </summary>
        public Vector3 gradient;

        /// <summary>
        /// 空间点的距离值
        /// </summary>
        public float distance;

        public SDFData()
        {
            gradient = Vector3.zero;
            distance = -1.0f;
        }

        public void Rotate(Quaternion q)
        {
            gradient = q * gradient;
        }
    }
}
