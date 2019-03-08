using System;
using UnityEngine;
using DacLib.Generic;

namespace DacLib.U3D.Generic
{
    public class GeometryFunc
    {
        public static Vector3 GetHorizontalDirection(Vector3 vec)
        {
            if (vec.x == 0 && vec.z == 0){return Vector3.zero;}
            return new Vector3(vec.x, 0, vec.z).normalized;
        }
        
    }
}

