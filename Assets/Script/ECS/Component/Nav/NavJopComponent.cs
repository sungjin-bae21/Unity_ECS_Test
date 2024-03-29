using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace NavJob.Components
{

    public enum AgentStatus
    {
        Idle = 0,
        PathQueued = 1,
        Moving = 2,
        Paused = 4
    }

    [GenerateAuthoringComponent]
    public struct NavAgent : IComponentData
    {
        public bool nextWayPoint;
        public float stoppingDistance;
        public float moveSpeed;
        public float acceleration;
        public float rotationSpeed;
        public int areaMask;
        public float3 destination { get; set; }
        public float currentMoveSpeed { get; set; }
        public int queryVersion { get; set; }

        [GhostField]
        public AgentStatus status;
        public float3 position;
        public float3 nextPosition { get; set; }
        public Quaternion rotation { get; set; }
         public float remainingDistance { get; set; }
        public float3 currentWaypoint { get; set; }
        public int nextWaypointIndex { get; set; }
        public int totalWaypoints { get; set; }

        public NavAgent(
            float3 position,
            Quaternion rotation,
            float stoppingDistance = 0f,
            float moveSpeed = 4f,
            float acceleration = 1f,
            float rotationSpeed = 360f,
            int areaMask = -1
        )
        {
            this.nextWayPoint = false;
            this.stoppingDistance = stoppingDistance;
            this.moveSpeed = moveSpeed;
            this.acceleration = acceleration;
            this.rotationSpeed = rotationSpeed;
            this.areaMask = areaMask;
            destination = Vector3.zero;
            currentMoveSpeed = 0;
            queryVersion = 0;
            status = AgentStatus.Idle;
            this.position = position;
            this.rotation = rotation;
            nextPosition = new float3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            remainingDistance = 0;
            currentWaypoint = Vector3.zero;
            nextWaypointIndex = 0;
            totalWaypoints = 0;
        }
    }

}