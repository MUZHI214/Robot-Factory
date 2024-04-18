using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDManager : MonoBehaviour
{
    public static TDManager main;

    public Transform startPoint;
    public Transform[] path;
   
    private void Awake()
    {
        main = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
