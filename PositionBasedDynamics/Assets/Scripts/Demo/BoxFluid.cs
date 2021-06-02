using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnifiedParticlePhysX;


namespace UPPhysXDemo
{
    [Serializable]
    public class BoxFluid : MonoBehaviour
    {
        public Material material = null;

        public Bounds bound;

        public float density = 1000.0f;

        protected int fluidEntity = Solver.INVALID_ENTITY;

        protected List<Transform> spheres = new List<Transform>();

        public void UpdateSpheres()
        {
            Solver solver = UnifiedParticleSystem.Instance.solvers[0];
            List<Vector3> positions = solver.GetEntityPositions(fluidEntity);

            for (int i = 0; i < positions.Count; ++i)
            {
                spheres[i].position = positions[i];
            }
        }

        private void Awake()
        {
            Solver solver = UnifiedParticleSystem.Instance.solvers[0];
            CreateParticles(solver);
        }

        private void CreateParticles(Solver solver)
        {
            float radius = solver.radius;
            float diameter = radius * 2.0f;
            int numX = (int)(bound.size.x / diameter);
            int numY = (int)(bound.size.y / diameter);
            int numZ = (int)(bound.size.z / diameter);

            List<Vector3> positions = new List<Vector3>();

            for (int z = 0; z < numZ; ++z)
            {
                for (int y = 0; y < numY; ++y)
                {
                    for (int x = 0; x < numX; ++x)
                    {
                        Vector3 pos = Vector3.zero;

                        pos.x = diameter * x + bound.min.x + diameter;
                        pos.y = diameter * y + bound.min.y + diameter;
                        pos.z = diameter * z + bound.min.z + diameter;
                        positions.Add(pos);

                        Transform xform = CreateSphere(pos, diameter);
                        spheres.Add(xform);
                    }
                }
            }

            fluidEntity = solver.CreateFluid(positions, density);
        }

        private Transform CreateSphere(Vector3 pos, float diameter)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = pos;
            sphere.transform.localScale = new Vector3(diameter, diameter, diameter);
            sphere.GetComponent<Collider>().enabled = false;
            sphere.GetComponent<MeshRenderer>().material = material;

            sphere.transform.SetParent(transform);
            return sphere.transform;
        }
    }
}

