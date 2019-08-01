using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public static class StringConvertExtensions
{
    public static Vector3 FromString(this Vector3 vector, string sVector) {
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }
        string[] sArray = sVector.Split(',');
        Vector3 result = new Vector3(
            float.Parse(sArray[0], CultureInfo.InvariantCulture),
            float.Parse(sArray[1], CultureInfo.InvariantCulture),
            float.Parse(sArray[2], CultureInfo.InvariantCulture));

        return result;
    }

    public static Vector2 FromString(this Vector2 vector, string sVector)
    {
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }
        string[] sArray = sVector.Split(',');

        Vector2 result = new Vector2(
            float.Parse(sArray[0], CultureInfo.InvariantCulture),
            float.Parse(sArray[1], CultureInfo.InvariantCulture)
        );

        return result;
    }
}
