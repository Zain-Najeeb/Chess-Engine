using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Chess;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour{
    [SerializeField] private Color normal, offset;
     private Vector3 draggedPosition;
    [SerializeField] private SpriteRenderer _renderer;
    public GameObject currPiece = null; 
    private readonly Color ColourSelect = new Color(228f /255f, 115f /255f, 43f /255f); 
    public Color defaultColor ; 
    public int piece = -1; 
    private int position; 
    private int hold = 0;
    private bool isMouseHeldDown = false;
    public void Init(bool isOffset, int position) {
        offset.a = 1f; 
        normal.a = 1f; 
        _renderer.color = isOffset ? offset : normal;
        defaultColor = _renderer.color; 
        this.position = position; 
        gameObject.AddComponent<BoxCollider2D>();
    }

    void Start() {

    }
    void Update() {
        if (Input.GetMouseButton(0) && isMouseHeldDown && piece == TileManger.board.Squares[TileManger.board.selectedIndex]  ) {
            hold++; 
            if (hold >= 100 ) {
                Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + draggedPosition;
                currPiece.transform.position = new Vector3(newPosition.x, newPosition.y, currPiece.transform.position.z);
            }
        } 
        if (Input.GetMouseButtonUp(0)) {
            if (hold >= 75) {
                if (isMouseHeldDown) {  
                    Vector2 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero);
                     if (hit.collider != null) { 
                        GameObject hitObject = hit.collider.gameObject;
                        int newPosition = Int32.Parse(hitObject.name);
                        HandleMouseEvent(newPosition); 
                     }
                    hold = 0; 
                }
            }
        }
    }
    public void AddPiece(Sprite sprite, int piece) {
        this.piece = piece; 
        if (currPiece != null) {
            Destroy(currPiece);
        }
        currPiece = new GameObject("Piece");
        currPiece.transform.parent = transform; 
        currPiece.transform.localPosition = Vector3.zero; 
        SpriteRenderer spriteRenderer = currPiece.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = _renderer.sortingOrder + 1; 
        currPiece.transform.localScale =new Vector3(0.65f, 0.65f, 0.65f);
        BoxCollider2D collider = currPiece.AddComponent<BoxCollider2D>();
        collider.isTrigger = true; 
        collider.size = new Vector2(0,0); 
    }

    private void HandleMouseEvent(int tile) {
         Sprite sprite = TileManger.tiles[TileManger.board.selectedIndex].currPiece.GetComponent<SpriteRenderer>().sprite; 
         if (hold >= 75 && tile == position)  {
            Destroy(currPiece); 
            AddPiece(sprite, piece); 
            return; 
         }

         int target = hold >= 75 ? tile : position;
         if (TileManger.IsValidMove(TileManger.tiles[TileManger.board.selectedIndex].position ,TileManger.tiles[TileManger.board.selectedIndex].piece, target)) {
            if (TileManger.tiles[TileManger.board.selectedIndex].currPiece != null) {
                Destroy(TileManger.tiles[TileManger.board.selectedIndex].currPiece); 
                TileManger.tiles[TileManger.board.selectedIndex].currPiece = null; 
            }
            TileManger.DeslectSquares(TileManger.board.selectedIndex,  TileManger.tiles[TileManger.board.selectedIndex].piece );   
            TileManger.MovePiece(tile, sprite, TileManger.tiles[TileManger.board.selectedIndex].piece, TileManger.board.selectedIndex); 
            TileManger.tiles[TileManger.board.selectedIndex].piece = -1; 
        } else {
            TileManger.DeslectSquares(TileManger.board.selectedIndex,  TileManger.tiles[TileManger.board.selectedIndex].piece );   
            TileManger.tiles[TileManger.board.selectedIndex].AddPiece(sprite,  TileManger.tiles[TileManger.board.selectedIndex].piece); 
        }
        TileManger.tiles[TileManger.board.selectedIndex].isMouseHeldDown = false; 
        TileManger.tiles[TileManger.board.selectedIndex].HighlightSquare(  TileManger.tiles[TileManger.board.selectedIndex].defaultColor); 
        TileManger.tiles[TileManger.board.selectedIndex].hold = 0; 
        TileManger.board.selectedIndex = -1;
  
        isMouseHeldDown = false; 
    }
     private void OnMouseDown() {
        int index = TileManger.board.selectedIndex;
    
        if (index == -1) {
            if (currPiece != null && Piece.IsColour(piece,TileManger.board.Color)) {
                HighlightSquare(ColourSelect); 
                TileManger.board.selectedIndex = position;
                TileManger.HighLgihtSqauares(position, piece); 
                if (!isMouseHeldDown) {
                    isMouseHeldDown = true;
                    draggedPosition = currPiece.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
            }
        } else {
            if (Piece.IsColour(piece ,TileManger.board.Color)) { 
                TileManger.tiles[index].HighlightSquare(TileManger.tiles[index].defaultColor);  
                TileManger.tiles[index].isMouseHeldDown = false;  
                if (TileManger.board.selectedIndex != position) {
                    HighlightSquare(ColourSelect); 
                    TileManger.DeslectSquares(TileManger.board.selectedIndex,  TileManger.tiles[TileManger.board.selectedIndex].piece );    
                    TileManger.board.selectedIndex = position;
                    isMouseHeldDown = true; 
                    draggedPosition = currPiece.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    TileManger.HighLgihtSqauares(position, piece); 
                } else {
                    if (hold < 75) {  //clicked on same spot
                        isMouseHeldDown = false;
                        TileManger.DeslectSquares(position,  piece);   
                        TileManger.board.selectedIndex = -1; 
                        hold =0; 
                    }
                }
            } else {
                HandleMouseEvent(position); 
                hold =0; 
            }
             
        }
    }
    public void DestroySprite() {
        Destroy(currPiece); 
        piece = -1; 
        currPiece = null; 
    }
    public void HighlightSquare(Color color) {
        _renderer.color = color;
    }
}
