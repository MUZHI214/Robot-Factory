using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        //GridSystem grid = new GridSystem(4, 2, 10f, new Vector3(0, 0));
        Pathfinding pathfinding = new Pathfinding(10,10);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

        }

        if (Input.GetMouseButtonDown(1))
        {

        }
    }
}
