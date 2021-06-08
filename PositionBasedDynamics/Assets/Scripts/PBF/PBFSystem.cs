using UnityEngine;

namespace PBF
{
    public class PBFSystem
    {
        public PBFSolver mSolver;
        private PBFSolverParam mSp;


        public void Initialize(PBFTempParam tp, PBFSolverParam sp)
        {
            mSolver = new PBFSolver();

            mSp = sp;

            mSolver.oldPos = new Vector4[sp.numParticles];
            mSolver.newPos = new Vector4[sp.numParticles];
            mSolver.velocities = new Vector3[sp.numParticles];
            mSolver.phases = new int[sp.numParticles];
            mSolver.densities = new float[sp.numParticles];

            mSolver.diffusePos = new Vector4[sp.numDiffuse];
            mSolver.diffuseVelocities = new Vector3[sp.numDiffuse];

            mSolver.neighbors = new int[sp.maxNeighbors * sp.numParticles];
            mSolver.numNeighbors = new int[sp.numParticles];
            mSolver.gridCells = new int[sp.numParticles * sp.gridSize];
            mSolver.gridCounters = new int[sp.numParticles];
            mSolver.contacts = new int[sp.numParticles * sp.maxContacts];
            mSolver.numContacts = new int[sp.numParticles];


            mSolver.deltaPs = new Vector3[sp.numParticles];

            mSolver.buffer0 = new float[sp.numParticles];

            for (int i = 0; i < sp.numParticles; ++i)
                mSolver.oldPos[i] = tp.positions[i];

            for (int i = 0; i < sp.numParticles; ++i)
                mSolver.newPos[i] = tp.positions[i];

            for (int i = 0; i < sp.numParticles; ++i)
                mSolver.velocities[i] = tp.velocities[i];

            for (int i = 0; i < sp.numParticles; ++i)
                mSolver.phases[i] = tp.phases[i];
        }

        public float EaseInOutQuad(float t, float b, float c, float d)
        {
            t /= d / 2;
            if (t < 1) 
                return c / 2 * t * t + b;
            t--;
            return -c / 2 * (t * (t - 2) - 1) + b;
        }

        public float WPoly6(Vector3 pi, Vector3 pj)
        {
            Vector3 r = pi - pj;
            float rLen = r.magnitude;
            if (rLen > mSp.radius || rLen == 0)
            {
                return 0;
            }
            return mSp.KPOLY * Mathf.Pow((mSp.radius * mSp.radius - Mathf.Pow(rLen, 2)), 3);
        }

        public Vector3 GradWPoly6(Vector3 pi, Vector3 pj)
        {
            Vector3 r = pi - pj;
            float rLen = r.magnitude;
            if (rLen > mSp.radius || rLen == 0)
            {
                return Vector3.zero;
            }

            float coeff = Mathf.Pow((mSp.radius * mSp.radius) - (rLen * rLen), 2);
            coeff *= -6 * mSp.KPOLY;
            return r * coeff;
        }

        public Vector3 WSpiky(Vector3 pi, Vector3 pj)
        {
            Vector3 r = pi - pj;
            float rLen = r.magnitude;
            if (rLen > mSp.radius || rLen == 0)
            {
                return Vector3.zero;
            }

            float coeff = (mSp.radius - rLen) * (mSp.radius - rLen);
            coeff *= mSp.SPIKY;
            coeff /= rLen;
            return r * -coeff;
        }

        public void Update(float dt)
        {
            for(int i = 0; i < mSp.numParticles; ++i)
                PredictPositions(i, dt);

            for (int i = 0; i < mSp.numParticles; ++i)
                ClearNeighbours(i);

            for (int i = 0; i < mSp.numParticles; ++i)
                ClearGrid(i);

            for (int i = 0; i < mSp.numParticles; ++i)
                UpdateGrid(i);

            for (int i = 0; i < mSp.numParticles; ++i)
                UpdateNeighbours(i);

            for (int j = 0; j < mSp.numIterations; j++)
            {
                for (int i = 0; i < mSp.numParticles; ++i)
                    ClearDeltaPos(i);

                for (int i = 0; i < mSp.numParticles; ++i)
                    ParticleCollisions(i);

                for (int i = 0; i < mSp.numParticles; ++i)
                    ApplyDeltaPos(i, 1);
            }

            //Solve constraints
            // UpdateWater
            for (int j = 0; j < mSp.numIterations; j++)
            {
                //Calculate fluid densities and store in densities
                for (int i = 0; i < mSp.numParticles; ++i)
                    CalcDensities(i);

                //Calculate all lambdas and store in buffer0
                for (int i = 0; i < mSp.numParticles; ++i)
                    CalcLambda(i);

                //calculate deltaP
                for (int i = 0; i < mSp.numParticles; ++i)
                    CalcDeltaPos(i);

                //update position x*i = x*i + deltaPi
                for (int i = 0; i < mSp.numParticles; ++i)
                    ApplyDeltaPos(i, 0);
            }

            //Update velocity, apply vorticity confinement, apply xsph viscosity, update position
            for (int i = 0; i < mSp.numParticles; ++i)
                UpdateVelocities(i, dt);

            //Set new velocity
            for (int i = 0; i < mSp.numParticles; ++i)
                UpdateXSPHVelocities(i, dt);
        }

        public Vector3 GetGridPos(Vector4 pos)
        {
            return new Vector3((int)(pos.x / mSp.radius) % mSp.gridWidth, (int)(pos.y / mSp.radius) % mSp.gridHeight, (int)(pos.z / mSp.radius) % mSp.gridDepth);
        }

        public int GetGridIndex(Vector3 pos)
        {
            return (int)((pos.z * mSp.gridHeight * mSp.gridWidth) + (pos.y * mSp.gridWidth) + pos.x);
        }

        public void PredictPositions(int index, float dt)
        {
            //update velocity vi = vi + dt * fExt
            mSolver.velocities[index] += ((mSolver.newPos[index].w > 0) ? 1 : 0) * mSp.gravity * dt;

            //predict position x* = xi + dt * vi
            Vector3 velocity = mSolver.velocities[index] * dt;
            mSolver.newPos[index] += new Vector4(velocity.x, velocity.y, velocity.z, 0);

            ConfineToBox(index);
        }

        public void ConfineToBox(int index)
        {
            var pos = mSolver.newPos[index];
            var vel = mSolver.velocities[index];

            if (pos.x < 0)
            {
                vel.x = 0;
                pos.x = 0.001f;
            }
            else if (pos.x > mSp.bounds.x)
            {
                vel.x = 0;
                pos.x = mSp.bounds.x - 0.001f;
            }

            if (pos.y < 0)
            {
                vel.y = 0;
                pos.y = 0.001f;
            }
            else if (pos.y > mSp.bounds.y)
            {
                vel.y = 0;
                pos.y = mSp.bounds.y - 0.001f;
            }

            if (pos.z < 0)
            {
                vel.z = 0;
                pos.z = 0.001f;
            }
            else if (pos.z > mSp.bounds.z)
            {
                vel.z = 0;
                pos.z = mSp.bounds.z - 0.001f;
            }

            mSolver.newPos[index] = pos;
            mSolver.velocities[index] = vel;
        }

        public void ClearNeighbours(int index)
        {
            mSolver.numNeighbors[index] = 0;
            mSolver.numContacts[index] = 0;
        }

        public void UpdateNeighbours(int index)
        {
            Vector3 pos = GetGridPos(mSolver.newPos[index]);
            int pIndex;

            for (int z = -1; z < 2; z++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        Vector3 n = new Vector3(pos.x + x, pos.y + y, pos.z + z);
                        if (n.x >= 0 && n.x < mSp.gridWidth && n.y >= 0 && n.y < mSp.gridHeight && n.z >= 0 && n.z < mSp.gridDepth)
                        {
                            int gIndex = GetGridIndex(n);
                            int cellParticles = Mathf.Min(mSolver.gridCounters[gIndex], mSp.maxParticles - 1);
                            for (int i = 0; i < cellParticles; i++)
                            {
                                if (mSolver.numNeighbors[index] >= mSp.maxNeighbors) return;

                                pIndex = mSolver.gridCells[gIndex * mSp.maxParticles + i];

                                Vector4 temp = mSolver.newPos[index] - mSolver.newPos[pIndex];
                                temp.w = 0;
                                if (temp.magnitude <= mSp.radius)
                                {
                                    mSolver.neighbors[(index * mSp.maxNeighbors) + mSolver.numNeighbors[index]] = pIndex;
                                    mSolver.numNeighbors[index]++;
                                    if (mSolver.phases[index] == 0 && mSolver.phases[pIndex] == 1 && mSolver.numContacts[index] < mSp.maxContacts)
                                    {
                                        mSolver.contacts[index * mSp.maxContacts + mSolver.numContacts[index]] = pIndex;
                                        mSolver.numContacts[index]++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ClearGrid(int index)
        {
            mSolver.gridCounters[index] = 0;
        }

        public void UpdateGrid(int index)
        {
            Vector3 pos = GetGridPos(mSolver.newPos[index]);
            int gIndex = GetGridIndex(pos);

            int i = mSolver.gridCounters[gIndex];
            mSolver.gridCounters[gIndex]++;
            i = Mathf.Min(i, mSp.maxParticles - 1);
            mSolver.gridCells[gIndex * mSp.maxParticles + i] = index;
        }

        public void ClearDeltaPos(int index)
        {
            mSolver.deltaPs[index] = Vector3.zero;
            mSolver.buffer0[index] = 0;
        }

        public void ParticleCollisions(int index)
        {
            for (int i = 0; i < mSolver.numContacts[index]; i++)
            {
                int nIndex = mSolver.contacts[index * mSp.maxContacts + i];
                if (mSolver.newPos[nIndex].w == 0) 
                    continue;

                Vector4 temp = mSolver.newPos[index] - mSolver.newPos[nIndex];

                Vector3 dir = new Vector3(temp.x, temp.y, temp.z);
                float len = dir.magnitude;
                float invMass = mSolver.newPos[index].w + mSolver.newPos[nIndex].w;
                Vector3 dp;
                if (len > mSp.radius || len == 0.0f || invMass == 0.0f) 
                    dp = Vector3.zero;
                else 
                    dp = (1 / invMass) * (len - mSp.radius) * (dir / len);

                mSolver.deltaPs[index] -= dp * mSolver.newPos[index].w;
                mSolver.buffer0[index]++;

                mSolver.deltaPs[nIndex].x += dp.x * mSolver.newPos[nIndex].w;
                mSolver.deltaPs[nIndex].y += dp.y * mSolver.newPos[nIndex].w;
                mSolver.deltaPs[nIndex].z += dp.z * mSolver.newPos[nIndex].w;

                mSolver.buffer0[nIndex]++;
            }
        }

        public void ApplyDeltaPos(int index, int flag)
        {
            if (mSolver.buffer0[index] > 0 && flag == 1)
            {
                Vector3 temp = mSolver.deltaPs[index] / mSolver.buffer0[index];
                mSolver.newPos[index] += new Vector4(temp.x, temp.y, temp.z, 0);
            }
            else if (flag == 0)
            {
                Vector3 temp = mSolver.deltaPs[index];

                mSolver.newPos[index] += new Vector4(temp.x, temp.y, temp.z, 0);
            }
        }

        public void CalcDensities(int index)
        {
            if (mSolver.phases[index] != 0)
                return;

            float rhoSum = 0.0f;
            for (int i = 0; i < mSolver.numNeighbors[index]; i++)
            {
                if (mSolver.phases[mSolver.neighbors[(index * mSp.maxNeighbors) + i]] == 0)
                {
                    var p1 = mSolver.newPos[index];
                    var p2 = mSolver.newPos[mSolver.neighbors[(index * mSp.maxNeighbors) + i]];
                    rhoSum += WPoly6(new Vector3(p1.x, p1.y, p1.z), new Vector3(p2.x, p2.y, p2.z));
                }
            }

            mSolver.densities[index] = rhoSum;
        }

        public void CalcLambda(int index)
        {
            if (mSolver.phases[index] != 0)
                return;

            float densityConstraint = (mSolver.densities[index] / mSp.restDensity) - 1;
            Vector3 gradientI = Vector3.zero;
            float sumGradients = 0.0f;
            for (int i = 0; i < mSolver.numNeighbors[index]; i++)
            {
                if (mSolver.phases[mSolver.neighbors[(index * mSp.maxNeighbors) + i]] == 0)
                {
                    //Calculate gradient with respect to j
                    var p1 = mSolver.newPos[index];
                    var p2 = mSolver.newPos[mSolver.neighbors[(index * mSp.maxNeighbors) + i]];
                    Vector3 gradientJ = WSpiky(new Vector3(p1.x, p1.y, p1.z), new Vector3(p2.x, p2.y, p2.z)) / mSp.restDensity;

                    //Add magnitude squared to sum
                    sumGradients += Mathf.Pow(gradientJ.magnitude, 2);
                    gradientI += gradientJ;
                }
            }

            //Add the particle i gradient magnitude squared to sum
            sumGradients += Mathf.Pow(gradientI.magnitude, 2);
            mSolver.buffer0[index] = (-1 * densityConstraint) / (sumGradients + mSp.lambdaEps);
        }

        public void CalcDeltaPos(int index)
        {
            if (mSolver.phases[index] != 0)
                return;

            mSolver.deltaPs[index] = Vector3.zero;

            Vector3 deltaP = Vector3.zero;
            for (int i = 0; i < mSolver.numNeighbors[index]; i++)
            {
                if (mSolver.phases[mSolver.neighbors[(index * mSp.maxNeighbors) + i]] == 0)
                {
                    float lambdaSum = mSolver.buffer0[index] + mSolver.buffer0[mSolver.neighbors[(index * mSp.maxNeighbors) + i]];
                    float sCorr = sCorrCalc(mSolver.newPos[index], mSolver.newPos[mSolver.neighbors[(index * mSp.maxNeighbors) + i]]);
                    var p1 = mSolver.newPos[index];
                    var p2 = mSolver.newPos[mSolver.neighbors[(index * mSp.maxNeighbors) + i]]; 
                    deltaP += WSpiky(new Vector3(p1.x, p1.y, p1.z), new Vector3(p2.x, p2.y, p2.z)) * (lambdaSum + sCorr);
                }
            }

            mSolver.deltaPs[index] = deltaP / mSp.restDensity;
        }

        public float sCorrCalc(Vector4 pi, Vector4 pj)
        {
            //Get Density from WPoly6
            float corr = WPoly6(new Vector3(pi.x, pi.y, pi.z), new Vector3(pj.x, pj.y, pj.z)) / mSp.wQH;
            corr *= corr * corr * corr;
            return -mSp.K * corr;
        }

        public void UpdateVelocities(int index, float dt)
        {
            if (mSolver.phases[index] != 0)
                return;

            //confineToBox(newPos[index], velocities[index]);

            //set new velocity vi = (x*i - xi) / dt
            var temp = mSolver.newPos[index] - mSolver.oldPos[index];
            mSolver.velocities[index] = (new Vector3(temp.x, temp.y, temp.z)) / dt;

            //apply vorticity confinement
            mSolver.velocities[index] += VorticityForce(index) * dt;

            //apply XSPH viscosity
            mSolver.deltaPs[index] = XsphViscosity(index);

            //update position xi = x*i
            mSolver.oldPos[index] = mSolver.newPos[index];
        }

        public Vector3 Eta(int index, float vorticityMag)
        {
            Vector3 eta = Vector3.zero;
            for (int i = 0; i < mSolver.numNeighbors[index]; i++)
            {
                if (mSolver.phases[mSolver.neighbors[(index * mSp.maxNeighbors) + i]] == 0)
                {
                    Vector4 p1 = mSolver.newPos[index];
                    Vector4 p2 = mSolver.newPos[mSolver.neighbors[(index * mSp.maxNeighbors) + i]];
                    eta += WSpiky(new Vector3(p1.x, p1.y, p1.z), new Vector3(p2.x, p2.y, p2.z)) * vorticityMag;
                }
                    
            }

            return eta;
        }

        public Vector3 VorticityForce(int index)
        {
            Vector3 omega = Vector3.zero;

            for (int i = 0; i < mSolver.numNeighbors[index]; i++)
            {
                if (mSolver.phases[mSolver.neighbors[(index * mSp.maxNeighbors) + i]] == 0)
                {
                    Vector3 velocityDiff = mSolver.velocities[mSolver.neighbors[(index * mSp.maxNeighbors) + i]] - mSolver.velocities[index];
                    Vector4 p1 = mSolver.newPos[index];
                    Vector4 p2 = mSolver.newPos[mSolver.neighbors[(index * mSp.maxNeighbors) + i]];
                    Vector3 gradient = WSpiky(new Vector3(p1.x, p1.y, p1.z), new Vector3(p2.x, p2.y, p2.z));
                    omega += Vector3.Cross(velocityDiff, gradient);
                }
            }

            float omegaLength = omega.magnitude;
            if (omegaLength == 0.0f)
            {
                //No direction for eta
                return Vector3.zero;
            }

            Vector3 etaVal = Eta(index, omegaLength);
            if (etaVal.x == 0 && etaVal.y == 0 && etaVal.z == 0)
            {
                //Particle is isolated or net force is 0
                return Vector3.zero;
            }

            Vector3 n = etaVal.normalized;

            return (Vector3.Cross(n, omega) * mSp.vorticityEps);
        }

        public Vector3 XsphViscosity(int index)
        {
            Vector3 visc = Vector3.zero;
            for (int i = 0; i < mSolver.numNeighbors[index]; i++)
            {
                if (mSolver.phases[mSolver.neighbors[(index * mSp.maxNeighbors) + i]] == 0)
                {
                    Vector3 velocityDiff = mSolver.velocities[mSolver.neighbors[(index * mSp.maxNeighbors) + i]] - mSolver.velocities[index];
                    Vector4 p1 = mSolver.newPos[index];
                    Vector4 p2 = mSolver.newPos[mSolver.neighbors[(index * mSp.maxNeighbors) + i]];
                    velocityDiff *= WPoly6(new Vector3(p1.x, p1.y, p1.z), new Vector3(p2.x, p2.y, p2.z));
                    visc += velocityDiff;
                }
            }

            return visc * mSp.C;
        }

        public void UpdateXSPHVelocities(int index, float dt)
        {
            if (mSolver.phases[index] != 0)
                return;

            mSolver.velocities[index] += mSolver.deltaPs[index] * dt;
        }
    }
}
