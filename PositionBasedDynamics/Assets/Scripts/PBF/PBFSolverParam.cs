using UnityEngine;

namespace PBF
{
    public class PBFSolverParam
    {
        public int maxNeighbors;
        public int maxParticles;
        public int maxContacts;
        public int gridWidth, gridHeight, gridDepth;
        public int gridSize;

        public int numParticles;
        public int numDiffuse;

        public int numCloth;
        public int numConstraints;

        public Vector3 gravity;
        public Vector3 bounds;

        public int numIterations;
        public float radius;
        public float restDistance;

        public float KPOLY;
        public float SPIKY;
        public float restDensity;
        public float lambdaEps;
        public float vorticityEps;
        public float C;
        public float K;
        public float dqMag;
        public float wQH;
    }
}