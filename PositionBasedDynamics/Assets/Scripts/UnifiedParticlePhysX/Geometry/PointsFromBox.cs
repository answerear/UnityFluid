using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    public class PointsFromBox
    {
        public PointsFromBox()
        {
        }

        public List<Vector3> CreatePoints(Solver solver, Bounds bound)
        {
            float diameter = solver.radius * 2.0f;
            int numX = (int)(bound.size.x / diameter);
            int numY = (int)(bound.size.y / diameter);
            int numZ = (int)(bound.size.z / diameter);

            int size = numX * numY * numZ;
            List<Vector3> points = new List<Vector3>(size);

            for (int z = 0; z < numZ; z++)
            {
                for (int y = 0; y < numY; y++)
                {
                    for (int x = 0; x < numX; x++)
                    {
                        Vector3 pos;
                        pos.x = diameter * (float)x + bound.min.x + solver.radius;
                        pos.y = diameter * (float)y + bound.min.y + solver.radius;
                        pos.z = diameter * (float)z + bound.min.z + solver.radius;
                        points.Add(pos);
                    }
                }
            }

            return points;
        }
    }
}
