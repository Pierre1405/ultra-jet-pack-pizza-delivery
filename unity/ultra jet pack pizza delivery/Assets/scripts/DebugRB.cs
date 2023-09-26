using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRB : MonoBehaviour
{
    public Rigidbody2D rb;
    public Vector2 velocity;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        velocity = rb.velocity;
    }
}
