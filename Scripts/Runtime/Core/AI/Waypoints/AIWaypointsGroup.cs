using System.Collections.Generic;
using UnityEngine;
using HJ.Tools;

namespace HJ.Runtime
{
    [ExecuteInEditMode]
    public class AIWaypointsGroup : MonoBehaviour
    {
        public List<AIWaypoint> Waypoints = new();

        [Header("Gizmos")]
        public Color GroupColor = Color.red;
        public bool ConnectedGizmos;
        public bool ConnectEndWithStart;
        public bool ConnectAllWithAll;

        private void Update()
        {
            if (transform.childCount == Waypoints.Count)
                return;

            IList<AIWaypoint> newWaypoints = new List<AIWaypoint>();
            foreach (Transform t in transform)
            {
                if(t.gameObject.TryGetComponent(out AIWaypoint waypoint))
                    newWaypoints.Add(waypoint);
            }

            Waypoints = new(newWaypoints);
        }

        void OnDrawGizmos()
        {
            if (Waypoints.Count > 0)
            {
                if(ConnectedGizmos && ConnectAllWithAll)
                {
                    foreach (var curr in Waypoints)
                    {
                        Vector3 currPos = curr.transform.position;
                        Gizmos.color = GroupColor.Alpha(0.5f);
                        Gizmos.DrawSphere(currPos, 0.1f);

                        foreach (var other in Waypoints)
                        {
                            if (curr == other) 
                                continue;

                            Vector3 otherPos = other.transform.position;
                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(currPos, otherPos);
                        }
                    }

                    return;
                }

                if (Waypoints.Count == 1)
                {
                    Gizmos.color = GroupColor.Alpha(0.5f);
                    Vector3 firstPos = Waypoints[0].transform.position;
                    Gizmos.DrawSphere(firstPos, 0.1f);
                }
                else if(Waypoints.Count > 1)
                {
                    for (int i = 0; i < Waypoints.Count - 1; i++)
                    {
                        AIWaypoint curr = Waypoints[i];
                        AIWaypoint next = Waypoints[i + 1];

                        if (curr != null && next != null && ConnectedGizmos)
                        {
                            Vector3 currPos = curr.transform.position;
                            Vector3 nextPos = next.transform.position;

                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(currPos, nextPos);
                        }

                        Gizmos.color = GroupColor.Alpha(0.5f);
                        if (curr != null)
                        {
                            Vector3 currPos = curr.transform.position;
                            Gizmos.DrawSphere(currPos, 0.1f);
                        }

                        if (next != null && (i + 1 >= Waypoints.Count - 1))
                        {
                            Vector3 nextPos = next.transform.position;
                            Gizmos.DrawSphere(nextPos, 0.1f);

                            if (ConnectEndWithStart)
                            {
                                Vector3 firstPos = Waypoints[0].transform.position;
                                Gizmos.color = Color.white;
                                Gizmos.DrawLine(firstPos, nextPos);
                            }
                        }
                    }
                }
            }
        }
    }
}