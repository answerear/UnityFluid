using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    /// <summary>
    /// 接触约束，用于判断两个粒子均非刚体接触的情况下
    /// $$
    /// \Delta{\vec{x}_i} = -\frac{w_i}{w_i + w_j} (d \cdot \vec{n}_{ij})
    /// \\
    /// \Delta{\vec{x}_j} = \frac{w_j}{w_i + w_j} (d \cdot \vec{n}_{ij})
    /// $$
    /// 其中 d 取两个粒子重叠部分，即 $d = \parallel \vec{x}_i - \vec{x}_j \parallel - 2 r$
    /// 
    /// </summary>
    internal class ContactConstraint : Constraint, IPoolObject
    {
        /// <summary>
        /// 一个要判断接触点的索引
        /// </summary>
        public int particleIndex1 { get; protected set; }

        /// <summary>
        /// 一个要判断接触点的索引
        /// </summary>
        public int particleIndex2 { get; protected set; }

        public bool stabile { get; protected set; }

        protected float distance;
        protected Vector3 normal;

        public ContactConstraint()
        {
            particleIndex1 = particleIndex2 = -1;
        }

        public void Init(Solver s, int index1, int index2, bool stable)
        {
            Init(s);
            particleIndex1 = index1;
            particleIndex2 = index2;
            stabile = stable;
        }

        public void OnRecycle()
        {
            Init(null, -1, -1, false);
        }

        public override void Project()
        {
            if (solver == null || !(solver is SolverCPU) || particleIndex1 == -1 || particleIndex1 == particleIndex2)
            {
                return;
            }

            SolverCPU solverCPU = solver as SolverCPU;
            var particles = solverCPU.particles;

            Particle p1 = particles[particleIndex1];
            Particle p2 = particles[particleIndex2];

            Vector3 n = p1.position - p2.position;

            float d = n.magnitude - 2 * solver.radius;
            if (d > 0)
            {
                // 超出两个粒子接触距离，直接退出
                return;
            }

            // $$
            // \Delta{\vec{x}_i} = -\frac{w_i}{w_i + w_j} (d \cdot \vec{n}_{ij})
            // \\
            // \Delta{\vec{x}_j} = \frac{w_j}{w_i + w_j} (d \cdot \vec{n}_{ij})
            // $$

            n /= n.magnitude;

            float w = p1.inverseMass + p2.inverseMass;
            Vector3 offset = d * n;

            Vector3 dx1 = -p1.inverseMass / w * offset / p1.numberOfNeighbors;
            Vector3 dx2 = p2.inverseMass / w * offset / p2.numberOfNeighbors;

            // 更新
            if (stabile)
            {
                p1.position += dx1;
                p2.position += dx2;
            }

            p1.predictPosition += dx1;
            p2.predictPosition += dx2;
        }

        public override void UpdateCounts()
        {
            if (solver == null || !(solver is SolverCPU) || particleIndex1 == -1 || particleIndex1 == particleIndex2)
            {
                return;
            }

            SolverCPU solverCPU = solver as SolverCPU;
            Particle p1 = solverCPU.particles[particleIndex1];
            Particle p2 = solverCPU.particles[particleIndex2];
            p1.numberOfNeighbors++;
            p2.numberOfNeighbors++;
        }
    }
}
