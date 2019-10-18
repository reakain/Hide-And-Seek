using UnityEngine;

public static class Vector2Extension
{

    //public static Vector2 Rotate(this Vector2 v, float degrees)
    //{
    //    float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
    //    float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

    //    float tx = v.x;
    //    float ty = v.y;

    //    return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
    //}


    /// Rotates the vector by radians. /// 

    //The vector. /// The radians. /// The pivot. /// Modified vector. 
    public static Vector2 RotateRad(this Vector2 vector, float radians, Vector2 pivot = default(Vector2))
    {
        var sin = Mathf.Sin(radians);
        var cos = Mathf.Cos(radians);
        vector -= pivot;
        vector.Set(cos*vector.x - sin *vector.y, sin *vector.x + cos *vector.y);
        vector += pivot;
        return vector;
    }

    ///
    /// Rotates the vector by degrees. /// 
    public static Vector2 Rotate(this Vector2 vector, float degrees, Vector2 pivot = default(Vector2))
    {
        return RotateRad(vector, degrees * Mathf.Deg2Rad, pivot);
    } 
}