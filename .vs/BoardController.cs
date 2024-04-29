using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    
    public GameObject chessSquarePrefab;
    void Start()
    {
        GameObject board = new GameObject("Board");
        board.transform.parent = transform; 

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.parent = board.transform;
        quad.transform.localScale = new Vector2(8f, 8f); 
        CreateChessboard();
        
    }

    void Update()
    {
        
    }

     void CreateChessboard() {
        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                GameObject square = Instantiate(chessSquarePrefab, new Vector3(i, 0, j), Quaternion.identity);
                square.transform.parent = transform;
            }
        }
    }
}
