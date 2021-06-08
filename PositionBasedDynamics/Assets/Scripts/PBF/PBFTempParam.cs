using System.Collections.Generic;
using UnityEngine;

namespace PBF
{
    public class PBFTempParam
    {
        public List<Vector4> positions = new List<Vector4>();
        public List<Vector3> velocities = new List<Vector3>();
        public List<int> phases = new List<int>();

        public void CreateParticleGrid(Vector3 lower, Vector3 dims, float radius)
        {
            for (int x = 0; x < dims.x; x++)
            {
                for (int y = 0; y < dims.y; y++)
                {
                    for (int z = 0; z < dims.z; z++)
                    {
                        Vector3 pos = lower + new Vector3((float)(x), (float)(y), (float)(z)) * radius;
                        positions.Add(new Vector4(pos.x, pos.y, pos.z, 1.0f));
                        velocities.Add(Vector3.zero);
                        phases.Add(0);
                    }
                }
            }
        }
    }
}