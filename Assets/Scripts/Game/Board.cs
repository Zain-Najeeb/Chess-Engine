namespace Chess {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AI;
    using System.Linq;  
    using Unity.Mathematics;

    public class Board {
        public  int[] Squares;
        public int Color;
        public int selectedIndex = -1; 
        public HashSet<int> check = new HashSet<int>(); 
        public int[] KingPositions = new int[]{4,60}; 
        public bool[] Kings = new bool[] {false, false};  

        public bool[][] Rooks = new bool[][]{
            new bool[] {true, true},
            new bool[] {true, true},
        }; 
        public List<HashSet<int>> pieces = new List<HashSet<int>> {
            new HashSet<int> {}, 
            new HashSet<int> {},  
        }; 
        int enPessant = -1; 
        public readonly Dictionary<int, int> rookPositions = new Dictionary<int, int> {
            {0, 0}, 
            {56, 0}, 
            {63,1}, 
            {7, 1}
        }; 
        public Board(string fen) {
            Squares = new int[64];
            LoadPositionFromFen(fen); 
            GameManager.ComputeEdges(); 
        }
        public Board() {

        }
        private void LoadPositionFromFen (string fen) {
            var piecePairs = new Dictionary<char, int> () {
                ['k'] = Piece.King,
                ['p'] = Piece.Pawn,
                ['n'] = Piece.Knight,
                ['b'] = Piece.Bishop,
                ['r'] = Piece.Rook,
                ['q'] = Piece.Queen
            }; 

            int file = 0, rank = 7; 
    
            foreach (char symbol in fen) {
                if (symbol =='/') {
                    file = 0; 
                    rank--; 
                } else if (symbol == ' ') {
                    break; 
                } else {
                    if (char.IsDigit(symbol)) {
                        file += (int) char.GetNumericValue(symbol); 
                    } else {
                        int pieceColour = char.IsUpper(symbol) ? Piece.White : Piece.Black;
                        int index = pieceColour == Piece.White ? 0 : 1;  
                        int pieceType = piecePairs[char.ToLower(symbol)]; 
                        Squares[rank * 8 + file] = pieceType | pieceColour;
                        if (pieceType == Piece.Rook) {
                            int position; 
                            if (rank * 8 + file == 7 || rank * 8 + file == 63 ) {
                                position = 1; 
                            } else {
                                position = 0 ;
                            }
                            rookPositions[rank * 8 + file] = position; 
                        }
                        if (pieceType == Piece.King) {
                            KingPositions[index] = rank * 8 + file;
                        }
                        pieces[index].Add(rank * 8 + file);
                        file++;
                    }
                }      
            }
            string[] words = fen.Split(' '); 
            Color = words[1][0] == 'w' ? 8 : 16; 
            string castleAbilites = words[2];
            foreach (char symbol in castleAbilites) {
                if (symbol == '-') break; 
                int pieceColour = char.IsUpper(symbol) ? 0: 1;
                int position = char.ToUpper(symbol) == 'K' ?  1 : 0; 
                Rooks[pieceColour][position] = false; 
            }
        }
        public List<int> PossibleMoves(int piece, int tile, Dictionary<int, int> pinned) {
            List<int> moves =  new List<int>();
            int offset = Piece.IsColour(piece, 8) ? 0 :1;

            if (pinned == null) {
                pinned = GameManager.GeneratePinnedPieces(KingPositions[offset], Color, Squares); 
            }
            if (Piece.IsSlidingPiece(piece)) {
                GameManager.GenerateSlidingMoves(tile, piece,moves,Squares,Color, pinned, false, check); 
            } else {
                if (Piece.PieceType(piece) == 2) { // pawn
                    GameManager.GeneratePawnMoves(tile, piece, moves, Squares, Color,  enPessant, pinned, false, check); 
                } else if (Piece.PieceType(piece) == Piece.Knight) {
                    GameManager.GenerateKnightMoves(tile, piece, moves, Squares, Color, pinned, check); 
                } else {
                    GameManager.GenerateKingMoves(tile, piece, moves, Squares, Color, Kings, Rooks, pieces, true, check); 
                }

            }
            return moves;
        }

        public Dictionary<int, List<int>> GenerateAllMoves(HashSet<int> pieces, int[] Squares) {
            Dictionary<int, List<int>> possible = new Dictionary<int, List<int>>(); 
            int offset = Color == 8 ? 0:1;
            Dictionary<int, int> pinned = GameManager.GeneratePinnedPieces(KingPositions[offset], Color, Squares); 
            InitilizeCheck();
            foreach (int tile in pieces) {
                possible[tile] = PossibleMoves(Squares[tile],tile, pinned);
                
                if (possible[tile].Count == 0) {
                    possible.Remove(tile);  
                } else {
                    // OrderMoves(tile, possible[tile], Squares); 
                }
            }
            return possible; 
        }
        public void InitilizeCheck() {
            int offset = Color == 16 ? 1 : 0;
            check = GameManager.CheckforCheck(KingPositions[offset], Color, Squares); 
            // foreach (int item in check) {
            //     Debug.Log(item); 
            // }
            // Debug.Log(offset); 
        }

        public void OrderMoves( int position, List<int> moves, int[] Squares) {
            foreach (int square in moves) {
                int moveScoreGuess = 0; 
                int movePieceType = Piece.PieceType(Squares[position]); 
                int capturePieceType = Piece.PieceType(Squares[position]);
                if (capturePieceType != Piece.None) {
                    moveScoreGuess = 10 * Computer.WorthFromType(capturePieceType) - Computer.WorthFromType(movePieceType); 
                }
                if (movePieceType == Piece.Pawn && GameManager.ends.Contains(square) ) {
                    moveScoreGuess += Computer.WorthFromType(Computer.WorthFromType(7)); 
                }
                if (GameManager.IsAttackedByPawn(square, Piece.Colour(Squares[position]), Squares )) {
                    moveScoreGuess -= Computer.WorthFromType(movePieceType); 
                }
            }

        }


        public void UndoMove(int oldPosition, int newPosition, int oldPiece,int piece) {
            
            Squares[oldPosition] =  oldPiece; 
            Squares[newPosition] = piece;
            int offset = Piece.IsColour(oldPiece, 8) ? 0 :1;
            int opponentOffset = offset == 0 ? 1: 0; 
            int opponent = offset == 0 ? Piece.Black : Piece.White;
            if (piece != 0 ) {
                pieces[opponentOffset].Add(newPosition); 
            } 
            if (Piece.PieceType(oldPiece) == Piece.Pawn && piece == 0  && oldPosition% 8 != newPosition% 8) {
   
                int addBackPawn = newPosition + 8 -16*(offset^1);
             
                Squares[addBackPawn] = opponent | Piece.Pawn; 
                pieces[offset^1].Add(addBackPawn);
            }
             if (Piece.PieceType(oldPiece) == Piece.King) {
                  int distance = GameManager.DidPlayerCastle(oldPosition, newPosition, out int defaultRook);
                  if (distance != 0 ) {
                    int rookPositon = distance < 0 ? 5 : 3; 
                    rookPositon += offset*56; 
                    defaultRook += offset*56;
                    Squares[rookPositon] = 0;                     
                    Squares[defaultRook] = Piece.Colour(oldPiece) | Piece.Rook; 
                    pieces[offset].Remove(rookPositon); 
                    // Debug.Log(oldPosition + " " + defaultRook + " " + rookPositon); 
                    pieces[offset].Add(defaultRook); 
                  }
                  KingPositions[offset] = oldPosition; 
            }

            pieces[offset].Remove(newPosition);
            pieces[offset].Add(oldPosition); 
            Color = Piece.IsColour(oldPiece, 8) ? 8 : 16; 
        }
        public void MoviePiece(int piece, int originalPosition, int newPosition) {
            int oldValue = Squares[newPosition]; 
            Squares[newPosition] = piece; 
            Squares[originalPosition] = 0; 
            Color = Piece.IsColour(piece, 8) ? 16 : 8; 
            int offset = Piece.IsColour(piece, 8) ? 0 :1;
            int opponent = offset == 0 ? 1 : 0; 
            pieces[offset].Remove(originalPosition); 
            pieces[offset].Add(newPosition); 
            if (pieces[opponent].Contains(newPosition)) {
                pieces[opponent].Remove(newPosition); 
            }
            if (Piece.PieceType(piece) == Piece.King && !Kings[offset]) { 
                int distance = GameManager.DidPlayerCastle(originalPosition, newPosition, out int defaultRook);
                if ( distance != 0) {
                    int rookPositon = distance < 0 ? 5 : 3; 
                    rookPositon += offset*56; 
                    defaultRook += offset*56;
                    Squares[rookPositon] = Piece.Colour(piece) | Piece.Rook; 
                    Squares[defaultRook] = 0; 
                    pieces[offset].Remove(defaultRook); 
                    pieces[offset].Add(rookPositon); 
                } else {
                    Kings[offset] = true; 
                }
            }
            if (Piece.PieceType(piece) == Piece.King) {
                KingPositions[offset] = newPosition; 
            }
            if (rookPositions.ContainsKey(originalPosition)) {
                Rooks[offset][rookPositions[originalPosition]] = true; 
            }
            if (rookPositions.ContainsKey(newPosition)) {
                int index; 
                if (newPosition == 0 || newPosition == 7 ) {
                    index = 0; 
                } else {
                    index =1 ;
                }
            
                Rooks[index][rookPositions[newPosition]] = true; 
            }
            enPessant = Piece.PieceType(piece) == Piece.Pawn ? GameManager.EnPessantCheck(originalPosition,newPosition) : -1; 
            if (Piece.PieceType(piece) == Piece.Pawn && oldValue == 0 && originalPosition% 8 != newPosition% 8  ) {
                int removePawnPosition = newPosition + 8 -16*(offset ^ 1);
                Squares[removePawnPosition] = 0; 
                pieces[opponent].Remove(removePawnPosition); 
            }

            if (Piece.PieceType(piece) == Piece.Pawn) {
                if (GameManager.PromotionCheck(newPosition)) {
                    Squares[newPosition] = Piece.Colour(piece) | Piece.Queen; 
                }
            }
            // if (check.Count != 0) {
            //     foreach (int item in check) {
            //         Debug.Log(item); 
            //     }
            // }

        }
      
    }
}