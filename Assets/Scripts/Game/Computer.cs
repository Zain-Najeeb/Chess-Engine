
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
namespace Chess {

    public struct Move {
        public int start;
        public int target;
         public Move(int start, int target) {
            this.start = start;
            this.target = target;
        }
    }



    public class Computer {
        int positions = 0; 
        const int deptToReach = 4;
        const int positiveInfinity = 9999999;
		const int negativeInfinity = -positiveInfinity;
        const int pawnValue = 100; 
        const int knightValue = 300; 
        const int bishopValue = 300; 
        const int rookValue = 500; 
        const int queenValue = 900; 
        
         
        public Computer() {} 

        public void PlayMove(Board board, int index) {
            positions = 0; 
                 int min = negativeInfinity; 
            Dictionary<(int,int), int>stack= new   Dictionary<(int,int), int>();  
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int eval = Search(deptToReach, board, negativeInfinity, positiveInfinity, stack); 

       
       
            //    TotalPositions(deptToReach, board);
            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            int originalPosition = -1; 
            int target = -1; 
            foreach (var kvp in stack) {           
                if (kvp.Value >= min) {
                    // Debug.Log(kvp.Value);
                    (originalPosition, target) = kvp.Key;
                }
 
            }
            UnityEngine.Debug.Log(positions + " "  + elapsed.TotalSeconds); 
        //   UnityEngine.Debug.Log(originalPosition + "  " + target + " " + " " + stack.Count );

        
          TileManger.MovePiece(target, TileManger.tiles[originalPosition].currPiece.GetComponent<SpriteRenderer>().sprite, board.Squares[originalPosition], originalPosition ); 
          TileManger.tiles[originalPosition].DestroySprite(); 
        }


        int Evaluate(Board board) {
            int offset = board.Color == 8 ? 1 : -1 ;
            int whiteEval = CountMaterial(board.pieces[0], board.Squares);
            int blackEval = CountMaterial(board.pieces[1], board.Squares);

            blackEval += CountEndPanws(board.Squares, board.pieces[1],  0);
            whiteEval += CountEndPanws(board.Squares, board.pieces[0],  1);
            
            int eval = whiteEval - blackEval; 
    
            return eval * offset; 
        }

        int CountEndPanws(int[] Squares, HashSet<int> pieces, int offset) {
            if (pieces.Count < 8) {
                return PawnAtEnd(Squares, pieces, GameManager.PawnStartingSquares[offset]); 
            } else {
                return PawnAtEnd(Squares, GameManager.PawnStartingSquares[offset], pieces); 
            }
        }

        int PawnAtEnd(int[] Squares, HashSet<int> External, HashSet<int> Containor) {
            int eval = 0; 
            foreach(int square in External) {
                if (Piece.PieceType(Squares[square]) == Piece.Pawn && Containor.Contains(square)) {
                    // UnityEngine.Debug.Log("tefffst"); 
                    eval += 600; 
                }
            }
            return eval; 
        }
        void TotalPositions (int depth, Board board) {
            if (depth == 0) {
                return ;
            }

            bool whiteKing = board.Kings[0]; 
            bool BlackKing = board.Kings[1]; 
            bool blackRookQueen = board.Rooks[1][0]; 
            bool blackRookKing = board.Rooks[1][1]; 
            
            bool whiteRookQueen = board.Rooks[0][0]; 
            bool whiteRookKing = board.Rooks[0][1];
            int index = board.Color == 8 ? 0 : 1; 
            Dictionary<int, List<int>> moves = board.GenerateAllMoves(board.pieces[index], board.Squares,  index);
              foreach (var kvp in moves) {
                foreach (int square in kvp.Value) { 
                    if (depth == 1)  positions++;
                    int atttacks = board.Squares[square]; 
                    int currpiece =  board.Squares[kvp.Key]; 
                    board.MoviePiece(board.Squares[kvp.Key], kvp.Key, square); 
                    TotalPositions(depth - 1, board); 
            
                    board.Kings[0] = whiteKing;
                    board.Kings[1] = BlackKing;
                    board.Rooks[1][0] = blackRookQueen;
                    board.Rooks[1][1] = blackRookKing;

                    board.Rooks[0][0] = whiteRookQueen;
                    board.Rooks[0][1] = whiteRookKing;
                    board.UndoMove(kvp.Key, square, currpiece, atttacks); 
                }
            }
        }

    
        int Search (int depth, Board board, int alpha, int beta,  Dictionary<(int,int), int>stack) {
            if (depth == 0) {
                return Evaluate(board); 
            }
            int index = board.Color == 8 ? 0 : 1; 
            Dictionary<int, List<int>> moves = board.GenerateAllMoves(board.pieces[index], board.Squares, index);
            if (moves.Count == 0) {
                if (board.check.Count != 0) {
                    return negativeInfinity; 
                } else {
                    return 0; 
                }
            }
            bool whiteKing = board.Kings[0]; 
            bool BlackKing = board.Kings[1]; 
            bool blackRookQueen = board.Rooks[1][0]; 
            bool blackRookKing = board.Rooks[1][1]; 
            bool whiteRookQueen = board.Rooks[0][0]; 
            bool whiteRookKing = board.Rooks[0][1]; 
            foreach (var kvp in moves) {
                int currpiece =  board.Squares[kvp.Key]; 
                foreach (int square in kvp.Value) {
                    int atttacks = board.Squares[square]; 
                    board.MoviePiece(currpiece, kvp.Key, square);
                    if (depth == 1) positions++;  
                    int evaluation = -Search(depth -1, board, -beta, -alpha,  stack);
                    board.UndoMove(kvp.Key, square, currpiece, atttacks);
                    board.Kings[0] = whiteKing;
                    board.Kings[1] = BlackKing;
                    board.Rooks[1][0] = blackRookQueen;
                    board.Rooks[1][1] = blackRookKing;
                    board.Rooks[0][0] = whiteRookQueen;
                    board.Rooks[0][1] = whiteRookKing;
                    if (evaluation >= beta) {
                        return beta; 
                    }
                    if (evaluation > alpha) {
                        if (depth == deptToReach) {
                            stack[(kvp.Key, square)] = evaluation; 
                        }
                        alpha = evaluation; 
                    }
                    
                }
            }
            return alpha; 
        }

        int CountMaterial(HashSet<int> pieces, int[] squares) {
            int material = 0;
            foreach (int item in pieces ) {
                material += WorthFromType(Piece.PieceType(squares[item])); 
            }
            return material; 
        }

        public static int WorthFromType(int piece) {
            return piece switch
            {
                2 => pawnValue,
                3 => bishopValue,
                4 => knightValue,
                5 => rookValue,
                6 => queenValue,
                _ => 0,
            };
        }
    }
}
