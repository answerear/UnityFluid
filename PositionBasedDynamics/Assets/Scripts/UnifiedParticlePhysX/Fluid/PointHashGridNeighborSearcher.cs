using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnifiedParticlePhysX
{
    internal sealed class PointHashGridNeighborSearcher : PointNeighborSearcher, IPoolObject
    {
        public List<Particle> particles;

        public Vector3Int resolution;
        public float gridSpacing;

        public List<int> indices { get; protected set; }
        public List<List<int>> buckets { get; protected set; }

        #region 构造函数
        public PointHashGridNeighborSearcher()
            : this(null, 0, 0, 0, 0.0f)
        {

        }

        public PointHashGridNeighborSearcher(List<Particle> points, Vector3Int res, float spacing)
            : this(points, res.x, res.y, res.z, spacing)
        {
        }

        public PointHashGridNeighborSearcher(List<Particle> points, int resolutionX, int resolutionY, int resolutionZ, float spacing)
        {
            particles = points;

            resolution.x = resolutionX;
            resolution.y = resolutionY;
            resolution.z = resolutionZ;

            gridSpacing = spacing;

            indices = new List<int>();
            buckets = new List<List<int>>();
        }
        #endregion

        #region 重写接口
        public void OnRecycle()
        {
            
        }

        public override void Build(List<Particle> points)
        {
            //buckets.Clear();
            //indices.Clear();

            //if (points.Count == 0)
            //{
            //    return;
            //}

            //// Allocate memory chunks
            //int size = resolution.x * resolution.y * resolution.z;
            //buckets.Capacity = size;
            //indices.Capacity = points.Count;

            //for (int i = 0; i < size; ++i)
            //{
            //    buckets.Add(new List<int>());
            //}

            //// Put points into buckets
            //for (int i = 0; i < points.Count; ++i)
            //{
            //    indices.Add(points[i]);
            //    int key = GetHashKeyFromPosition(points[i]);
            //    buckets[key].Add(i);
            //}
        }

        public override void ForeachNearbyPoint(int origin, float radius, Action<int, int> handler)
        {
            if (buckets.Count == 0)
            {
                return;
            }

            Particle o = particles[origin];
            int[] nearbyKeys = GetNearbyKeys(origin);
            float r2 = radius * radius;
            for (int i = 0; i < 8; ++i)
            {
                var b = buckets[nearbyKeys[i]];
                int numberOfPointsInBucket = b.Count;

                for (int j = 0; j < numberOfPointsInBucket; ++j)
                {
                    int pointIndex = b[j];
                    Particle p = particles[indices[pointIndex]];
                    float d2 = (p.predictPosition - o.predictPosition).sqrMagnitude;
                    if (d2 < r2)
                    {
                        Callback<int, int> callback = ObjectsPools.Instance.AcquireObject<Callback<int, int>>();
                        callback.Arg1 = pointIndex;
                        callback.Arg2 = indices[pointIndex];
                        callback.Handler = handler;
                        callback.Run();
                        ObjectsPools.Instance.ReleaseObject(callback);
                    }
                }
            }
        }
        #endregion

        #region 私有接口
        private int GetHashKeyFromPosition(int index)
        {
            Vector3Int bucketIdx = GetBucketIndex(index);
            return GetHashKeyFromBucketIndex(bucketIdx);
        }

        private Vector3Int GetBucketIndex(int index)
        {
            Particle p = particles[index];
            Vector3 pos = p.predictPosition / gridSpacing;
            return Vector3Int.FloorToInt(pos);
        }

        private int GetHashKeyFromBucketIndex(Vector3Int bucketIdx)
        {
            Vector3Int wrappedIdx = bucketIdx;
            wrappedIdx.x = bucketIdx.x % resolution.x;
            wrappedIdx.y = bucketIdx.y % resolution.y;
            wrappedIdx.z = bucketIdx.z % resolution.z;

            if (wrappedIdx.x < 0)
            {
                wrappedIdx.x += resolution.x;
            }

            if (wrappedIdx.y < 0)
            {
                wrappedIdx.y += resolution.y;
            }

            if (wrappedIdx.z < 0)
            {
                wrappedIdx.z += resolution.z;
            }

            return (wrappedIdx.z * resolution.y + wrappedIdx.y) * resolution.x + wrappedIdx.x;
        }

        private int[] GetNearbyKeys(int index)
        {
            Vector3Int originIdx = GetBucketIndex(index);
            Vector3Int[] nearbyBucketIndices = new Vector3Int[8];

            for (int i = 0; i < 8; ++i)
            {
                nearbyBucketIndices[i] = originIdx;
            }

            Vector3 pos = particles[index].predictPosition;

            if ((originIdx.x + 0.5f) * gridSpacing <= pos.x)
            {
                nearbyBucketIndices[4].x += 1;
                nearbyBucketIndices[5].x += 1;
                nearbyBucketIndices[6].x += 1;
                nearbyBucketIndices[7].x += 1;
            }
            else
            {
                nearbyBucketIndices[4].x -= 1;
                nearbyBucketIndices[5].x -= 1;
                nearbyBucketIndices[6].x -= 1;
                nearbyBucketIndices[7].x -= 1;
            }

            if ((originIdx.y + 0.5f) * gridSpacing <= pos.y)
            {
                nearbyBucketIndices[2].y += 1;
                nearbyBucketIndices[3].y += 1;
                nearbyBucketIndices[6].y += 1;
                nearbyBucketIndices[7].y += 1;
            }
            else
            {
                nearbyBucketIndices[2].y -= 1;
                nearbyBucketIndices[3].y -= 1;
                nearbyBucketIndices[6].y -= 1;
                nearbyBucketIndices[7].y -= 1;
            }

            if ((originIdx.z + 0.5f) * gridSpacing <= pos.z)
            {
                nearbyBucketIndices[1].z += 1;
                nearbyBucketIndices[3].z += 1;
                nearbyBucketIndices[5].z += 1;
                nearbyBucketIndices[7].z += 1;
            }
            else
            {
                nearbyBucketIndices[1].z -= 1;
                nearbyBucketIndices[3].z -= 1;
                nearbyBucketIndices[5].z -= 1;
                nearbyBucketIndices[7].z -= 1;
            }

            int[] nearbyKeys = new int[8];
            for (int i = 0; i < 8; i++)
            {
                nearbyKeys[i] = GetHashKeyFromBucketIndex(nearbyBucketIndices[i]);
            }

            return nearbyKeys;
        }
        #endregion
    }
}
