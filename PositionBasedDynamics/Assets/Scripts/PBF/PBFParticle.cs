using UnityEngine;

namespace PBF
{
    public class PBFParticle
    {
        public Vector3 oldPos;
        public Vector3 newPos;
        public Vector3 velocity;
        public float invMass;
        int phase;
    }
}