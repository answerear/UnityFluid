using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    internal sealed class BoundaryConstraint : Constraint, IPoolObject
    {
        public int index { get; private set; }

        public Plane plane { get; private set; }

        public BoundaryConstraint()
        {
            index = -1;
        }

        public void Init(Solver s, int idx, Vector3 n, float d)
        {
            Init(s);
            index = idx;
            plane = new Plane(n, d);
        }

        public void OnRecycle()
        {
            Init(null, -1, Vector3.up, 0.0f);
        }

        public override void Project()
        {
            if (solver == null || !(solver is SolverCPU) || index == -1)
            {
                return;
            }

            SolverCPU solverCPU = solver as SolverCPU;
            Particle p = solverCPU.particles[index];

            float extra = 0.0f;
            float d = solver.radius + extra;

            // 获取离平面最近的点
            Vector3 pos = plane.ClosestPointOnPlane(p.predictPosition);
            // 沿平面法线，设置接触边界的点
            p.predictPosition = pos + plane.normal * d;
            p.position = p.predictPosition;

            // 计算基于 normal 的切向量，公式：
            // \Delta \vec{x}_{\perp} = [(\vec{x}_i^* - \vec{x}_i) - (\vec{x}_j^* - \vec{x}_j)] \perp \vec{n}
            Vector3 n = plane.normal.normalized;
            Vector3 dx12 = (p.predictPosition - p.position);

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
                dx = dxt * Mathf.Min(solver.dynamicFriction * d / len, 1.0f);
            }

            p.predictPosition -= dx;
        }

        public override void UpdateCounts()
        {
            if (solver == null || !(solver is SolverCPU) || index == -1)
            {
                return;
            }

            SolverCPU solverCPU = solver as SolverCPU;
            Particle p = solverCPU.particles[index];
            p.numberOfNeighbors++;
        }
    }
}
