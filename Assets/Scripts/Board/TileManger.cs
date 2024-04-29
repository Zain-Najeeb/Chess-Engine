using System;
using System.Collections;
using System.Collections.Generic;
using Chess;
using Packages.Rider.Editor.UnitTesting;
using UnityEditor.UIElements;
using UnityEngine;

public class TileManger : MonoBehaviour
{

    public const string StartPositionFEN = "r3k2r/p1ppqpb1/Bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPB1PPP/R3K2R w KQkq - 0 1";
    [SerializeField] private  int width, height; 
    [SerializeField] private  Tile prefab;
    private static readonly Color SelectedColour  = new Color (222f /255f , 62f /255f,  77f /255f);  
    [SerializeField] private  Transform cam; 
    public static Board board; 
    private static Dictionary<int, Sprite> piecePairs;
    public static readonly List<Tile> tiles = new List<Tile>();
    public static Computer engine = new Computer(); 
    void Start() {
        CreateBoard();
        LoadSprites(); 
        board = new Board(StartPositionFEN);  
        for (int i = 0; i < 64; i++) {
            if (board.Squares[i] != 0) {
                tiles[i].AddPiece(piecePairs[board.Squares[i]], board.Squares[i]);
            } 
        }
        // engine.PlayMove(board, board.Color); 
    }

    void LoadSprites() {
        piecePairs = new Dictionary<int, Sprite> {
        {Piece.White | Piece.Pawn, LoadSpriteFromFile("Assets/Sprites/WhitePawn.png")}, 
        {Piece.White | Piece.Bishop, LoadSpriteFromFile("Assets/Sprites/WhiteBishop.png")},
        {Piece.White | Piece.King, LoadSpriteFromFile("Assets/Sprites/WhiteKing.png")},
        {Piece.White | Piece.Queen, LoadSpriteFromFile("Assets/Sprites/WhiteQueen.png")},
        {Piece.White | Piece.Knight, LoadSpriteFromFile("Assets/Sprites/WhiteHorse.png")},
        {Piece.White | Piece.Rook, LoadSpriteFromFile("Assets/Sprites/WhiteRook.png")},
        {Piece.Black | Piece.Pawn, LoadSpriteFromFile("Assets/Sprites/BlackPawn.png")}, 
        {Piece.Black | Piece.Bishop,LoadSpriteFromFile("Assets/Sprites/BlackBishop.png")},
        {Piece.Black | Piece.King, LoadSpriteFromFile("Assets/Sprites/BlackKing.png")},
        {Piece.Black | Piece.Queen, LoadSpriteFromFile("Assets/Sprites/BlackQueen.png")},
        {Piece.Black | Piece.Knight, LoadSpriteFromFile("Assets/Sprites/BlackKnight.png")},
        {Piece.Black | Piece.Rook, LoadSpriteFromFile("Assets/Sprites/BlackRook.png")}  
        }; 
    }

     private Sprite LoadSpriteFromFile(string path) {
        Texture2D texture = LoadTextureFromFile(path);
        if (texture != null) {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            return sprite;
        }
        return null;
    }
     private Texture2D LoadTextureFromFile(string path) {
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData); 
        return texture;
    }
    void Update() {
        
    }
    void CreateBoard() {

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                Tile _tile = Instantiate(prefab, new Vector3(j,i), Quaternion.identity);  
                _tile.name =  tiles.Count.ToString(); 
                var offset = (i + j)%2 == 0; 
                _tile.Init(offset, tiles.Count); 
                tiles.Add(_tile); 
            }
        }
        cam.transform.position = new Vector3((float) width/2 -0.5f , (float)height/2 -0.5f, -10); 
    }

    public static bool IsValidMove(int tile, int piece, int target) {
        List<int> moves = board.PossibleMoves(piece, tile,null); 
        return moves.Contains(target); 
    }

    public static void HighLgihtSqauares (int tile, int piece) {
        board.InitilizeCheck();
        List<int> moves = board.PossibleMoves(piece, tile,null); 
        for (int i = 0; i < moves.Count; i++) {
            tiles[moves[i]].HighlightSquare(SelectedColour); 
        }
    }
    public static void MovePiece(int newPosition, Sprite sprite, int piece, int originalPosition) {

        int oldValue = tiles[newPosition].piece;
        int offset = Piece.IsColour(piece, 8) ? 0 :1;
        int opponent = Piece.IsColour(piece, 8) ? 1 :0; 
        tiles[newPosition].AddPiece(sprite, piece); 
        board.InitilizeCheck(); 
        board.MoviePiece(piece,originalPosition,newPosition);
        if (Piece.PieceType(piece) == Piece.King) {
                int distance = GameManager.DidPlayerCastle(originalPosition, newPosition, out int defaultRook);
                if ( distance != 0) {
                    int rookPositon = distance < 0 ? 5 : 3; 
                    rookPositon += offset*56; 
                    defaultRook += offset*56;
                    Sprite rookSprite = tiles[defaultRook].currPiece.GetComponent<SpriteRenderer>().sprite; 
                    tiles[rookPositon].AddPiece( rookSprite, Piece.Colour(piece) | Piece.Rook); 
                    tiles[defaultRook].DestroySprite(); 
                }
            }
        
       if (Piece.PieceType(piece) == Piece.Pawn && oldValue == -1 && originalPosition% 8 != newPosition% 8) {
            int removePawnPosition = newPosition + 8 -16*opponent;
            tiles[removePawnPosition].DestroySprite();
        }

        if (Piece.PieceType(piece) == Piece.Pawn) {
            if (GameManager.PromotionCheck(newPosition)) {
                tiles[newPosition].DestroySprite(); 
                int promoted = Piece.Colour(piece) | Piece.Queen; 
                tiles[newPosition].AddPiece(piecePairs[promoted], promoted ); 
            }
        }
            

        if (board.Color == 16) {
           engine.PlayMove(board, opponent);
        }

    }
    public static void DeslectSquares (int tile, int piece) {
        List<int> moves = board.PossibleMoves(piece, tile, null); 
        for (int i = 0; i < moves.Count; i++) {
            tiles[moves[i]].HighlightSquare( tiles[moves[i]].defaultColor); 
        }
    }
}

