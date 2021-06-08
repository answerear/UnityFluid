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
        public Vector3 position;

        /// <summary>
        /// 模拟过程中的预测位置
        /// </summary>
        public Vector3 predictPosition;

        /// <summary>
        /// 当前速度
        /// </summary>
        public Vector3 velocity;

        /// <summary>
        /// 质量的倒数，为 0 时是无穷大，可以用于表示静止物体
        /// </summary>
        public float inverseMass;

        /// <summary>
        /// 用于标识不同的物体，相同的 phase 不会处理碰撞
        /// </summary>
        public Phase phase;

        /// <summary>
        /// 粒子所属的物体对象的索引
        /// </summary>
        public int body;

        public float lambda;

        /// <summary>
        /// 当前粒子相接触的粒子数量
        /// </summary>
        public int numberOfNeighbors;

        public Particle(Vector3 pos, float invMass, Phase ph, int entity)
        {
            position = new Vector3(pos.x, pos.y, pos.z);
            predictPosition = new Vector3(pos.x, pos.y, pos.z);
            velocity = Vector3.zero;
            inverseMass = invMass;
            phase = ph;
            body = entity;
            lambda = 0.0f;
            numberOfNeighbors = 0;
        }

        public void FrameInit()
        {
            numberOfNeighbors = 0;
            lambda = 0.0f;
        }
    }
}
