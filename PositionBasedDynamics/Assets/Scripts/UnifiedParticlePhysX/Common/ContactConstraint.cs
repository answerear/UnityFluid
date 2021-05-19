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
    public class ContactConstraint : Constraint, IPoolObject
    {
        /// <summary>
        /// 一个要判断接触点的索引
        /// </summary>
        public int particleIndex1;

        /// <summary>
        /// 一个要判断接触点的索引
        /// </summary>
        public int particleIndex2;

        public Solver solver;

        public ContactConstraint()
        {
            solver = null;
            particleIndex1 = particleIndex2 = -1;
        }

        public void OnRecycle()
        {
            solver = null;
            particleIndex1 = particleIndex2 = -1;
        }

        public override void project()
        {
            if (solver == null || !(solver is SolverCPU) || particleIndex1 == particleIndex2)
            {
                return;
            }

            SolverCPU solverCPU = solver as SolverCPU;
            var particles = solverCPU.particles;

            Particle particle1 = particles[particleIndex1];
            Particle particle2 = particles[particleIndex2];

            Vector3 p1 = particle1.position;
            Vector3 p2 = particle2.position;

            Vector3 n = p1 - p2;

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

            float w = particle1.inverseMass + particle2.inverseMass;
            Vector3 offset = d * n;

            Vector3 deltaP1 = -particle1.inverseMass / w * offset / particle1.numberOfNeighbors;
            Vector3 deltaP2 = particle2.inverseMass / w * offset / particle2.numberOfNeighbors;

            particle1.position += deltaP1;
            particle2.position += deltaP2;

            particle1.predictPosition += deltaP1;
            particle2.predictPosition += deltaP2;
        }
    }
}
