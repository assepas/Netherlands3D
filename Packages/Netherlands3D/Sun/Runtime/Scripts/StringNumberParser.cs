using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

public class StringNumberParser : MonoBehaviour
{
    [SerializeField] private IntEvent intEvent;
    [SerializeField] private FloatEvent floatEvent;

    public void ParseToInteger(string integerString)
    {
        if(int.TryParse(integerString, out int parsedInt))
        {
            intEvent.Invoke(parsedInt);
        }
    }
    public void ParseToFloat(string floatString)
    {
        if(float.TryParse(floatString, out float parsedFloat))
        {
            floatEvent.Invoke(parsedFloat);
        }

    }
    
}
