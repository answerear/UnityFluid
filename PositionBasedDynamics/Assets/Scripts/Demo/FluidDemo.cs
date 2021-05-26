using System.Collections;
using System.Collections.Generic;
using UnifiedParticlePhysX;
using UnityEngine;
using UFramework;


namespace UPPhysXDemo
{
    public class FluidDemo : MonoBehaviour
    {
        private void Awake()
        {
            UnifiedParticleSystem ups = UnifiedParticleSystem.CreateInstance();
            Solver solver = ups.CreateSolver(true);

            // 物理世界边界
            GameObject go = ResourceManager.Instance.LoadPrefabSync("PBD", "Prefabs", "Boundary");
            go.transform.SetParent(transform);

            // 创建流体
            CreateFluid(solver);

            solver.Start();
        }

        private void FixedUpdate()
        {
            UnifiedParticleSystem.Instance.Step(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            UnifiedParticleSystem.DestroyInstance();
        }

        

        private void CreateFluid(Solver solver)
        {

        }
    }
}

