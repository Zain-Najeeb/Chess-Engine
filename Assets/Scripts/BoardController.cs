using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    
    
    void Start()
    {
        GameObject board = new GameObject("Board");
        board.transform.parent = transform; 

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.parent = board.transform;
        quad.transform.localScale = new Vector2(8f, 8f); 
        Material quadMaterial = new Material(Shader.Find("Unlit/Color"));

    }

    void Update()
    {
        
    }
}
