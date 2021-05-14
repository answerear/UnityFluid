using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    public class Particle
    {
        /// <summary>
        /// 当前位置
        /// </summary>
        public Vector3 position { get; set; }

        /// <summary>
        /// 模拟过程中的预测位置
        /// </summary>
        public Vector3 predictPosition { get; set; }

        /// <summary>
        /// 当前速度
        /// </summary>
        public Vector3 velocity { get; set; }

        /// <summary>
        /// 模拟过程中受的外力
        /// </summary>
        public Vector3 force { get; set; }

        /// <summary>
        /// 质量的倒数，为 0 时是无穷大，可以用于表示静止物体
        /// </summary>
        public float inverseMass { get; set; }

        /// <summary>
        /// 用于标识不同的物体，相同的 phase 不会处理碰撞
        /// </summary>
        public Phase phase { get; set; }

        /// <summary>
        /// 粒子所属的物体对象的索引
        /// </summary>
        public int body { get; set; }

        public Particle(Vector3 pos, float invMass, Phase ph, int entity)
        {
            position = new Vector3(pos.x, pos.y, pos.z);
            predictPosition = new Vector3(pos.x, pos.y, pos.z);
            velocity = new Vector3();
            force = new Vector3();
            inverseMass = invMass;
            phase = ph;
            body = entity;
        }
    }
}
