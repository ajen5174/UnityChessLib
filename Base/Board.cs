using System;
using System.Collections.Generic;

namespace UnityChess {
	/// <summary>An 8x8 column-major matrix representation of a chessboard.</summary>
	public class Board {
		public King WhiteKing { get; private set; }
		public King BlackKing { get; private set; }
		private readonly Piece[,] boardMatrix;
		
		public Piece this[Square position] {
			get {
				if (position.IsValid) return boardMatrix[position.File - 1, position.Rank - 1];
				throw new ArgumentOutOfRangeException($"Position was out of range: {position}");
			}

			set {
				if (position.IsValid) boardMatrix[position.File - 1, position.Rank - 1] = value;
				else throw new ArgumentOutOfRangeException($"Position was out of range: {position}");
			}
		}

		public Piece this[int file, int rank] {
			get => this[new Square(file, rank)];
			set => this[new Square(file, rank)] = value;
		}

		/// <summary>Creates a Board with initial chess game position.</summary>
		public Board() {
			boardMatrix = new Piece[8, 8];
			SetStartingPosition();
		}

		/// <summary>Creates a deep copy of the passed Board.</summary>
		public Board(Board board) {
			// TODO optimize this method
			// Creates deep copy (makes copy of each piece and deep copy of their respective ValidMoves lists) of board (list of BasePiece's)
			// this may be a memory hog since each Board has a list of Piece's, and each piece has a list of Movement's
			// avg number turns/Board's per game should be around ~80. usual max number of pieces per board is 32
			boardMatrix = new Piece[8, 8];
			for (int file = 1; file <= 8; file++)
				for (int rank = 1; rank <= 8; rank++) {
					Piece pieceToCopy = board[file, rank];
					if (pieceToCopy == null) continue;

					this[file, rank] = pieceToCopy.DeepCopy();
				}

			InitKings();
		}

		public void ClearBoard() {
			for (int file = 1; file <= 8; file++)
				for (int rank = 1; rank <= 8; rank++)
					this[file, rank] = null;

			WhiteKing = null;
			BlackKing = null;
		}

		public void SetStartingPosition() {
			//I'm going to make changes here!
			ClearBoard();

			//Row 2/Rank 7 and Row 7/Rank 2, both rows of pawns
			for (int file = 1; file <= 8; file++)
				foreach (int rank in new[] {2, 7}) {
					Square position = new Square(file, rank);
					Side pawnColor = rank == 2 ? Side.White : Side.Black;
					this[position] = new Pawn(position, pawnColor);
				}

			//SetNormalFirstAndLastFile();
			SetFischerFirstAndLastFile();
		}

		int GetRandomRangeExcluding(int min, int max, List<int> excludedNumbers)
        {
			int temp;
			do
			{
				temp = UnityEngine.Random.Range(min, max);
			} while (excludedNumbers.Contains(temp));
			return temp;
        }

		void SetFischerFirstAndLastFile() {
			List<int> occupiedSpaces = new List<int>();

			int rook1 = UnityEngine.Random.Range(1, 9);
			occupiedSpaces.Add(rook1);

			int rook2 = GetRandomRangeExcluding(1, 9, occupiedSpaces);
			occupiedSpaces.Add(rook2);
			if (rook1 > rook2)
			{
				int temp = rook1;
				rook1 = rook2;
				rook2 = temp;
			}

			int king;
			do
			{
				king = GetRandomRangeExcluding(1, 9, occupiedSpaces);
			} while (!(rook1 < king && king < rook2));
			occupiedSpaces.Add(king);

			int bishop1 = GetRandomRangeExcluding(1, 9, occupiedSpaces);
			occupiedSpaces.Add(bishop1);

			int bishop2;
			do
			{
				bishop2 = GetRandomRangeExcluding(1, 9, occupiedSpaces);

			} while ((bishop1 % 2) == (bishop2 % 2));
			occupiedSpaces.Add(bishop2);

			int knight1 = GetRandomRangeExcluding(1, 9, occupiedSpaces);
			occupiedSpaces.Add(knight1);
			int knight2 = GetRandomRangeExcluding(1, 9, occupiedSpaces);
			occupiedSpaces.Add(knight2);
			int queen = GetRandomRangeExcluding(1, 9, occupiedSpaces);
			occupiedSpaces.Add(queen);

			//Rows 1 & 8/Ranks 8 & 1, back rows for both players
			for (int file = 1; file <= 8; file++)
				foreach (int rank in new[] { 1, 8 })
				{
					Square position = new Square(file, rank);
					Side pieceColor = rank == 1 ? Side.White : Side.Black;


					if(file == rook1 || file == rook2)
                    {
						this[position] = new Rook(position, pieceColor);
					}
					else if(file == bishop1 || file == bishop2)
                    {
						this[position] = new Bishop(position, pieceColor);

					}
					else if(file == knight1 || file == knight2)
                    {
						this[position] = new Knight(position, pieceColor);

					}
					else if(file == king)
                    {
						this[position] = new King(position, pieceColor);
						WhiteKing = (King)this[file, rank];
						BlackKing = (King)this[file, rank];

					}
					else if(file == queen)
                    {
						this[position] = new Queen(position, pieceColor);

					}
				}

		}

		void SetNormalFirstAndLastFile() {
			//Rows 1 & 8/Ranks 8 & 1, back rows for both players
			for (int file = 1; file <= 8; file++)
				foreach (int rank in new[] { 1, 8 })
				{
					Square position = new Square(file, rank);
					Side pieceColor = rank == 1 ? Side.White : Side.Black;
					switch (file)
					{
						case 1:
						case 8:
							this[position] = new Rook(position, pieceColor);
							break;
						case 2:
						case 7:
							this[position] = new Knight(position, pieceColor);
							break;
						case 3:
						case 6:
							this[position] = new Bishop(position, pieceColor);
							break;
						case 4:
							this[position] = new Queen(position, pieceColor);
							break;
						case 5:
							this[position] = new King(position, pieceColor);
							break;
					}
				}

			WhiteKing = (King)this[5, 1];
			BlackKing = (King)this[5, 8];
		}

		public void MovePiece(Movement move) {
			if (!(this[move.Start] is Piece pieceToMove)) throw new ArgumentException($"No piece was found at the given position: {move.Start}");
			
			this[move.Start] = null;
			this[move.End] = pieceToMove;

			pieceToMove.HasMoved = true;
			pieceToMove.Position = move.End;

			(move as SpecialMove)?.HandleAssociatedPiece(this);
		}
		
		internal bool IsOccupied(Square position) => this[position] != null;

		internal bool IsOccupiedBySide(Square position, Side side) => this[position] is Piece piece && piece.Color == side;

		public void InitKings() {
			for (int file = 1; file <= 8; file++) {
				for (int rank = 1; rank <= 8; rank++) {
					if (this[file, rank] is King king) {
						if (king.Color == Side.White) WhiteKing = king;
						else BlackKing = king;
					}
				}
			}
		}
	}
}