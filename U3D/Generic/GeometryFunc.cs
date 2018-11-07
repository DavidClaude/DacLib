using System;
using UnityEngine;

namespace DacLib.U3D.Generic
{
    public class GeometryFunc
    {

        /// <summary>
        /// 获取向量在水平面上的单位向量
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector3 GetHorizontalDirection(Vector3 vec)
        {
            if (vec.x == 0 && vec.z == 0)
            {
                return Vector3.zero;
            }
            return new Vector3(vec.x, 0, vec.z).normalized;
        }
    }
}

