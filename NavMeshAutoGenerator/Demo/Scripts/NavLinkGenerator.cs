using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

namespace NavLinkGeneration
{
    [System.Serializable]
    public class NavLinkGenerator : MonoBehaviour {
        /// <summary>
        /// Width for CreateLinksInLine
        /// </summary>
        //[SerializeField]
        //public int CreateLinksInLineWidth = 5;
        /// <summary>
        /// Distance to search outwards for NavMesh
        /// </summary>
        [SerializeField]
        public float FindNavMeshDistance = 10;
        /// <summary>
        /// Maximum y difference.  larger difference will create one way link
        /// </summary>
        [SerializeField]
        public float MaxJumpHeight = 10;
        /// <summary>
        /// Maximum y difference for a link.  Higher than this and no link will be generated
        /// </summary>
        [SerializeField]
        public float MaxFallHeight = 20;
        /// <summary>
        /// Maximum xz distance for link
        /// </summary>
        [SerializeField]
        public float MaxHorizontalDistance = 7;
        /// <summary>
        /// width of generated NavMesh links.  Too large leads to some weird behavior for NavMesh Agents
        /// </summary>
        [SerializeField]
        public float NavLinkWidth = 2;
        /// <summary>
        /// No link will be created for height differences less than this.
        /// </summary>
        [SerializeField]
        public float NavMeshStepHeight = .37f;
        /// <summary>
        /// Agent type number.  Change this to set which agent type is allowed to use the generated links
        /// </summary>
        [SerializeField]
        public int AgentTypeNumber = 0;
        /// <summary>
        /// Whether the links are bidirectional.  Used with SwitchBidirectional.
        /// </summary>
        protected bool Bidirectional;
        /// <summary>
        /// List of all NavMeshLinks
        /// </summary>
        List<NavMeshLink> linkList;
        /// <summary>
        /// Check if link start and end is distant enough from all links' start and end
        /// </summary>
        /// <param name="linkStart"></param>
        /// <param name="linkEnd"></param>
        /// <returns></returns>
        public bool IsLinkDistantEnough(Vector3 linkStart, Vector3 linkEnd)
        {
            foreach (NavMeshLink oldLink in linkList)
            {
                Transform oldLinkTrans = oldLink.transform;
                //check start points against start points and ends against ends
                if (Vector3.Distance(linkStart, oldLinkTrans.TransformPointUnscaled(oldLink.startPoint)) < NavLinkWidth - .3f
                    && Vector3.Distance(linkEnd, oldLinkTrans.TransformPointUnscaled(oldLink.endPoint)) < NavLinkWidth - .3f)
                {
                    return false;
                }
                //check ends against starts and starts against ends
                if (Vector3.Distance(linkEnd, oldLink.transform.TransformPointUnscaled(oldLink.startPoint)) < NavLinkWidth - .3f
                    && Vector3.Distance(linkStart, oldLinkTrans.TransformPointUnscaled(oldLink.endPoint)) < NavLinkWidth - .3f)
                {
                    return false;
                }
                //check if y distance is greater than step height(should have own method)
                if (Mathf.Abs(linkStart.y - linkEnd.y) < NavMeshStepHeight && Vector3.Distance(linkStart, linkEnd) < 1)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// check if there is an obstruction that cannot be jumped over
        /// </summary>
        /// <param name="linkStart"></param>
        /// <param name="linkEnd"></param>
        /// <returns></returns>
        public bool IsLinkObstructed(Vector3 linkStart, Vector3 linkEnd)
        {
            //use start plus jump height to check for obstructions at jump height
            Vector3 adjStart = linkStart + Vector3.up * MaxJumpHeight;
            //set end to start height
            Vector3 adjEnd = new Vector3(linkEnd.x, adjStart.y, linkEnd.z);
            //linecast between start and end to check if there is an obstruction
            if (Physics.Linecast(adjStart, adjEnd, -1))
            {
                return true;
            }
            //linecast straight down to check for downward obstacles
            if (Physics.Linecast(adjEnd, Vector3.MoveTowards(linkEnd, adjEnd, 1)))
            {
                return true;
            }
            return false;
        }
        public void CreateLink(Vector3 linkCenter, Vector3 linkDirection,float upDistance,float downDistance, Transform targetParent, int agentTypeId)
        {
            //holders for found start and end from GetClosestOnNavMesh
            Vector3 startPoint;
            Vector3 endPoint;
            if (NavHelper.GetClosestOnNavMesh(linkCenter, linkDirection,upDistance,downDistance, FindNavMeshDistance, out startPoint) 
                && NavHelper.GetClosestOnNavMesh(linkCenter, -linkDirection, upDistance, downDistance, FindNavMeshDistance, out endPoint))
            {
                //if start is higher than end then swap.  This keeps bidirectional falling down instead of jumping up
                if (startPoint.y < endPoint.y)
                {
                    Vector3 startHolder = startPoint;
                    startPoint = endPoint;
                    endPoint = startHolder;
                }
                //if link start and end match another link's start and end or end and start then return
                if (!IsLinkDistantEnough(startPoint, endPoint))
                {
                    return;
                }
                //return if obstructed
                if (IsLinkObstructed(startPoint, endPoint))
                {
                    return;
                }
                //grounded vectors to find ground distance
                Vector3 startGrounded = startPoint.GroundedVector();
                Vector3 endGrounded = endPoint.GroundedVector();
                float groundedDistance = Vector3.Distance(startGrounded, endGrounded);
                float yDistance = Mathf.Abs(startPoint.y - endPoint.y);

                //return if yDistance is greater than fall distance//
                if (yDistance > MaxFallHeight)
                {
                    return;
                }
                //return if distance is too far
                if(groundedDistance>MaxHorizontalDistance)
                {
                    return;
                }

                //create NavMeshLink
                NavMeshLink navLink = targetParent.gameObject.AddComponent<NavMeshLink>();
                navLink.startPoint = targetParent.InverseTransformPointUnscaled(startPoint);
                navLink.endPoint = targetParent.InverseTransformPointUnscaled(endPoint);
                navLink.width = NavLinkWidth;
                navLink.agentTypeID = agentTypeId;

                linkList.Add(navLink);
                //if height difference is greater than jump height then only allow falling
                if (yDistance > MaxJumpHeight)
                    navLink.bidirectional = false;
            }
        }
        public void CreateLinksInLine(float width)
        {
            for (float i = .5f; i < width; i++)
            {
                CreateLink(transform.position + transform.right * i, transform.forward, MaxJumpHeight, MaxJumpHeight + MaxFallHeight, transform, 0);
                CreateLink(transform.position - transform.right * i, transform.forward, MaxJumpHeight, MaxJumpHeight + MaxFallHeight, transform, 0);
            }
        }
        /// <summary>
        /// turn Bidirectional off or on for all NavMeshLinks in children
        /// </summary>
        public void SwitchBidirectional()
        {
            foreach (NavMeshLink link in GetComponentsInChildren<NavMeshLink>())
            {
                link.bidirectional = Bidirectional;

                float yDistance = Mathf.Abs(link.startPoint.y - link.endPoint.y);
                if (yDistance > MaxJumpHeight)
                {
                    Debug.Log("jump height too small");
                    link.bidirectional = false;
                }
            }
            Bidirectional = !Bidirectional;
        }
        /// <summary>
        /// swat start and end for all NavMeshLinks in children
        /// </summary>
        public void SwapDirections()
        {
            foreach (NavMeshLink link in GetComponentsInChildren<NavMeshLink>())
            {
                Vector3 start = link.startPoint;
                Vector3 end = link.endPoint;
                link.endPoint = start;
                link.startPoint = end;
            }
        }
        /// <summary>
        /// create NavMeshLinks between two points
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="refDirection"></param>
        /// <param name="targetParent"></param>
        /// <param name="agentTypeID"></param>
        public void CreateLinksBetweenPoints(Vector3 start, Vector3 end, Vector3 refDirection, Transform targetParent, int agentTypeID)
        {

            Vector3 calcNormal = NavHelper.FindFlatPerpVector(refDirection, start, end);
            Vector3 heading = end - start;
            float mag = heading.magnitude;
            Vector3 normHeading = heading.normalized;

            for (float i = (NavLinkWidth / 2) + .1f; i < mag; i += NavLinkWidth)
            {
                //create jump to links
                CreateLink(start + normHeading * i, calcNormal, MaxJumpHeight, MaxJumpHeight + MaxFallHeight + .1f, targetParent, agentTypeID);
                //create fall to links
                CreateLink(start + normHeading * i, calcNormal, .1f, MaxFallHeight + .1f, targetParent, agentTypeID);
                //create mid links
                CreateLink(start + normHeading * i, calcNormal, MaxJumpHeight / 2, MaxJumpHeight/2, targetParent, agentTypeID);
            }

        }
        /// <summary>
        /// create links on a box collider
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="objTransform"></param>
        /// <param name="agentTypeID"></param>
        public void CreateLinksOnBox(BoxCollider collider, Transform objTransform, int agentTypeID)
        {
            Vector3 frontLeft;
            Vector3 frontRight;
            Vector3 backLeft;
            Vector3 backRight;

            frontLeft = objTransform.TransformPoint(new Vector3(.5f, .5f, .5f));
            frontRight = objTransform.TransformPoint(new Vector3(.5f, .5f, -.5f));
            backLeft = objTransform.TransformPoint(new Vector3(-.5f, .5f, -.5f));
            backRight = objTransform.TransformPoint(new Vector3(-.5f, .5f, .5f));

            CreateLinksBetweenPoints(frontLeft, frontRight, transform.TransformPoint(collider.center), collider.transform, agentTypeID);
            CreateLinksBetweenPoints(frontRight, backLeft, transform.TransformPoint(collider.center), collider.transform, agentTypeID);
            CreateLinksBetweenPoints(backLeft, backRight, transform.TransformPoint(collider.center), collider.transform, agentTypeID);
            CreateLinksBetweenPoints(backRight, frontLeft, transform.TransformPoint(collider.center), collider.transform, agentTypeID);

        }
        /// <summary>
        /// create links on all child box colliders
        /// </summary>
        public void CreateLinksOnChildBoxColliders()
        {
            linkList = new List<NavMeshLink>();
            foreach (BoxCollider collider in GetComponentsInChildren<BoxCollider>())
            {
                CreateLinksOnBox(collider, collider.transform, AgentTypeNumber);
            }
        }
        /// <summary>
        /// remove links on all children
        /// </summary>
        public void ClearLinks()
        {
            linkList = new List<NavMeshLink>();
            foreach (NavMeshLink link in GetComponentsInChildren<NavMeshLink>())
            {
                DestroyImmediate(link);
            }
        }
    }
}