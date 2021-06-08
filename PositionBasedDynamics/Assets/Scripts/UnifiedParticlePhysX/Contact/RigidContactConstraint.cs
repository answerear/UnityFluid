using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    internal sealed class RigidContactConstraint : ContactConstraint, IPoolObject
    {
        public RigidbodyEntity rigidbody;

        public RigidContactConstraint()
        {
            rigidbody = null;
            distance = 0.0f;
            normal.x = normal.y = normal.z = 0.0f;
        }

        public void Init(Solver s, RigidbodyEntity entity, int index1, int index2, bool stable)
        {
            Init(s, index1, index2, stable);
            rigidbody = entity;
        }

        public new void OnRecycle()
        {
            Init(null, null, -1, -1, false);
        }

        public override void Project()
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
                distance = 2 * solver.radius - len;

                if (distance < UnifiedParticleSystem.kEpsilon)
                {
                    // 半径之和跟两个粒子距离非常接近，表示没有接触，不用处理，直接返回
                    return;
                }

                normal = x12 / len;
            }
            else
            {
                // 两个距离值均小于零，表示在刚体内部，选择分离距离和分离方向，公式：
                // $ d= \text{min}(|\phi_i|, |\phi_j|)
                //
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
                    distance = sdf1.distance;
                    normal = sdf1.gradient;
                }
                else
                {
                    distance = sdf2.distance;
                    normal = sdf2.gradient;
                }

                if (distance < solver.radius * 2 + UnifiedParticleSystem.kEpsilon)
                {
                    // 边界粒子，跟内部粒子要分开处理
                    if (InitBoundary(p1, p2, solver.radius))
                    {
                        return;
                    }
                }

                float w = p1.inverseMass + p2.inverseMass;
                Vector3 offset = distance * normal;
                Vector3 dx1 = -p1.inverseMass / w * offset / p1.numberOfNeighbors;
                Vector3 dx2 = p2.inverseMass / w * offset / p2.numberOfNeighbors;

                // 更新位置, 即：
                // 13: update $\vec{x}_i = \vec{x}_i + \Delta \vec{x}_i / n$
                p1.position += dx1;
                p2.position += dx2;

                // 计算摩擦力并施加到粒子上

                // 计算基于 normal 的切向量，公式：
                // \Delta \vec{x}_{\perp} = [(\vec{x}_i^* - \vec{x}_i) - (\vec{x}_j^* - \vec{x}_j)] \perp \vec{n}
                Vector3 n = normal.normalized;
                Vector3 dx12 = (p1.predictPosition - p1.position) - (p2.predictPosition - p2.position);

                // 因为 n 长度是1，所以这里点积后就是在 n 方向上的投影长度
                // 长度 * 法向量构成一个向量，位移向量和法向量相减得切向量
                Vector3 dxt = dx12 - Vector3.Dot(dx12, n) * n;

                float len = dxt.magnitude;
                if (len < UnifiedParticleSystem.kEpsilon)
                {
                    // 切向量太小，几乎不存在
                    return;
                }

                // 公式：
                // \Delta \vec{x}_i = \frac{w_i}{w_i + w_j}
                // \begin{cases}
                // \Delta \vec{x}_{\perp} \quad & \parallel \Delta \vec{x}_{\perp} \parallel < \mu_s d \\
                // \Delta \vec{x}_{\perp} \cdot \text{min} \left(\frac{\mu_k d}{\parallel \Delta \vec{x}_{\perp}\parallel}, 1 \right) \quad &otherwise
                // \end{cases}
                Vector3 dx;
                if (len < solver.staticFriction)
                {
                    dx = dxt;
                }
                else
                {
                    dx = dxt * Mathf.Min(solver.dynamicFriction * distance / len, 1.0f);
                }

                dx1 = dx * p1.inverseMass / w;
                dx2 = dx * p2.inverseMass / w;
                p1.position -= dx1;
                p2.position += dx2;

                p1.predictPosition -= dx1;
                p2.predictPosition += dx2;
            }
        }

        private bool InitBoundary(Particle p1, Particle p2, float radius)
        {
            // $$
            // \vec{n^*}_{ij} =
            // \begin{cases}
            // \vec{x}_{ij} - 2(\vec{x}_{ij} \cdot \vec{n}_{ij}) \vec{n}_{ij} \quad &\vec{x}_{ij} \cdot \vec{n}_{ij} < 0 \\
            // \vec{x}_{ij} \quad &otherwise
            // \end{cases}
            // $$
            
            Vector3 x12 = p1.predictPosition - p2.predictPosition;
            float len = x12.magnitude;
            distance = 2 * radius - len;
            if (distance < UnifiedParticleSystem.kEpsilon)
            {
                return true;
            }

            x12 = (len > UnifiedParticleSystem.kEpsilon ? x12 / len : Vector3.up);
            float dot = Vector3.Dot(x12, normal);

            if (dot < 0)
            {
                normal = x12 - 2.0f * dot * normal;
            }
            else
            {
                normal = x12;
            }

            return false;
        }
    }
}
