using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeType{Circle, Box, Squiggle, Zig, Spiral, Polygon}
public class Shape{
    public void MakeShape(){

        // SplinePointPair = SplineUtil.CreateSpline
    }
}

public class Circle : Shape{

}

public class ShapeFactory : MonoBehaviour
{
    public void MakeShape(ShapeType t){
        Shape c = null;

        switch(t){
            case ShapeType.Circle:
                c = new Circle();
            break;

             case ShapeType.Box:
            
            break;

             case ShapeType.Zig:
            
            break;

             case ShapeType.Squiggle:
            
            break;

             case ShapeType.Spiral:
            
            break;
        }

        c.MakeShape();
    }
}
