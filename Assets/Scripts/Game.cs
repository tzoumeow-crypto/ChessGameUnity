using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Game : MonoBehaviour
{
    public GameObject chessPiece;
    public GameObject movePlate;
    private GameObject[,] gameBoard = new GameObject[8, 8];
    private GameObject[] whitePieces = new GameObject[16];
    private GameObject[] blackPieces = new GameObject[16];
    public string currentPlayer { get; set; } = "white";
    public bool gameOver { get; set; } = false;
    void Start()
    {
        ////Instantiate(一個prefab, 座標, 四元數旋轉)
        //Instantiate(chessPiece, new Vector3(0, 0, -1), Quaternion.identity);
        //Create(PieceType, xBoard, yBoard);
        whitePieces = new GameObject[16]
        {
            Create(PieceType.WhiteRook, 0, 0),
            Create(PieceType.WhiteKnight, 1, 0),
            Create(PieceType.WhiteBishop, 2, 0),
            Create(PieceType.WhiteQueen, 3, 0),
            Create(PieceType.WhiteKing, 4, 0),
            Create(PieceType.WhiteBishop, 5, 0),
            Create(PieceType.WhiteKnight, 6, 0),
            Create(PieceType.WhiteRook, 7, 0),
            Create(PieceType.WhitePawn, 0, 1),
            Create(PieceType.WhitePawn, 1, 1),
            Create(PieceType.WhitePawn, 2, 1),
            Create(PieceType.WhitePawn, 3, 1),
            Create(PieceType.WhitePawn, 4, 1),
            Create(PieceType.WhitePawn, 5, 1),
            Create(PieceType.WhitePawn, 6, 1),
            Create(PieceType.WhitePawn, 7, 1)
        };
        blackPieces = new GameObject[16]
        {
            Create(PieceType.BlackRook, 0, 7),
            Create(PieceType.BlackKnight, 1, 7),
            Create(PieceType.BlackBishop, 2, 7),
            Create(PieceType.BlackQueen, 3, 7),
            Create(PieceType.BlackKing, 4, 7),
            Create(PieceType.BlackBishop, 5, 7),
            Create(PieceType.BlackKnight, 6, 7),
            Create(PieceType.BlackRook, 7, 7),
            Create(PieceType.BlackPawn, 0, 6),
            Create(PieceType.BlackPawn, 1, 6),
            Create(PieceType.BlackPawn, 2, 6),
            Create(PieceType.BlackPawn, 3, 6),
            Create(PieceType.BlackPawn, 4, 6),
            Create(PieceType.BlackPawn, 5, 6),
            Create(PieceType.BlackPawn, 6, 6),
            Create(PieceType.BlackPawn, 7, 6)
        };


        for (int i = 0; i < 16; i++)
        {
            SetPosition(whitePieces[i]);
            SetPosition(blackPieces[i]);
        }
    }




    //現在是個人人皆可調用的工具(好欸)
    //藉由xBoard和yBoard，計算出棋子在世界[座標]的位置。是座標!!!
    //注意不要與Game.cs裡的SetPosition混淆，SstPosition適用於紀錄剛剛從Create()造出來的Gameobject在chess
    public static void SetCoord(int anyXBoard, int anyYBoard, GameObject obj)
    {
        Debug.Log($"收到({anyXBoard},{anyYBoard})");
        float x = anyXBoard;
        float y = anyYBoard;
        //線性變換:x = xBoard * 0.66f - 2.31f; y = yBoard * 0.66f - 2.31f;
        x *= 0.66f;
        y *= 0.66f;
        x -= 2.31f;
        y -= 2.31f;
        //transform.position是Unity裡面每個物件都有的屬性，代表物件在世界座標的位置
        //Vector3(x, y, z) z軸是深度，越大越靠近鏡頭
        int z = -1;
        if(obj.GetComponent<ChessMan>() != null)
        {
            z = -1;
        }
        else if (obj.GetComponent<MovePlate>() != null)
        {
            z = -2;
        }

        obj.transform.position = new Vector3(x, y, z);
        Debug.Log($"{obj}放置在({x},{y})");
    }




    //依據xBoard和yBoard創造出合適的GameObject並且擺放到適當的地方
    public GameObject Create(PieceType pieceType, int x, int y)
    {
        GameObject obj = Instantiate(chessPiece, new Vector3(0, 0, -1), Quaternion.identity);
        //需要跟ChessMan抓xBoard和yBoard
        ChessMan cm = obj.GetComponent<ChessMan>();
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.pieceType = pieceType;
        cm.Activate();
        return obj;
    }

    //把Create()出來的GameObject放到chessPieces[,]裡，並且把GameObject放到棋盤上
    public void SetPosition(GameObject obj)
    {
        ChessMan cm = obj.GetComponent<ChessMan>();
        int x = cm.GetXBoard();
        int y = cm.GetYBoard();
        gameBoard[x, y] = obj;
    }



    //SetPositionEmpty(0, 0);       // 原本的格子清空：gameBoard[0, 0] = null;
    //SetPosition(piece, 0, 1);     // 新的格子登記：gameBoard[0, 1] = piece;
    public void SetPositionEmpty(int x, int y) => gameBoard[x, y] = null;

    //重要!!x,y可能不再gameBoard的索引範圍內，要先檢查!!!
    public GameObject GetPosition(int x, int y) =>
        (x >= 0 && x < 8 && y >= 0 && y < 8) ? gameBoard[x, y] : null;
    //判斷(x, y)是否在棋盤上
    public bool PositionIsOnBoard(int x, int y) => (x >= 0 && x < gameBoard.GetLength(0) && y >= 0 && y < gameBoard.GetLength(1));










    //下面是MovePlate生成系列
    public void QueenMovePlate(GameObject movingPiece)
    {
        LineMovePlate(1, 0, movingPiece);
        LineMovePlate(1, 1, movingPiece);
        LineMovePlate(0, 1, movingPiece);
        LineMovePlate(-1, 1, movingPiece);
        LineMovePlate(-1, 0, movingPiece);
        LineMovePlate(-1, -1, movingPiece);
        LineMovePlate(0, -1, movingPiece);
        LineMovePlate(1, -1, movingPiece);
    }

    public void RookMovePlate(GameObject movingPiece)
    {
        LineMovePlate(1, 0, movingPiece);
        LineMovePlate(0, 1, movingPiece);
        LineMovePlate(-1, 0, movingPiece);
        LineMovePlate(0, -1, movingPiece);
    }

    public void BishopMovePlate(GameObject movingPiece)
    {
        LineMovePlate(1, 1, movingPiece);
        LineMovePlate(-1, 1, movingPiece);
        LineMovePlate(-1, -1, movingPiece);
        LineMovePlate(1, -1, movingPiece);
    }

    public void LineMovePlate(int xIncrement, int yIncrement, GameObject movingPiece)
    {
        ChessMan mcm = movingPiece.GetComponent<ChessMan>();

        int x = mcm.GetXBoard() + xIncrement;
        int y = mcm.GetYBoard() + yIncrement;
        while (PositionIsOnBoard(x, y) && GetPosition(x, y) == null)
        {
            SpawnMovePlate(x, y, false);
            x += xIncrement;
            y += yIncrement;
        }
        //假設抓到一個在場上的GameObject，則判斷敵友
        //sc.GetPosition(x, y)抓到的東西是GameObject，要取得裏面的變數請抓她的ChessMan
        if (GetPosition(x, y) && GetPosition(x, y).GetComponent<ChessMan>().player != mcm.player)
        {
            SpawnMovePlate(x, y, true); //改變isAttack應該會寫在裡面
        }
    }


    public void KnightMovePlate(GameObject movingPiece)
    {
        PointMovePlate(2, 1, movingPiece);
        PointMovePlate(1, 2, movingPiece);
        PointMovePlate(-1, 2, movingPiece);
        PointMovePlate(-2, 1, movingPiece);
        PointMovePlate(-2, -1, movingPiece);
        PointMovePlate(-1, -2, movingPiece);
        PointMovePlate(1, -2, movingPiece);
        PointMovePlate(2, -1, movingPiece);
    }


    public void KingMovePlate(GameObject movingPiece)
    {
        PointMovePlate(1, 0, movingPiece);
        PointMovePlate(1, 1, movingPiece);
        PointMovePlate(0, 1, movingPiece);
        PointMovePlate(-1, 1, movingPiece);
        PointMovePlate(-1, 0, movingPiece);
        PointMovePlate(-1, -1, movingPiece);
        PointMovePlate(0, -1, movingPiece);
        PointMovePlate(1, -1, movingPiece);
    }



    public void PointMovePlate(int xIncrement, int yIncrement, GameObject movingPiece)
    {
        ChessMan mcm = movingPiece.GetComponent<ChessMan>();
        int x = mcm.GetXBoard() + xIncrement;
        int y = mcm.GetYBoard() + yIncrement;

        if(PositionIsOnBoard(x, y) && GetPosition(x, y) == null)
        {
            SpawnMovePlate(x, y, false);
        }
        if (GetPosition(x, y) && GetPosition(x, y).GetComponent<ChessMan>().player != mcm.player)
        {
            SpawnMovePlate(x, y, true); //改變isAttack應該會寫在裡面
        }
    }


    public void PawnMovePlate(GameObject movingPiece)
    {
        ChessMan mcm = movingPiece.GetComponent<ChessMan>();
        int x = mcm.GetXBoard();
        int y = mcm.GetYBoard();

        (int x, int y) frontLeft;
        (int x, int y) frontRight;
        (int x, int y) front;
        (int x, int y) frontFront;


        if (mcm.player == "white")
        {
            frontLeft = (x - 1, y + 1);
            frontRight = (x + 1, y + 1);
            front = (x, y + 1);
            frontFront = (x, y + 2);
        }
        else
        {
            frontLeft = (x + 1, y - 1);
            frontRight = (x - 1, y - 1);
            front = (x, y - 1);
            frontFront = (x, y - 2);
        }

        // forward square
        if (PositionIsOnBoard(front.x, front.y) && GetPosition(front.x, front.y) == null)
        {
            SpawnMovePlate(front.x, front.y, false);

            // double forward from starting rank
            bool isStartingRank = (mcm.player == "white" && y == 1) || (mcm.player == "black" && y == 6);
            if (isStartingRank && PositionIsOnBoard(frontFront.x, frontFront.y) && GetPosition(frontFront.x, frontFront.y) == null)
            {
                SpawnMovePlate(frontFront.x, frontFront.y, false);
            }
        }

        // capture diagonals
        GameObject potentialObj = GetPosition(frontLeft.x, frontLeft.y);
        if (PositionIsOnBoard(frontLeft.x,frontLeft.y) && potentialObj != null && potentialObj.GetComponent<ChessMan>() != null && potentialObj.GetComponent<ChessMan>().player != mcm.player)
        {
            SpawnMovePlate(frontLeft.x,frontLeft.y, true);
        }
        potentialObj = GetPosition(frontRight.x, frontRight.y);
        if (PositionIsOnBoard(frontRight.x,frontRight.y) && potentialObj != null && potentialObj.GetComponent<ChessMan>() != null && potentialObj.GetComponent<ChessMan>().player != mcm.player)
        {
            SpawnMovePlate(frontRight.x, frontRight.y, true);
        }

        // en passant, promotion, etc. can be added here

    }



    //把要生成東西的方法放在controller
    public void SpawnMovePlate(int x, int y, bool isAttack)
    {
        GameObject obj = Instantiate(movePlate, new Vector3(x, y, -2), Quaternion.identity);
        MovePlate mp = obj.GetComponent<MovePlate>();
        mp.isAttack = isAttack;
        mp.SetPosition(x, y);
        //把SetCoord()一道Game.cs
        SetCoord(x, y, obj);
        

    }

    
}
