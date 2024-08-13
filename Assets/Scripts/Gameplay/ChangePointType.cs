using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePointType : ActivatedBehaviour
{

    [SerializeField] PointTypes pointType;

    public override void DoBehaviour()
    {
        GetComponent<Point>().SetPointType(pointType);
    }
}
