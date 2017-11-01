using UnityEngine;
using System.Collections;

namespace NavLinkGeneration
{
    public static class TransformExtensions
    {
        /// <summary>
        /// transform point from local to world unscaled
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
        {
            var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            return localToWorldMatrix.MultiplyPoint3x4(position);
        }
        /// <summary>
        /// transform point from world to local unscaled
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
        {
            var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
            return worldToLocalMatrix.MultiplyPoint3x4(position);
        }
    }
}