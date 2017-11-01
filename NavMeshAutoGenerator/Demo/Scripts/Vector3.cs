using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NavLinkGeneration
{
    public static class Vector3Extensions
    {
        /// <summary>
        /// return vector with a y of 0...probably not neccessary.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 GroundedVector(this Vector3 vector)
        {
            return new Vector3(vector.x, 0, vector.z);
        }
    }
}
