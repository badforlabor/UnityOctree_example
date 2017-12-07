using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MyBoundTree))]
public class MyBoundTreeInspector : Editor
{
    public override void OnInspectorGUI()
    {
 	    base.OnInspectorGUI();

        if (GUILayout.Button("随机创建"))
        {
            MyBoundTree root = this.target as MyBoundTree;

            var prefab = Resources.Load("Cube");
            int min = -100, max = 100;
            for (int i = 0; i < 100; i++)
            {
                var go = GameObject.Instantiate(prefab) as GameObject;
                go.transform.parent = root.Cubes;
                var v = new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100));
                var s = new Vector3(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
                go.transform.position = v;
                go.transform.localScale = s;
            }

            EditorApplication.MarkSceneDirty();
        }
    }
}
