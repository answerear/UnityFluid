
#define USE_SIMPLE_TEST

using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
#if USE_SIMPLE_TEST
    internal sealed class BoundaryConstraint : Constraint, IPoolObject
    {
        public enum Boundary
        {
            kLeft = 0,
            kRight,
            kTop,
            kBottom,
            kForward,
            kBack
        }

        public int index { get; private set; }

        public Boundary boundary { get; private set; }

        public float value { get; private set; }

        public bool stabile { get; private set; }

        public BoundaryConstraint()
        {
            index = -1;
            stabile = false;
        }

        public void Init(Solver s, int idx, Boundary bound, float val, bool stable)
        {
            Init(s);
            index = idx;
            boundary = bound;
            value = val;
            stabile = stable;
        }

        public void OnRecycle()
        {
            Init(null, -1, Boundary.kLeft, 0.0f, false);
        }

        public override void Project()
        {
            if (solver == null || !(solver is SolverCPU) || index == -1)
            {
                return;
            }

            SolverCPU solverCPU = solver as SolverCPU;
            Particle p = solverCPU.particles[index];

            float extra = (p.phase == Phase.kFluid ? Random.Range(0.0f, 1.0f) * 0.003f : 0.0f);
            float d = solver.radius + extra;
            Vector3 n = Vector3.zero;

            switch (boundary)
            {
                case Boundary.kLeft:    // 超越左边界
                    {
                        if (p.predictPosition.x >= value + solver.radius)
                        {
                            return;
                        }

                        p.predictPosition.x = value + d;
                        if (stabile)
                        {
                            p.position.x = value + d;
                        }
                        n = Vector3.right;
                    }
                    break;
                case Boundary.kRight:   // 超越右边界
                    {
                        if (p.predictPosition.x <= value - solver.radius)
                        {
                            return;
                        }

                        p.predictPosition.x = value - d;
                        if (stabile)
                        {
                            p.position.x = value - d;
                        }
                        n = Vector3.left;
                    }
                    break;
                case Boundary.kTop: // 上边界
                    {
                        if (p.predictPosition.y <= value - solver.radius)
                        {
                            return;
                        }

                        p.predictPosition.y = value - d;
                        if (stabile)
                        {
                            p.position.y = value - d;
                        }
                        n = Vector3.down;
                    }
                    break;
                case Boundary.kBottom:  // 下边界
                    {
                        if (p.predictPosition.y >= value + solver.radius)
                        {
                            return;
                        }

                        p.predictPosition.y = value + d;
                        if (stabile)
                        {
                            p.position.y = value + d;
                        }
                        n = Vector3.up;

                        Log.Debug("PBD", "Touch bottom boundary particle #", index, " ", p.predictPosition.ToString(), " ", p.position.ToString());
                    }
                    break;
                case Boundary.kForward: // 前边界
                    {
                        if (p.predictPosition.z <= value - solver.radius)
                        {
                            return;
                        }

                        p.predictPosition.z = value - d;
                        if (stabile)
                        {
                            p.position.z = value - d;
                        }
                        n = Vector3.back;
                    }
                    break;
                case Boundary.kBack:    // 后边界
                    {
                        if (p.predictPosition.z >= value + solver.radius)
                        {
                            return;
                        }

                        p.predictPosition.z = value + d;
                        if (stabile)
                        {
                            p.position.z = value + d;
                        }
                        n = Vector3.forward;
                    }
                    break;
            }

            if (stabile)
            {
                return;
            }

            // 计算基于 normal 的切向量，公式：
            // \Delta \vec{x}_{\perp} = [(\vec{x}_i^* - \vec{x}_i) - (\vec{x}_j^* - \vec{x}_j)] \perp \vec{n}
            Vector3 dx12 = (p.predictPosition - p.position) / p.numberOfNeighbors;

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
            if (len < solver.staticFriction * d)
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
#else
    internal sealed class BoundaryConstraint : Constraint, IPoolObject
    {
        public int index { get; private set; }

        public Plane plane { get; private set; }

        public bool stabile { get; private set; }

        public BoundaryConstraint()
        {
            index = -1;
        }

        public void Init(Solver s, int idx, Vector3 n, float d, bool stable)
        {
            Init(s);
            index = idx;
            plane = new Plane(n, d);
            stabile = stable;
        }

        public void OnRecycle()
        {
            Init(null, -1, Vector3.up, 0.0f, false);
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
            p.predictPosition = pos - plane.normal * d;
            //p.position = p.predictPosition;

            return;

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
#endif
}
