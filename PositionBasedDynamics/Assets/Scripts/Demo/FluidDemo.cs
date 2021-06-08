using System.Collections;
using System.Collections.Generic;
using UnifiedParticlePhysX;
using UnityEngine;
using UFramework;


namespace UPPhysXDemo
{
    public class FluidDemo : MonoBehaviour
    {
        protected Boundary boundary = null;

        protected BoxFluid fluid = null;

        private void Awake()
        {
            UnifiedParticleSystem ups = UnifiedParticleSystem.CreateInstance();
            Solver solver = ups.CreateSolver(true);

            solver.radius = 0.25f;

            GameObject go;

            // 创建流体
            go = ResourceManager.Instance.LoadPrefabSync("PBD", "Prefabs", "BoxFluid");
            go.transform.SetParent(transform);
            fluid = go.GetComponent<BoxFluid>();

            // 物理世界边界
            go = ResourceManager.Instance.LoadPrefabSync("PBD", "Prefabs", "Boundary");
            go.transform.SetParent(transform);
            boundary = go.GetComponent<Boundary>();

            solver.Start();
        }

        private void FixedUpdate()
        {
            UnifiedParticleSystem.Instance.Step(Time.fixedDeltaTime);

            if (fluid != null)
            {
                fluid.UpdateSpheres();
            }
        }

        private void OnDestroy()
        {
            UnifiedParticleSystem.DestroyInstance();
        }
    }
}

