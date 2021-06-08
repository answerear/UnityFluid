using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    public class PointsFromBound
    {
        public PointsFromBound()
        {
        }

        public List<Vector3> CreatePoints(Solver solver, Bounds bound)
        {
            float diameter = solver.radius * 2.0f;
            int numX = (int)(bound.size.x / diameter);
            int numY = (int)(bound.size.y / diameter);
            int numZ = (int)(bound.size.z / diameter);

            int size = (numX + 2) * (numY + 2) * (numZ + 2);
            List<Vector3> points = new List<Vector3>(size);

            for (int z = -1; z < numZ + 1; z++)
            {
                for (int y = -1; y < numY + 1; y++)
                {
                    for (int x = -1; x < numX + 1; x++)
                    {
                        Vector3 pos;
                        pos.x = diameter * (float)x + bound.min.x + solver.radius;
                        pos.y = diameter * (float)y + bound.min.y + solver.radius;
                        pos.z = diameter * (float)z + bound.min.z + solver.radius;

                        if (!bound.Contains(pos))
                        {
                            points.Add(pos);
                        }
                    }
                }
            }

            return points;
        }
    }
}
