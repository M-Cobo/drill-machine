using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods {
    public static float ReMap(this float value, float fromMin, float fromMax, float toMin, float toMax) {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }

    public static float ClampAngle(this float angle, float min, float max) {
		float rot = angle;
		angle = (angle > 180) ? angle - 360 : angle;
		if(angle < min) { rot = 360 + min; }
		if(angle > max) { rot = 0 + max; }

		return rot; 
    }

    public static List<GameObject> Shuffle(this List<GameObject> value)
    {
        for (int i = 0; i < value.Count - 1; i++)
        {
            GameObject tempGO = null;
            int rnd = UnityEngine.Random.Range(i, value.Count);

            tempGO = value[rnd];
            value[rnd] = value[i];
            value[i] = tempGO;
        }
        
        return value;
    }
}
