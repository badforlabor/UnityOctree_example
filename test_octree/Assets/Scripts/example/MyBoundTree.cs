using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyBoundTree : MonoBehaviour 
{

    public Transform Cubes;
    public BoxCollider Tester;

    BoundsOctree<GameObject> boundsTree = null;

	// Use this for initialization
	void Start () 
    {
        boundsTree = new BoundsOctree<GameObject>(15, transform.position, 1, 1.25f);

        for (int i = 0; i < Cubes.childCount; i++)
        {
            var t = Cubes.GetChild(i);
            boundsTree.Add(t.gameObject, t.gameObject.GetComponent<BoxCollider>().bounds);
        }
	}
    void Update() 
    {
        if (boundsTree == null || Tester == null)
            return;

        for (int i = 0; i < Cubes.childCount; i++)
        {
            Cubes.GetChild(i).GetComponent<Renderer>().enabled = false;
        }

        List<GameObject> collides = new List<GameObject>();
        boundsTree.GetColliding(collides, Tester.bounds);
        for (int i = 0; i < collides.Count; i++)
        {
            collides[i].GetComponent<Renderer>().enabled = true;
        }

    }

    void OnDrawGizmos()
    {
        if (boundsTree != null)
        {
            boundsTree.DrawAllBounds();
        }
    }

}
