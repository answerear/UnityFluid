using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    public class RigidContactConstraint : ContactConstraint, IPoolObject
    {
        public RigidbodyEntity rigidbody;

        private float d;
        private Vector3 n;

        public RigidContactConstraint()
        {
            rigidbody = null;
            d = 0.0f;
            n.x = n.y = n.z = 0.0f;
        }

        public new void OnRecycle()
        {
            base.OnRecycle();
            rigidbody = null;
        }

        public override void project()
        {
            if (solver == null || !(solver is SolverCPU) || particleIndex1 == particleIndex2)
            {
                return;
            }

            SolverCPU solverCPU = solver as SolverCPU;
            var particles = solverCPU.particles;

            Particle p1 = particles[particleIndex1];
            Particle p2 = particles[particleIndex2];

            SDFData sdf1 = rigidbody.GetSDFData(particleIndex1);
            SDFData sdf2 = rigidbody.GetSDFData(particleIndex2);

            if (sdf1.distance > 0 || sdf2.distance > 0)
            {
                // 有一个距离值为正，其实这里应该是有错的，按照理论上物体的 SDF 值应该都是小于等于0
                Vector3 x12 = p2.predictPosition - p1.predictPosition;
                float len = x12.magnitude;
                d = 2 * solver.radius - len;

                if (d < UnifiedParticleSystem.kEpsilon)
                {
                    // 半径之和跟两个粒子距离非常接近，表示没有接触，不用处理，直接返回
                    return;
                }

                n = x12 / len;
            }
            else
            {
                // 两个距离值均小于零，表示在刚体内部，选择分离距离和分离方向
                // $ d= \text{min}(|\phi_i|, |\phi_j|)
                // $$
                // \vec{n}_{ij} =
                // \begin{ cases}
                // \nabla \phi_i \quad &if |\phi_i | < |\phi_j | \\
                // - \nabla \phi_j \quad & otherwise
                // \end{ cases}
                // $$

                if (sdf1.distance < sdf2.distance)
                {
                    // 粒子i的距离值比粒子j的距离值小，d使用粒子i的距离值，方向使用粒子i的梯度方向
                    d = sdf1.distance;
                    n = sdf1.gradient;
                }
                else
                {
                    d = sdf2.distance;
                    n = sdf2.gradient;
                }

                if (d < solver.radius * 2 + UnifiedParticleSystem.kEpsilon)
                {
                    // 边界粒子，跟内部粒子要分开处理
                    if (InitBoundary(p1, p2))
                    {
                        return;
                    }
                }
            }
        }

        private bool InitBoundary(Particle p1, Particle p2)
        {
            // $$
            // \vec{n^*}_{ij} =
            // \begin{ cases}
            // \vec{x}_{ij} - 2(\vec{x}_{ij} \cdot \vec{n}_{ij}) \vec{n}_{ij} \quad &\vec{x}_{ij} \cdot \vec{n}_{ij} < 0 \\
            // \vec{x}_{ij} \quad & otherwise
            // \end{ cases}
            // $$
            Vector3 x12 = p1.predictPosition - p2.predictPosition;

            return false;
        }
    }
}
