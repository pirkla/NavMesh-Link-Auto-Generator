using UnityEngine;
using UnityEngine.AI;
namespace NavLinkGeneration
{
    public static class NavHelper
    {
        /// <summary>
        /// offset of navlinks to keep them slightly within NavMesh
        /// </summary>
        public static float NavLinkOffset = .05f;
        /// <summary>
        /// interval to check for NavMesh.  Bigger numbers is finer but takes longer
        /// </summary>
        public static float NavLinkCheckInterval = .2f;
        /// <summary>
        /// get closest point on navmesh in two rays oriented in direction starting at position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <param name="navMeshPoint"></param>
        /// <returns></returns>
        public static bool GetClosestOnNavMesh(Vector3 position, Vector3 direction,float upCheckDist, float downCheckDist, float outwardDistance, out Vector3 navMeshPoint)
        {
            RaycastHit hit;
            NavMeshHit navHit = new NavMeshHit();
            for (float i = .2f; i < outwardDistance; i += NavLinkCheckInterval)
            {
                Physics.Raycast(position + direction * i + Vector3.up * upCheckDist, Vector3.down, out hit, downCheckDist+upCheckDist);
                if (NavMesh.SamplePosition(hit.point, out navHit, .1f, NavMesh.AllAreas))
                {
                    navMeshPoint = navHit.position + direction * NavLinkOffset;
                    if (navMeshPoint.magnitude > .1)
                        return true;
                }
            }
            navMeshPoint = Vector3.zero;
            return false;
        }
        public static bool GetClosestVerticalOnNavMesh(Vector3 position, Vector3 direction, float distance, out Vector3 navMeshPoint)
        {
            RaycastHit hit;
            NavMeshHit navHit = new NavMeshHit();
            for (float i = .2f; i < distance; i += .1f)
            {
                Physics.Raycast(position + direction * i + Vector3.up * 10, Vector3.down, out hit, 12);
                if (NavMesh.SamplePosition(hit.point, out navHit, .1f, NavMesh.AllAreas))
                {
                    navMeshPoint = navHit.position + direction * NavLinkOffset;
                    return true;
                }
            }
            navMeshPoint = Vector3.zero;
            return false;
        }
        /// <summary>
        /// Find normal or perpendicular vector from three points
        /// </summary>
        /// <param name="Up"></param>
        /// <param name="pointB"></param>
        /// <param name="pointC"></param>
        /// <returns></returns>
        public static Vector3 FindFlatPerpVector(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            Vector3 side1 = pointB - pointA;
            Vector3 side2 = pointC - pointA;
            Vector3 perpVector = Vector3.Cross(side1, side2);
            perpVector.y = 0;
            perpVector.Normalize();
            return perpVector;
        }
    }
}
