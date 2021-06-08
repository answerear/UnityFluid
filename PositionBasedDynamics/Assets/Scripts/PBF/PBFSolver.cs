using System.Collections.Generic;
using UnityEngine;

namespace PBF
{
    public class PBFSolver
    {
        public Vector4[] oldPos;
        public Vector4[] newPos;
        public Vector3[] velocities;
        public int[] phases;
        public float[] densities;

        public Vector4[] diffusePos;
        public Vector3[] diffuseVelocities;


        public int[] neighbors;
        public int[] numNeighbors;
        public int[] gridCells;
        public int[] gridCounters;
        public int[] contacts;
        public int[] numContacts;

        public Vector3[] deltaPs;

        public float[] buffer0;




    }
}