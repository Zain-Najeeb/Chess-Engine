using System.Collections.Generic;
using System ;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Security.Cryptography;



namespace Chess {
    public static class GameManager {
     
        static readonly int[] DirectionOffsets = {8, -8,-1,1,7,-7,9,-9};
        static readonly int[][] PawnStartingSquares = new int[][]{
            new int[] {8,9,10,11,12,13,14,15}, 
            new int[] {48,49,50,51,52,53,54,55}, 
        }; 
        public static readonly HashSet<int>  ends = new HashSet<int>{
        0, 1, 2, 3, 4, 5, 6, 7, 56, 57, 58, 59, 60, 61, 62, 63
    };
        static readonly int[] knightOffsets = {-2, -1, 2, 1}; 
        static readonly int[][] NumSquaresToedge = new int[64][];   
        public static void GenerateSlidingMoves(int tile, int piece, List<int> moves, int[] Squares, int colour, Dictionary<int, int> pinned, bool forCheck, HashSet<int> check) {
            if (DoubleCheck(check)) return;
            int opponent = Piece.IsColour(piece , 8) ? 16 : 8; 
            int direction = pinned.ContainsKey(tile) ? pinned[tile] : -1; 

            int oppisoteDirection = GetOtherDirection(direction);  
            int startDirIndex = Piece.PieceType(piece) == Piece.Bishop ? 4 : 0; 
            int endDirIndex = Piece.PieceType(piece) == Piece.Rook ? 4 : 8; 
            if (direction != -1) {
                if ( !(startDirIndex <= direction && endDirIndex >=  direction))  return;  
            }
            for (int i = startDirIndex; i < endDirIndex; i++) {
                for (int j = 0; j < NumSquaresToedge[tile][i]; j ++) {
                    int curr = tile + DirectionOffsets[i] * (j +1); 
                    if (Piece.IsColour(Squares[curr], colour))  {
                        IgnorePieceCheck(curr, moves, forCheck); 
                        break;
                    }
                    if (direction == -1) {
                        IsValid(curr, check, moves); 
                    } else {
                        if (i == direction || i == oppisoteDirection) {
                            IsValid(curr, check, moves); 
                        }
                    }

                    if ( Piece.IsColour(Squares[curr], opponent) && (!forCheck || (forCheck && Piece.PieceType(Squares[curr]) != Piece.King) )) break; 
                }
            }
        }
        public static int GetOtherDirection(int direction) {
            if (direction%2 == 0) return direction + 1; 
            return direction -1; 
        }
        public static bool DoubleCheck(HashSet<int> check) {
            if (check == null) return false; 
            if (check.Contains(-1)) return true;
            return false;
        }

        public static void IsValid(int position, HashSet<int> check, List<int> moves) {
            if (check == null || check.Count == 0) {
                moves.Add(position); 
                return;
            }
            if (check.Contains(position)) moves.Add(position); 
        }
        public static void IgnorePieceCheck(int position, List<int> moves,  bool KingCheck) {
            if (!KingCheck) return; 
            moves.Add(position); 
        }
        public static void GeneratePawnMoves(int tile, int piece, List<int> moves, int[] Squares, int colour, int enPessant, Dictionary<int, int> pinned, bool KingCheck, HashSet<int> check) {
            if (DoubleCheck(check)) return; 
            int pinnedDirection = pinned.ContainsKey(tile) ? pinned[tile] : -1; 
            if (pinnedDirection == 2 || pinnedDirection == 3) return; 
            int opponent = Piece.IsColour(piece , 8) ? 16 : 8; 
            int direction = colour == 8 ? 8: -8;  
            int index = colour == 8 ? 0 : 1; 
            int size = PawnStartingSquares[index].Contains(tile) ? 2 : 1; 
            for (int i = 0; i < size; i++) {
                int curr = tile + (i+1)*direction; 
                if (i == 0 && pinnedDirection != 0 && pinnedDirection != 1) {
                    int right = (curr + 1)%8 == 0 ? 0 : 1;
                    int left = curr % 8 == 0 ? 0 : 1;
                    if (right == 1) { 
                        if (pinnedDirection == -1 || (pinnedDirection != -1 && ( (colour == 16 && (pinnedDirection == 4 || pinnedDirection == 5)) || (colour == 8 && (pinnedDirection == 6 || pinnedDirection == 7)) )  )) {
                            if (curr + right == enPessant && Piece.IsColour(Squares[curr + direction + right], opponent)) IsValid(curr + right, check, moves); 
                            if (Piece.IsColour(Squares[curr + right], opponent) || KingCheck) IsValid(curr + right, check, moves); 
                        }
                    }   
                    if (left == 1) {
                         if (pinnedDirection == -1 || (pinnedDirection != -1 &&   ( (colour == 8 && (pinnedDirection == 4 || pinnedDirection == 5)) || (colour == 16 && (pinnedDirection == 6 || pinnedDirection == 7)) ) )) { 
                            if (curr - left == enPessant && Piece.IsColour(Squares[curr + direction + right], opponent)) IsValid(curr -left, check, moves);  
                            if (Piece.IsColour(Squares[curr - left], opponent) || KingCheck) IsValid(curr -left, check, moves);  
                        }
                    }
                }
                if (Squares[curr] != 0) break;   
                if ((pinnedDirection == -1 || pinnedDirection == 0 || pinnedDirection == 1) && !KingCheck) IsValid(curr, check, moves);
            }
        }

        public static void GenerateKnightMoves(int tile, int piece, List<int> moves, int[] Squares, int colour, Dictionary<int, int> pinned, HashSet<int> check) {
            if (DoubleCheck(check)) return; 
            if (pinned != null) {
                if (pinned.ContainsKey(tile)) return; 
            }
            
            for (int i = 0 ; i < 4; i++) {
                int widthOffset = knightOffsets[i]; 

                if ( i < 2) {
                    if ((tile + widthOffset) % 8 > tile % 8 || tile + widthOffset < 0) continue;
                } else {
                    if ((tile + widthOffset) % 8 < tile % 8 ) continue; 
                }
                int height = i%2 == 0 ? 1: 2; 
                int curr = tile + widthOffset + height*8; 
                if (curr <= 63 ) {
                    if (!Piece.IsColour(Squares[curr], colour)) {
                        // Debug.Log(curr + " " + i + " " + tile + " " + widthOffset +" " + tile +widthOffset);
                        IsValid(curr, check, moves);
                    }  else {
                        IgnorePieceCheck(curr, moves, check == null); 
                    }
                }
                curr -= 2*height*8; 
                if (curr >= 0) {
                    if (!Piece.IsColour(Squares[curr], colour)) {
                        IsValid(curr, check, moves);
                    }  else {
                        IgnorePieceCheck(curr, moves, check == null); 
                    }
                
           
                } 
            }

        }
        public static HashSet<int> GenerateAllPossibleMoves(HashSet<int> pieces, int oppisoteColour, int [] Squares) {
            List<int> attacks = new List<int>();
            Dictionary<int, int> pinned = new Dictionary<int, int>();   
            HashSet<int> moves = new HashSet<int>(); 
            foreach (int item in pieces) {
                int opponentPiece = Squares[item]; 
                if (Piece.IsSlidingPiece(opponentPiece)) {
                GenerateSlidingMoves(item, opponentPiece,attacks,Squares,oppisoteColour, pinned ,true, null); 
                } else {
                    if (Piece.PieceType(opponentPiece) == 2) { // pawn
                        GeneratePawnMoves(item, opponentPiece, attacks, Squares, oppisoteColour,  -1, pinned, true, null); 
                    } else if (Piece.PieceType(opponentPiece) == Piece.Knight) {
                        GenerateKnightMoves(item, opponentPiece, attacks, Squares, oppisoteColour, null, null); 
                    } else {
                        GenerateKingMoves(item, opponentPiece, attacks, Squares, oppisoteColour, null, null, null, false, null); 
                    }
                }
              moves.UnionWith(attacks); 
            }
            return moves; 
        }
        public static HashSet<int> CheckforCheck(int kingPosition, int colour, int[] Squares) {
            HashSet<int> moves = new HashSet<int>(); 
            bool empty = true; 
            int oppisoteColour = colour == 8 ? 16 : 8;  
            for (int i = 0; i < 8; i ++) {
                for (int j = 0; j < NumSquaresToedge[kingPosition][i]; j ++) {
                    int curr = kingPosition + DirectionOffsets[i] * (j +1);
                    if (Piece.IsColour(Squares[curr], colour)) break;  
                    if (empty) {
                        moves.Add(curr);
                    } 
                    if (Piece.IsColour(Squares[curr], oppisoteColour) && Piece.IsSlidingPiece(Squares[curr])) {
                        int type = Piece.PieceType(Squares[curr]); 
                        if (type == Piece.Queen || (Piece.Rook == type && i < 4) || (Piece.Bishop == type && i >= 4) ) {
                            if (!empty) {
                                moves.Add(-1); 
                                return moves; 
                            } else {
                                empty = false; 
                                break; 
                            }   
                        } else {
                            break;
                        }
                        
                    }  if (Piece.IsColour(Squares[curr], oppisoteColour)) break; 
                }
                if (empty) {
                    moves.Clear(); 
                }
            }
            List<int> attacks = new List<int>();
            GenerateKnightMoves(kingPosition, -1, attacks, Squares, colour, null, null);
               
            for (int i = 0; i < attacks.Count; i++) {
                if (Squares[attacks[i]] != 0 && Piece.PieceType(Squares[attacks[i]]) == Piece.Knight && Piece.IsColour(Squares[attacks[i]], oppisoteColour)) {

                    if (!empty) {
                        moves.Add(-1); 
                        return moves; 
                    } else {
                        moves.Add(attacks[i]); 
                        empty = false; 
                        break; 
                    }  
                }
            }
            attacks.Clear();
            int height = colour == 8 ? 8 : -8; 
            int right = (kingPosition + 1)%8 == 0 ? 0 : 1;
            int left = kingPosition % 8 == 0 ? 0 : 1;
            int current = kingPosition + height;
            if (right == 1) {
                if (Squares[current + right] != 0 && Piece.PieceType(Squares[current + right]) == Piece.Pawn && Piece.IsColour(Squares[current + right], oppisoteColour)) {
                    if (!empty) {
                        moves.Add(-1); 
                        return moves; 
                    } else {
                        moves.Add(current + right); 
            
                    }  
                }
            }
            if (left == 1) {
                if (Squares[current - left] != 0 && Piece.PieceType(Squares[current - left]) == Piece.Pawn && Piece.IsColour(Squares[current - left], oppisoteColour)) {
                    if (!empty) {
                        moves.Add(-1); 
                        return moves; 
                    } else {
                        moves.Add(current - left); 
            
                    }  
                }
            }

            return moves; 
        }

        public static void GenerateKingMoves(int tile, int piece, List<int> moves, int[] Squares, int colour, bool[] kings, bool[][] rooks, List<HashSet<int>> pieces, bool forValid, HashSet<int> check ) {
            int index = colour == 8 ? 0 : 1; 
            int oppisote = colour == 8 ? 1 : 0; 
            int oppisoteColour = colour == 8 ? 16 : 8;  
            HashSet<int> notValid = new HashSet<int>(); 
            if (forValid) {
                notValid = GenerateAllPossibleMoves(pieces[oppisote], oppisoteColour, Squares); 
            }
            
            for (int i = 0; i < 8; i ++) {
                if (NumSquaresToedge[tile][i] != 0) {
                    if (!Piece.IsColour(Squares[tile + DirectionOffsets[i]], colour) && !notValid.Contains(tile + DirectionOffsets[i])) {
                        moves.Add(tile + DirectionOffsets[i]); 
                    }
                    if (Piece.IsColour(Squares[tile + DirectionOffsets[i]], colour) && check == null) {
                        moves.Add(tile + DirectionOffsets[i]); 
                    }
             
                }
            }
       
            if (forValid && !kings[index] && (check == null || check.Count == 0))  {
                //Kingside castle 
                int offset = 5 + 56*index; 
                if (Squares[offset] == 0 && Squares[offset + 1] == 0 && !notValid.Contains(offset +1) && !notValid.Contains(offset +1) && !notValid.Contains(offset )) {
                    if (!rooks[index][1]) moves.Add(offset +1);  
                }
                //Queenside Castle
                offset = 1 + 56*index; 
                if (Squares[offset] == 0 && Squares[offset + 1] == 0 && Squares[offset + 2] == 0 && !notValid.Contains(offset +1) && !notValid.Contains(offset )) {
                      if (!rooks[index][0]) moves.Add(offset +1); 
                }
            }
            
        }
        public static int DidPlayerCastle(int originalPosition, int newPosition, out int rook) {
            rook = 0; 
            if (originalPosition/8 != newPosition/8) return 0; 
            int differnce =  originalPosition - newPosition; 
            if (Math.Abs(differnce) > 1) {
                rook = differnce < 0 ? 7 : 0; 
                return differnce;
            }
            return 0; 
        }

        public static int EnPessantCheck(int originalPosition, int newPosition) {
            if (Math.Abs(originalPosition - newPosition) ==  16) {
                return (originalPosition + newPosition) >> 1; 
            }
            return -1 ;
        }
        public static bool PromotionCheck(int newPosition) {
            return ends.Contains(newPosition); 
        }

        public static Dictionary<int, int> GeneratePinnedPieces(int kingPosition, int colour, int[] Squares) {
            int opponent = Piece.IsColour(colour , 8) ? 16 : 8; 

            Dictionary<int, int> pinned = new Dictionary<int, int>(); 
            for (int i = 0; i < 8; i++) {
                int friendlyPosition = -1; 
                for (int j = 0; j < NumSquaresToedge[kingPosition][i]; j ++) {
                    int curr = kingPosition + DirectionOffsets[i] * (j +1); 
                    if (Squares[curr] !=0 && Piece.IsColour(Squares[curr], colour)) {
                        if (friendlyPosition != -1) break;
                        friendlyPosition = curr;  
                        continue; 
                    }
                    if (Squares[curr] !=0 && Piece.IsColour(Squares[curr], opponent) && friendlyPosition != -1 )  {
                        if (Piece.IsSlidingPiece(Squares[curr])) {
                            int direction = i; 
                            if (Piece.PieceType(Squares[curr]) == Piece.Bishop) {
                                if (direction >= 4) {
                                    pinned.Add(friendlyPosition, direction); 
                                }
                            } else if (Piece.PieceType(Squares[curr]) == Piece.Rook) {
                                if (direction < 4) {
                                 
                                    pinned.Add(friendlyPosition, direction); 
                                }
                            } else {
                                pinned.Add(friendlyPosition, direction); 
                            }
                        }
                        break; 
                    }
                }
            }
            return pinned; 
        }

        public static bool IsAttackedByPawn(int square, int colour, int[] Squares) {
            int direction =  colour == 8 ? 8: -8;  
            int right = (square + 1)%8 == 0 ? 0 : 1;
            int left = square % 8 == 0 ? 0 : 1;
            if (right == 1) { 
                if (Piece.PieceType(Squares[square + direction + right]) == Piece.Pawn && !Piece.IsColour(Squares[square + direction + right], colour)) {
                    return true; 
                }
            } 
            if (right == 1) {
                if (Piece.PieceType(Squares[square + direction + left]) == Piece.Pawn && !Piece.IsColour(Squares[square + direction + left], colour)) {
                    return true; 
                }
            }
            return false; 
        }

        public static void ComputeEdges() {
            for (int file =0; file <8; file ++) {
                for (int rank =0; rank <8; rank++) {
                    int numNorth = 7 - rank;
                    int numSouth = rank; 
                    int numWest = file; 
                    int numEast = 7 - file; 

                    int squareIndex = rank * 8 + file; 
                    
                    NumSquaresToedge[squareIndex] = new int[]{
                        numNorth,
                        numSouth, 
                        numWest,
                        numEast,
                        Math.Min(numNorth, numWest), 
                        Math.Min(numSouth, numEast), 
                        Math.Min(numNorth, numEast), 
                        Math.Min(numSouth, numWest)
                    }; 
                }
            }
        } 
    }


}