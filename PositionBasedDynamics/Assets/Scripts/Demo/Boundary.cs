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

        protected GameObject goVisible = null;

        private void Start()
        {
            Solver solver = UnifiedParticleSystem.Instance.solvers[0];
            CreateBoundaries(solver);
        }

        private void CreateBoundaries(Solver solver)
        {
            const int numPlanes = 6;
            const float size = 200.0f;
            float[,] planes = new float[numPlanes, 4];
            float minX = -size * 0.5f, minY = 0.0f, minZ = -size*0.5f;
            float maxX = size * 0.5f, maxY = size, maxZ = size * 0.5f;

            Vector3 offset = new Vector3(0, size * 0.5f, 0);

            goVisible = new GameObject("Visible");
            goVisible.transform.SetParent(transform);
            goVisible.SetActive(false);

            // Left
            planes[0, 0] = 1.0f;
            planes[0, 1] = planes[0, 2] = 0.0f;
            planes[0, 3] = minX;
            CreatePlane(planes[0, 0], planes[0, 1], planes[0, 2], planes[0, 3], size, offset);

            // Right
            planes[1, 0] = -1.0f;
            planes[1, 1] = planes[1, 2] = 0.0f;
            planes[1, 3] = maxX;
            CreatePlane(planes[1, 0], planes[1, 1], planes[1, 2], planes[1, 3], size, offset);

            // Top
            planes[2, 0] = planes[2, 2] = 0.0f;
            planes[2, 1] = 1.0f;
            planes[2, 3] = minY;
            CreatePlane(planes[2, 0], planes[2, 1], planes[2, 2], planes[2, 3], size, Vector3.zero);

            // Bottom
            planes[3, 0] = planes[3, 2] = 0.0f;
            planes[3, 1] = -1.0f;
            planes[3, 3] = maxY;
            CreatePlane(planes[3, 0], planes[3, 1], planes[3, 2], planes[3, 3], size, Vector3.zero);

            // Forward
            planes[4, 0] = planes[4, 1] = 0.0f;
            planes[4, 2] = -1.0f;
            planes[4, 3] = maxZ;
            CreatePlane(planes[4, 0], planes[4, 1], planes[4, 2], planes[4, 3], size, offset);

            // Backward
            planes[5, 0] = planes[5, 1] = 0.0f;
            planes[5, 2] = 1.0f;
            planes[5, 3] = minZ;
            CreatePlane(planes[5, 0], planes[5, 1], planes[5, 2], planes[5, 3], size, offset);

            solver.numPlanes = numPlanes;
            solver.planes = planes;
        }

        private void CreatePlane(float A, float B, float C, float D, float size, Vector3 offset)
        {
            Vector3 normal = new Vector3(A, B, C);
            Vector3 pos = -Mathf.Abs(D) * normal + offset;

            Quaternion orient = new Quaternion();
            orient.SetFromToRotation(Vector3.back, -normal);

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plane.transform.SetParent(goVisible.transform);
            plane.transform.position = pos;
            plane.transform.rotation = orient;

            plane.transform.localScale = new Vector3(size, size, size);

            plane.GetComponent<MeshRenderer>().material = material;
        }
    }
}

