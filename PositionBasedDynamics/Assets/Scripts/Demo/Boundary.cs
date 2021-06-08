using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnifiedParticlePhysX;
using UnityEngine;


namespace UPPhysXDemo
{
    [Serializable]
    public class Boundary : MonoBehaviour
    {
        public Material material = null;

        public Bounds bound;

        protected List<Transform> spheres = new List<Transform>();

        private void Awake()
        {
            Solver solver = UnifiedParticleSystem.Instance.solvers[0];
            CreatePlaneBoundaries(solver);
            CreateSphereBoundaries(solver);
        }

        private void Update()
        {
            Solver solver = UnifiedParticleSystem.Instance.solvers[0];
            List<Vector3> positions = solver.GetEntityPositions(solver.boundary);

            for (int i = 0; i < positions.Count; ++i)
            {
                spheres[i].position = positions[i];
            }
        }

        private void CreatePlaneBoundaries(Solver solver)
        {
            const int numPlanes = 6;
            float[,] planes = new float[numPlanes, 4];

            Vector3 size = bound.size;

            float d = size.x * 0.5f;

            Vector3 offset = new Vector3(0, size.y * 0.5f, 0);

            GameObject goVisible = new GameObject("Visible_Plane");
            goVisible.transform.SetParent(transform);
            goVisible.SetActive(false);

            // Left
            Vector3 n = Vector3.right;
            Vector3 p = new Vector3(-d, 0.0f, 0.0f);
            planes[0, 0] = n.x;
            planes[0, 1] = n.y;
            planes[0, 2] = n.z;
            planes[0, 3] = -Vector3.Dot(n, p);
            CreatePlane(goVisible, planes[0, 0], planes[0, 1], planes[0, 2], planes[0, 3], size, offset);

            // Right
            n = Vector3.left;
            p = new Vector3(d, 0.0f, 0.0f);
            planes[1, 0] = n.x;
            planes[1, 1] = n.y;
            planes[1, 2] = n.z;
            planes[1, 3] = -Vector3.Dot(n, p);
            CreatePlane(goVisible, planes[1, 0], planes[1, 1], planes[1, 2], planes[1, 3], size, offset);

            // Top
            n = Vector3.down;
            p = new Vector3(0.0f, 2.0f * d, 0.0f);
            planes[2, 0] = n.x;
            planes[2, 1] = n.y;
            planes[2, 2] = n.z;
            planes[2, 3] = -Vector3.Dot(n, p);
            CreatePlane(goVisible, planes[2, 0], planes[2, 1], planes[2, 2], planes[2, 3], size, Vector3.zero);

            // Bottom
            n = Vector3.up;
            p = Vector3.zero;
            planes[3, 0] = n.x;
            planes[3, 1] = n.y;
            planes[3, 2] = n.z;
            planes[3, 3] = -Vector3.Dot(n, p);
            CreatePlane(goVisible, planes[3, 0], planes[3, 1], planes[3, 2], planes[3, 3], size, Vector3.zero);

            // Forward
            n = Vector3.back;
            p = new Vector3(0.0f, 0.0f, d);
            planes[4, 0] = n.x;
            planes[4, 1] = n.y;
            planes[4, 2] = n.z;
            planes[4, 3] = -Vector3.Dot(n, p);
            CreatePlane(goVisible, planes[4, 0], planes[4, 1], planes[4, 2], planes[4, 3], size, offset);

            // Backward
            n = Vector3.forward;
            p = new Vector3(0.0f, 0.0f, -d);
            planes[5, 0] = n.x;
            planes[5, 1] = n.y;
            planes[5, 2] = n.z;
            planes[5, 3] = -Vector3.Dot(n, p);
            CreatePlane(goVisible, planes[5, 0], planes[5, 1], planes[5, 2], planes[5, 3], size, offset);

            solver.numPlanes = numPlanes;
            solver.planes = planes;
        }

        private void CreatePlane(GameObject parent, float A, float B, float C, float D, Vector3 size, Vector3 offset)
        {
            Vector3 normal = new Vector3(A, B, C);
            Vector3 pos = -Mathf.Abs(D) * normal + offset;

            Quaternion orient = new Quaternion();
            orient.SetFromToRotation(Vector3.back, -normal);

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plane.transform.SetParent(parent.transform);
            plane.transform.position = pos;
            plane.transform.rotation = orient;
            plane.transform.localScale = new Vector3(size.x, size.y, size.z);

            plane.GetComponent<MeshRenderer>().material = material;
        }

        private void CreateSphereBoundaries(Solver solver)
        {
            GameObject goVisible = new GameObject("Visible_Sphere");
            goVisible.transform.SetParent(transform);
            goVisible.SetActive(true);

            List<Vector3> positions = solver.GetEntityPositions(solver.boundary);
            float diameter = solver.radius * 2.0f;

            for (int i = 0; i < positions.Count; ++i)
            {
                Transform xform = CreateSphere(goVisible, positions[i], diameter);
                spheres.Add(xform);
            }
        }

        private Transform CreateSphere(GameObject parent, Vector3 pos, float diameter)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = pos;
            sphere.transform.localScale = new Vector3(diameter, diameter, diameter);
            sphere.GetComponent<Collider>().enabled = false;
            sphere.GetComponent<MeshRenderer>().material = material;

            sphere.transform.SetParent(parent.transform);
            return sphere.transform;
        }
    }
}

