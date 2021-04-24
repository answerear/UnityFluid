using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PositionBasedDynamics
{
    public abstract class Entity 
    {
        public List<Vector3> Positions { get; private set; }

        public List<Vector3> Predicted { get; private set; }

        public List<Vector3> Velocities { get; private set; }

        public List<Constraint> Constraints { get; private set; }

        public Entity(ParticlesData particles, float mass)
        {

        }
    }
}
