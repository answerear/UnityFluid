using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PBF;

namespace UPPhysXDemo
{
    [Serializable]
    public class PBFScene : MonoBehaviour
    {
        private PBFSystem mSystem;
        private PBFSolverParam mSp;

        protected List<Transform> spheres = new List<Transform>();
        public Material material = null;

        private void Awake()
        {
            Init();
            CreateParticles();
        }

        private int cnt = 0;
        private void FixedUpdate()
        {
            if(cnt%20 ==0)
            {
                mSystem.Update(0.02f);
                UpdateSpheres();
            }
            cnt++;
        }


        public void UpdateSpheres()
        {
            for (int i = 0; i < mSp.numParticles; ++i)
            {
                Vector4 temp = mSystem.mSolver.oldPos[i];
                spheres[i].position = new Vector3(temp.x, temp.y, temp.z);
            }
        }

        public void Init()
        {
            const float radius = 0.1f;
            const float restDistance = radius * 0.5f;

            Vector3 lower = new Vector3(0.0f, 0.1f, 0.0f);
            Vector3 dims = new Vector3(10, 10, 10);

            PBFTempParam tp = new PBFTempParam();
            tp.CreateParticleGrid(lower, dims, restDistance);

            PBFSolverParam sp = new PBFSolverParam();
            sp.radius = radius;
            sp.restDistance = restDistance;
            sp.numIterations = 4;
            sp.numDiffuse = 1024 * 2048;
            sp.numParticles = (int)(tp.positions.Count);
            sp.numCloth = 0;
            sp.numConstraints = 0;
            sp.gravity = new Vector3(0, -9.8f, 0);
            sp.bounds = dims * radius;
            sp.gridWidth = (int)(sp.bounds.x / radius);
            sp.gridHeight = (int)(sp.bounds.y / radius);
            sp.gridDepth = (int)(sp.bounds.z / radius);
            sp.gridSize = sp.gridWidth * sp.gridHeight * sp.gridDepth;
            sp.maxContacts = 10;
            sp.maxNeighbors = 50;
            sp.maxParticles = 50;
            sp.restDensity = 6378.0f;
            sp.lambdaEps = 600.0f;
            sp.vorticityEps = 0.0001f;
            sp.C = 0.01f; //0.0025f;
            sp.K = 0.00001f;
            sp.KPOLY = 315.0f / (64.0f * Mathf.PI * Mathf.Pow(radius, 9));
            sp.SPIKY = 45.0f / (Mathf.PI * Mathf.Pow(radius, 6));
            sp.dqMag = 0.2f * radius;
            sp.wQH = sp.KPOLY * Mathf.Pow((radius * radius - sp.dqMag * sp.dqMag), 3);

            mSp = sp;

            mSystem = new PBFSystem();
            mSystem.Initialize(tp, sp);
        }

        private void CreateParticles()
        {
            for(int i = 0; i < mSp.numParticles; ++i)
            {
                Vector4 temp = mSystem.mSolver.oldPos[i];
                Transform xform = CreateSphere(new Vector3(temp.x, temp.y, temp.z), mSp.radius);
                spheres.Add(xform);
            }
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

