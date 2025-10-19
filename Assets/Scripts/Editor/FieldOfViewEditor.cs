using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//SOURCE: https://www.youtube.com/watch?v=rQG9aUWarwE
//Need to find a way to make the cone/angle follow the enemies last facing direction.

[CustomEditor (typeof (FieldOfView))]
public class FieldOfViewEditor : Editor
{

}
/*{
    private void OnSceneGUI()
    {
        FieldOfView fow = (FieldOfView) target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fow.transform.position, Vector3.forward, Vector3.up, 360, fow.viewRadius);
        Vector3 viewAngleA = fow.DirFromAngle(-fow.viewAngle / 2, false);
        Vector3 viewAngleB = fow.DirFromAngle(fow.viewAngle / 2, false);

        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * fow.viewRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * fow.viewRadius);

        foreach (Transform visibleTarget in fow.visibleTargets)
        {
            Handles.DrawLine(fow.transform.position, visibleTarget.position);
        }
    }
}*/
