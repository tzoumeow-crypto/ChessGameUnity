using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

//遊戲名稱: Stalemate Chess
//規則1: 初始時白國王在c3、黑國王在f6，未來的所有時間內兩個王都會以棋盤中央輻射對稱的方式移動。

//規則2:每個玩家手上將會持有queen、rook、bishop、knight各一隻、以及Pawn無限多個。Pawn和king都沒有攻擊格。movingPiece、rook、bishop、knight的攻擊格都與chess遊戲中相同。

//規則3:玩家在一回合內可以做兩件事情則一，移動king或擺放棋子。
//3-1 兩個king任何時候呈現輻射對稱，所以移動自己的king同時也是在移動對方的king。移動時回合方會在自己的king後面留下一個Pawn作為軌跡。(對手則不需要)
//3-2 擺放棋子的要求為不改變任一方king的狀態。擺放之後不能再移動棋子，直到被對方國王吃掉則可再重新擺放。
//3-3 king的移動需要遵守三個條件
//3-3-1 不能自己改變自己的狀態，但可以在不改變狀態的前提下吃對手擺在棋盤上的棋子
//3-3-2 不能移動自己king時讓對手吃掉對手自己的棋子(對手若將一個Pawn擺在自己的右邊，則我方king不能向左走，否則對手將會吃掉對手自己的Pawn。但移動king的同時讓對手吃我方棋子是符合規範的，還可以收回其子做新的攻擊)
//3-3-3 當兩個King相接於棋盤中央點時(例如分別位於d4和e5) ，由於移動後需要放一個Pawn的原因，所以兩個King不能互換位置的穿過。

//規則4: 本遊戲中共有兩種狀態: 正常狀態與被攻擊狀態。只有在回合中移動king，在自己沒有改變自身狀態的前提下將對手拖入我方攻擊格之中 ，則對手的狀態變成被攻擊狀態。（沒有其他改變狀態方法，例如自己踏入攻擊格、擺放棋子直接攻擊對手、擺放棋子擋住原本唯一在攻擊自己的敵方棋子使自己不再受到攻擊等皆為違規）

//規則5: 輸贏判定
//5-1 在我方回合完成的瞬間，查看對手的king是否尚有符合規則移動的可能性，若對方King在我方回合結束的瞬間周圍八個個子都無法移動，則我方獲勝、對方輸。
//5-1 此判定只在一方回合結束後對另一方生效。換而言之若我方回合結束時雙方king都無路可走，則仍然由我方獲勝。
//5-3 應該是沒有和局的吧...?

//更多解釋:
//1: 本遊戲的攻擊手段除了擺放棋子將對手步步緊逼以外，還可以將對手拖入我方攻擊範圍使其無路可走。例如白方在 black king緊鄰white bishop的情況下移動king將對手拖入white bishop的攻擊範圍內使其呈現相接的狀態，在沒有其他棋子介入之下其實白方已經獲勝，因為此時黑方無符合規則移動可走。黑方不能自己逃脫攻擊範圍，黑方不能在bishop攻擊路徑上退後，因為他退後的同時生出來的pawn會擋住bishop的攻擊讓自子回歸正常狀態，正樣是不合規的。黑方不能吃掉白方主教，因為吃掉之後沒人再攻擊黑方，等於黑方自己移動而改變自己狀態，也是不合規的。將對手拖入有限攻擊格內與避免自己被對手拖入有限攻擊格之內，為需要建立的思維。
//2: 棋子有限，所以追擊要謹慎，若追擊不當可能陷入無子可用的處境。
//3: 此遊戲中tempo很重要，追擊方要盡可能保持自己對於對手的威脅，使對手不斷防禦直到遊戲結束。若某一瞬間自己被迫防禦，則很可能會被對手會在可用棋子多、進攻猛烈的狀態下失去自己原有的優勢。
//4: 下pawn也可能是一種攻擊，如果對手有重要的逃跑路徑且自己處境安全下，可以下pawn於對手逃跑路徑的輻射對稱點阻止對手逃跑，(因為3-3-2)，對手失去所有移動可能時對手輸。同樣道理，若預測到對手可能會下子在某處時，也可以先行一步擺放作為阻攔。




public class Controller : MonoBehaviour
{

    public GameObject SChessPiece;
    public GameObject NumberText;

    public GameObject movePlate;
    public bool boardIsFlipped = false;

    public GameObject[,] gameBoard { get; set; } = new GameObject[8, 8];
    //public GameObject[][,] gameBoards { get; set; } = null;
    //array一旦定義之後只能固定長度，用List吧
    public List<PieceType?[,]> gameBoards { get; set; } = new List<PieceType?[,]>();
    public int howManyMoves { get; set; } = 0;

    public bool[,] whiteAtkSquare { get; set; } = new bool[8, 8];
    public bool[,] blackAtkSquare { get; set; } = new bool[8, 8];

    public bool[,] whiteOppoPieceSquare { get; set; } = new bool[8, 8]; //紀錄對手的pawn在哪裡，因為pawn沒有攻擊格，但是仍能限制對手行動
    public bool[,] blackOppoPieceSquare { get; set; } = new bool[8, 8]; //紀錄對手的pawn在哪裡，因為pawn沒有攻擊格，但是仍能限制對手行動

    //會需要創造虛擬的block來計算
    private int virtualBlockX = -1;
    private int virtualBlockY = -1;


    public string currentPlayer { get; set; } = "white";
    public bool maybeGameOver { get; set; } = false;

    public GameObject theEnd;




    public void Start()
    {

        howManyMoves = 0;
        SetNumberText(boardIsFlipped);
        ResetGameBoard();
        gameBoards = new List<PieceType?[,]>();
        theEnd = Instantiate(theEnd, new Vector3(-4.6f, -1.8f, 0f), Quaternion.identity);
        theEnd.SetActive(false);
        //一開始先存
        SaveGameBoard();

    }


    private float rKeyTimer = 0.0f; // 計時器
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            FlipBoard();
        }
        // 2. 當持續按住 R 鍵時
        if (Input.GetKey(KeyCode.R))
        {
            rKeyTimer += Time.deltaTime; // 累加每幀經過的時間

            // 達到 3 秒門檻
            if (rKeyTimer >= 1.5)
            {
                ResetGameBoard();
                SChessMan.DestroyMovePlate();
                howManyMoves = 0;
                currentPlayer = "white";
                gameBoards = new List<PieceType?[,]>();
                SaveGameBoard() ;
                rKeyTimer = 0.0f; // 重置計時器，避免下一幀繼續重複觸發
            }
        }
        // 3. 中途只要手放開 R 鍵，立刻將計時歸零
        if (Input.GetKeyUp(KeyCode.R))
        {
            rKeyTimer = 0.0f;
        }


        //if (howManyMoves % 2 == 0)
        //{
        //    currentPlayer = "white";
        //}
        //else
        //{
        //    currentPlayer = "black";
        //}

    }




    //現在是個人人皆可調用的工具(好欸)
    //藉由xBoard和yBoard，計算出棋子在世界[座標]的位置。是座標!!!
    //注意不要與Game.cs裡的SetPosition混淆，SstPosition適用於紀錄剛剛從Create()造出來的Gameobject在chess
    public static void SetCoord(int anyXBoard, int anyYBoard, GameObject obj, bool boardIsFlipped)
    {
        if (boardIsFlipped)
        {
            anyXBoard = 7 - anyXBoard;
            anyYBoard = 7 - anyYBoard;
        }
        //Debug.Log($"收到({anyXBoard},{anyYBoard})");
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
        if (obj.GetComponent<ChessMan>() != null)
        {
            z = -1;
        }
        else if (obj.GetComponent<MovePlate>() != null)
        {
            z = -2;
        }

        obj.transform.position = new Vector3(x, y, z);
        //Debug.Log($"{(obj.TryGetComponent<SChessMan>(out var scm) ? scm.pieceType.ToString() : obj.name)} 放置在 ({x}, {y})");
    }

    public (GameObject wk, GameObject wq, GameObject wr, GameObject wb, GameObject wn, 
        GameObject bk, GameObject bq, GameObject br, GameObject bb, GameObject bn) ResetGameBoard()
    {
        //清空場上旗子
        GameObject[] sChessPieces = GameObject.FindGameObjectsWithTag("SChessPiece");
        foreach(GameObject g in sChessPieces)
        {
            Destroy(g);
        }



        //清空棋盤
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                gameBoard[x, y] = null;
            }
        }
        GameObject wk = CreateSChessPiece(PieceType.WhiteKing, 2, 2, true);
        GameObject bk = CreateSChessPiece(PieceType.BlackKing, 5, 5, true);
        //在棋盤右邊建立黑棋與白棋的選擇
        GameObject wq = CreateSChessPiece(PieceType.WhiteQueen, 8, 7, false);
        GameObject wr = CreateSChessPiece(PieceType.WhiteRook, 8, 6, false);
        GameObject wb = CreateSChessPiece(PieceType.WhiteBishop, 8, 5, false);
        GameObject wn = CreateSChessPiece(PieceType.WhiteKnight, 8, 4, false);
        CreateSChessPiece(PieceType.WhitePawn, 8, 3, false);

        GameObject bq = CreateSChessPiece(PieceType.BlackQueen, 9, 7, false);
        GameObject br = CreateSChessPiece(PieceType.BlackRook, 9, 6, false);
        GameObject bb = CreateSChessPiece(PieceType.BlackBishop, 9, 5, false);
        GameObject bn = CreateSChessPiece(PieceType.BlackKnight, 9, 4, false);
        CreateSChessPiece(PieceType.BlackPawn, 9, 3, false);
        currentPlayer = "white";
        return (wk, wq, wr, wb, wn, bk, bq, br, bb, bn);
    }

    public GameObject CreateSChessPiece(PieceType pieceType, int xBoard, int yBoard, bool isReal)
    {
        GameObject chessPiece = Instantiate(SChessPiece, new Vector3(xBoard, yBoard, -1), Quaternion.identity);
        SChessMan cpscm = chessPiece.GetComponent<SChessMan>();
        cpscm.pieceType = pieceType;
        cpscm.isReal = isReal;
        cpscm.xBoard = xBoard;
        cpscm.yBoard = yBoard;
        cpscm.Activate();
        SetCoord(xBoard, yBoard, chessPiece, boardIsFlipped);
        if (isReal)
        {
            SetPosition(chessPiece);
        }
        return chessPiece;
    }

    //把Create()出來的GameObject放到chessPieces[,]裡，並且把GameObject放到棋盤上
    public void SetPosition(GameObject obj)
    {
        SChessMan scm = obj.GetComponent<SChessMan>();
        int x = scm.xBoard;
        int y = scm.yBoard;
        gameBoard[x, y] = obj;
    }
    //重要!!x,y可能不再gameBoard的索引範圍內，要先檢查!!!
    public GameObject GetPosition(int x, int y) =>
        (x >= 0 && x < 8 && y >= 0 && y < 8) ? gameBoard[x, y] : null;
    //判斷(x, y)是否在棋盤上
    public bool PositionIsOnBoard(int x, int y) => (x >= 0 && x < gameBoard.GetLength(0) && y >= 0 && y < gameBoard.GetLength(1));
    public void SetPositionEmpty(int x, int y)
    {
        if(x >= 0 && x < 8 && y >= 0 && y < 8)
        {
            gameBoard[x, y] = null;
        }
    }
        











    //這個方法是用來切換棋盤的顯示狀態，當玩家點擊翻轉按鈕時會被呼叫
    public void SetNumberText(bool boardIsFlipped)
    {
        DeleteNumberText();
        BuildNumberText(boardIsFlipped);
    }

    public void FlipBoard()
    {
        boardIsFlipped = !boardIsFlipped;
        SetNumberText(boardIsFlipped);
        SetSChessPieces(boardIsFlipped);
        SChessMan.DestroyMovePlate();
    }

    public void DeleteNumberText()
    {
        GameObject[] numberTexts = GameObject.FindGameObjectsWithTag("NumberText");
        foreach (GameObject numberText in numberTexts)
        {
            Destroy(numberText);
        }
    }

    public void BuildNumberText(bool boardIsFlipped)
    {
        if (!boardIsFlipped)
        {
            GameObject[] numbers = new GameObject[]
            {
                CreateNumberText(NumberTextEnum.Number1, -1, 0),
                CreateNumberText(NumberTextEnum.Number2, -1, 1),
                CreateNumberText(NumberTextEnum.Number3, -1, 2),
                CreateNumberText(NumberTextEnum.Number4, -1, 3),
                CreateNumberText(NumberTextEnum.Number5, -1, 4),
                CreateNumberText(NumberTextEnum.Number6, -1, 5),
                CreateNumberText(NumberTextEnum.Number7, -1, 6),
                CreateNumberText(NumberTextEnum.Number8, -1, 7),
            };
            GameObject[] texts = new GameObject[]
            {
                CreateNumberText(NumberTextEnum.TextA, 0, -1),
                CreateNumberText(NumberTextEnum.TextB, 1, -1),
                CreateNumberText(NumberTextEnum.TextC, 2, -1),
                CreateNumberText(NumberTextEnum.TextD, 3, -1),
                CreateNumberText(NumberTextEnum.TextE, 4, -1),
                CreateNumberText(NumberTextEnum.TextF, 5, -1),
                CreateNumberText(NumberTextEnum.TextG, 6, -1),
                CreateNumberText(NumberTextEnum.TextH, 7, -1),
            };
        }
        else if (boardIsFlipped)
        {
            GameObject[] numbers = new GameObject[]
            {
                CreateNumberText(NumberTextEnum.Number8, -1, 0),
                CreateNumberText(NumberTextEnum.Number7, -1, 1),
                CreateNumberText(NumberTextEnum.Number6, -1, 2),
                CreateNumberText(NumberTextEnum.Number5, -1, 3),
                CreateNumberText(NumberTextEnum.Number4, -1, 4),
                CreateNumberText(NumberTextEnum.Number3, -1, 5),
                CreateNumberText(NumberTextEnum.Number2, -1, 6),
                CreateNumberText(NumberTextEnum.Number1, -1, 7),
            };
            GameObject[] texts = new GameObject[]
            {
                CreateNumberText(NumberTextEnum.TextH, 0,-1 ),
                CreateNumberText(NumberTextEnum.TextG , 1,-1 ),
                CreateNumberText(NumberTextEnum.TextF, 2,-1 ),
                CreateNumberText(NumberTextEnum.TextE, 3,-1 ),
                CreateNumberText(NumberTextEnum.TextD, 4,-1 ),
                CreateNumberText(NumberTextEnum.TextC, 5,-1 ),
                CreateNumberText(NumberTextEnum.TextB, 6,-1 ),
                CreateNumberText(NumberTextEnum.TextA, 7,-1 ),
            };
        }
    }
    public GameObject CreateNumberText(NumberTextEnum numberTextEnum, int xBoard, int yBoard)
    {
        GameObject numberText = Instantiate(NumberText, new Vector3(xBoard, yBoard, 0), Quaternion.identity);
        numberText.GetComponent<NumberText>().numberTextEnum = numberTextEnum;
        numberText.GetComponent<NumberText>().xBoard = xBoard;
        numberText.GetComponent<NumberText>().yBoard = yBoard;
        numberText.GetComponent<NumberText>().Activate();
        SetCoord(xBoard, yBoard, numberText, false);
        return numberText;
    }

    //這個方法是用來重新渲染棋盤上所有棋子的座標，當棋盤翻轉時會被呼叫
    public void SetSChessPieces(bool boardIsFlipped)
    {
        foreach (var piece in gameBoard)
        {
            if (piece != null)
            {
                SChessMan scm = piece.GetComponent<SChessMan>();
                if (scm != null)
                {
                    SetCoord(scm.xBoard, scm.yBoard, piece, boardIsFlipped);
                }
            }
        }
    }







    //當一個棋子被擺放於gameBoard時，計算出白方棋與黑方棋攻擊的格子，紀錄於whiteAtkSquare和blackAtkSquare
    public void UpdateAttackSquares()
    {
        //清空攻擊格
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                whiteAtkSquare[x, y] = false;
                blackAtkSquare[x, y] = false;
            }
        }
        //遍歷棋盤上的所有棋子，計算攻擊格
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = gameBoard[x, y];
                if (piece != null)
                {
                    SChessMan scm = piece.GetComponent<SChessMan>();
                    switch (scm.pieceType)
                    {
                        case PieceType.WhiteQueen:
                        case PieceType.BlackQueen:
                            QueenAtkSquare(piece);
                            break;
                        case PieceType.WhiteRook:
                        case PieceType.BlackRook:
                            RookAtkSquare(piece);
                            break;
                        case PieceType.WhiteBishop:
                        case PieceType.BlackBishop:
                            BishopAtkSquare(piece);
                            break;
                        case PieceType.WhiteKnight:
                        case PieceType.BlackKnight:
                            KnightAtkSquare(piece);
                            break;
                    }
                }
            }
        }
        ////先把抓到攻擊格顯示在Debug.Log
        //for(int i = 0; i < 8; i++)
        //{
        //    for(int j = 0; j < 8; j++)
        //    {
        //        if(whiteAtkSquare[i, j] == true)
        //        {
        //            Debug.Log($"white攻擊({i}, {j})");
        //        }
        //    }
        //}
        //for (int i = 0; i < 8; i++)
        //{
        //    for (int j = 0; j < 8; j++)
        //    {
        //        if (blackAtkSquare[i, j] == true)
        //        {
        //            Debug.Log($"black攻擊({i}, {j})");
        //        }
        //    }
        //}

    }
    public void QueenAtkSquare(GameObject AttackingPiece)
    {
        LineAtkSquare(1, 0, AttackingPiece);
        LineAtkSquare(1, 1, AttackingPiece);
        LineAtkSquare(0, 1, AttackingPiece);
        LineAtkSquare(-1, 1, AttackingPiece);
        LineAtkSquare(-1, 0, AttackingPiece);
        LineAtkSquare(-1, -1, AttackingPiece);
        LineAtkSquare(0, -1, AttackingPiece);
        LineAtkSquare(1, -1, AttackingPiece);
    }

    public void RookAtkSquare(GameObject AttackingPiece)
    {
        LineAtkSquare(1, 0, AttackingPiece);
        LineAtkSquare(0, 1, AttackingPiece);
        LineAtkSquare(-1, 0, AttackingPiece);
        LineAtkSquare(0, -1, AttackingPiece);
    }

    public void BishopAtkSquare(GameObject AttackingPiece)
    {
        LineAtkSquare(1, 1, AttackingPiece);
        LineAtkSquare(-1, 1, AttackingPiece);
        LineAtkSquare(-1, -1, AttackingPiece);
        LineAtkSquare(1, -1, AttackingPiece);
    }

    private void LineAtkSquare(int xDir, int yDir, GameObject AttackingPiece) //Direction
    {
        int x = AttackingPiece.GetComponent<SChessMan>().xBoard;
        int y = AttackingPiece.GetComponent<SChessMan>().yBoard;
        string player = AttackingPiece.GetComponent<SChessMan>().player;
        x += xDir;
        y += yDir;
        while (PositionIsOnBoard(x, y) && GetPosition(x, y) == null && !(x == virtualBlockX && y == virtualBlockY))
        {
            if (player == "white")
            {
                whiteAtkSquare[x, y] = true;
            }
            else if (player == "black")
            {
                blackAtkSquare[x, y] = true;
            }
            x += xDir;
            y += yDir;
        }
        //如果遇到棋子，則該格也算是攻擊格，

        if(PositionIsOnBoard(x, y))
        {
            if (GetPosition(x, y) != null || (x == virtualBlockX && y == virtualBlockY))
            {
                if (player == "white")
                {
                    whiteAtkSquare[x, y] = true;
                }
                else if (player == "black")
                {
                    blackAtkSquare[x, y] = true;
                }
            }
        }
        
    }

    public void KnightAtkSquare(GameObject AttackingPiece)
    {
        pointAtkSquare(1, 2, AttackingPiece);
        pointAtkSquare(2, 1, AttackingPiece);
        pointAtkSquare(2, -1, AttackingPiece);
        pointAtkSquare(1, -2, AttackingPiece);
        pointAtkSquare(-1, -2, AttackingPiece);
        pointAtkSquare(-2, -1, AttackingPiece);
        pointAtkSquare(-2, 1, AttackingPiece);
        pointAtkSquare(-1, 2, AttackingPiece);
    }
    private void pointAtkSquare(int xIncrement, int yIncrement, GameObject AttackingPiece)
    {
        int x = AttackingPiece.GetComponent<SChessMan>().xBoard + xIncrement;
        int y = AttackingPiece.GetComponent<SChessMan>().yBoard + yIncrement;
        if (PositionIsOnBoard(x, y))
        {
            if (AttackingPiece.GetComponent<SChessMan>().player == "white")
            {
                whiteAtkSquare[x, y] = true;
            }
            else if (AttackingPiece.GetComponent<SChessMan>().player == "black")
            {
                blackAtkSquare[x, y] = true;
            }
        }
    }







    //計算oppoPawn
    public void UpdateOppoPieceSquares()
    {

        //清空OppositePawn格
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                whiteOppoPieceSquare[x, y] = false;
                blackOppoPieceSquare[x, y] = false;
            }
        }
        //遍歷棋盤上的所有棋子，計算OppositePawn格
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = gameBoard[x, y];
                if (piece != null)
                {

                    SChessMan scm = piece.GetComponent<SChessMan>();
                    if(scm.pieceType == PieceType.WhiteKing)
                    {
                        whiteOppoPieceSquare[x, y] = true;
                    }
                    else if(scm.pieceType == PieceType.BlackKing)
                    {
                        blackOppoPieceSquare[x, y] = true;
                    }
                    else if (scm.player == "white")
                    {
                        whiteOppoPieceSquare[7 - x, 7 - y] = true;
                    }
                    else if (scm.player == "black")
                    {
                        blackOppoPieceSquare[7 - x, 7 - y] = true;
                    }
                }
                
                
            }
        }
    }








    public void KingMovePlate(GameObject king, bool isVirtual)
    {
        PointMovePlate(1, 0, king, isVirtual);
        PointMovePlate(1, 1, king, isVirtual);
        PointMovePlate(0, 1, king, isVirtual);
        PointMovePlate(-1, 1, king, isVirtual);
        PointMovePlate(-1, 0, king, isVirtual);
        PointMovePlate(-1, -1, king, isVirtual);
        PointMovePlate(0, -1, king, isVirtual);
        PointMovePlate(1, -1, king, isVirtual);
    }
    public void PointMovePlate(int xIncrement, int yIncrement, GameObject movingPiece, bool isVirtual)
    {
        SChessMan mscm = movingPiece.GetComponent<SChessMan>();
        string player = mscm.player;
        int x = mscm.xBoard + xIncrement;
        int y = mscm.yBoard + yIncrement;

        //要檢查x,y是否在棋盤上，檢查king的狀態合併檢查敵方攻擊格、敵方oppoPawn與敵方king的位置決定是否生成，最後再看是否isAttack
        if (player == "white")
        {
            if (!mscm.kingIsAttacked)
            {
                if (PositionIsOnBoard(x, y) && !blackAtkSquare[x, y] && !blackOppoPieceSquare[x, y])
                {
                    if (GetPosition(x, y) == null) { if (!isVirtual) { SpawnMovePlate(x, y, false); } else { maybeGameOver = false; } }
                    else if (GetPosition(x, y).GetComponent<SChessMan>().player != player) { if (!isVirtual) { SpawnMovePlate(x, y, true); } else { maybeGameOver = false; } }
                }
            }
            else if (mscm.kingIsAttacked)
            {
                if (PositionIsOnBoard(x, y) && blackAtkSquare[x, y] && !blackOppoPieceSquare[x, y])
                {
                    //還要檢查是否會在軌跡pawn生成之後是否會改變自己的狀態
                    if(CheckKingPawnBlocking(mscm.xBoard, mscm.yBoard, x, y, player))
                    {
                        if (GetPosition(x, y) == null) { if (!isVirtual) { SpawnMovePlate(x, y, false); } else { maybeGameOver = false; } }
                        else if (GetPosition(x, y).GetComponent<SChessMan>().player != player) { if (!isVirtual) { SpawnMovePlate(x, y, true); } else { maybeGameOver = false; } }
                    }
                    
                    
                }
            }
        }
        else if (player == "black")
        {
            Debug.Log("黑方king的狀態: " + mscm.kingIsAttacked);
            if (!mscm.kingIsAttacked)
            {
                if (PositionIsOnBoard(x, y) && !whiteAtkSquare[x, y] && !whiteOppoPieceSquare[x, y])
                {
                    //Debug.Log("我Spawn了");
                    
                    if (GetPosition(x, y) == null) { if (!isVirtual) { SpawnMovePlate(x, y, false); } else { maybeGameOver = false; } }
                    else if (GetPosition(x, y).GetComponent<SChessMan>().player != player) { if (!isVirtual) { SpawnMovePlate(x, y, true); } else { maybeGameOver = false; } }
                }
            }
            else if (mscm.kingIsAttacked)
            {
                if (PositionIsOnBoard(x, y) && whiteAtkSquare[x, y] && !whiteOppoPieceSquare[x, y])
                {
                    //Debug.Log("我Spawn了");
                    if (CheckKingPawnBlocking(mscm.xBoard, mscm.yBoard, x, y, player))
                    {
                        if (GetPosition(x, y) == null) { if (!isVirtual) { SpawnMovePlate(x, y, false); } else { maybeGameOver = false; } }
                        else if (GetPosition(x, y).GetComponent<SChessMan>().player != player) { if (!isVirtual) { SpawnMovePlate(x, y, true); } else { maybeGameOver = false; } }
                    }
                        
                }
            }
        }
        
    }



    private bool CheckKingPawnBlocking(int OriX, int OriY, int x, int y, string player)
    {
        //模擬king移動並生成pawn的狀況，在movingPiece原本的地方生成一個virtualBlock，king移動到(x, y)並判斷
        //搞不好我之前那一坨判斷都不用寫直接這個邏輯用到底就可以了呵呵
        //gameBoard[x, y] = movingPiece;
        //GameObject virtualBlock = null;
        virtualBlockX = OriX; virtualBlockY = OriY;
        //gameBoard[OriX, OriY] = virtualBlock;
        //SChessMan mscm = movingPiece.GetComponent<SChessMan>();
        bool finalReturn = false;
        if (player == "white")
        {
            UpdateAttackSquares();
            if (blackAtkSquare[x, y])
            {
                finalReturn =  true;
            }
            else finalReturn =  false;
        }
        else if (player == "black")
        {
            UpdateAttackSquares();
            if (whiteAtkSquare[x, y])
            {
                finalReturn =  true;
            }
            else finalReturn =  false;
        }
        else { Debug.LogError("player不是white不是black不然是什麼?"); }
        //東西要復原
        virtualBlockX = -1; virtualBlockY = -1;
        UpdateAttackSquares();

        return finalReturn;
    }



    //public void SpawnVirtualMovePlate(int x, int y)
    //{

    //}











    //全部64格-GetPosition(x, y) != null的格子-if(kingIsAttacked)直接對king造成傷害的格子
    //直接對king造成傷害的格子靠由king反向禁止生成之MovePlate來協助判定
    public void CalMovePlate(GameObject movingPiece)
    {
        //要先把對手的king抓出來
        string player = movingPiece.GetComponent<SChessMan>().player;
        GameObject opponantKing = null;
        GameObject myKing = null;
        bool[,] movePlateMap = new bool[8, 8];

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {

                if (gameBoard[i, j] == null)
                {
                    movePlateMap[i, j] = true;
                }
                if (gameBoard[i, j] != null)
                {
                    if (player == "white" && gameBoard[i, j].GetComponent<SChessMan>().pieceType == PieceType.BlackKing
                        || player == "black" && gameBoard[i, j].GetComponent<SChessMan>().pieceType == PieceType.WhiteKing)
                    {
                        opponantKing = gameBoard[i, j];
                    }
                    if(player == "white" && gameBoard[i, j].GetComponent<SChessMan>().pieceType == PieceType.WhiteKing
                        || player == "black" && gameBoard[i, j].GetComponent<SChessMan>().pieceType == PieceType.BlackKing)
                    {
                        myKing = gameBoard[i, j];
                    }
                }


            }
        }
        if(!opponantKing.GetComponent<SChessMan>().kingIsAttacked)
        {
            switch(movingPiece.GetComponent<SChessMan>().pieceType)
            {
                case PieceType.WhiteQueen:
                case PieceType.BlackQueen:
                    DimQueenMovePlate(opponantKing, movePlateMap);
                    break;
                case PieceType.WhiteRook:
                case PieceType.BlackRook:
                    DimRookMovePlate(opponantKing, movePlateMap);
                    break;
                case PieceType.WhiteBishop:
                case PieceType.BlackBishop:
                    DimBishopMovePlate(opponantKing, movePlateMap);
                    break;
                case PieceType.WhiteKnight:
                case PieceType.BlackKnight:
                    DimKnightMovePlate(opponantKing, movePlateMap);
                    break;
            }   
        }
        if (opponantKing.GetComponent<SChessMan>().kingIsAttacked || myKing.GetComponent<SChessMan>().kingIsAttacked)
        {
            //在攻擊狀態下不能用棋子擋住攻擊改變自己的狀態或他人的狀態
            DimBlockMovePlate(movePlateMap);
        }
        //kingIsAttacked的情況下，對手的king可以走到攻擊格內，所以movePlateMap不需要再減去攻擊king的格子
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if(movePlateMap[i, j])
                {
                    SpawnMovePlate(i, j, false);
                }
            }
        }
        
    }

    public void DimBlockMovePlate( bool[,] movePlateMap)
    {
        //創造一個虛假的ChessPiece放在gameBoard並且UpdateAttackSquares看看雙方king是否仍被攻擊?
        (GameObject wKing, GameObject bKing) kings = FindTwoKings();
        if (kings.wKing == null || kings.bKing == null)
        {
            Debug.LogError("怎麼可能沒有king啊?");
            return;
        }
        //GameObject virtualBlock = null; //這樣做不可行，會是null
        SChessMan wkscm = kings.wKing.GetComponent<SChessMan>();
        SChessMan bkscm = kings.bKing.GetComponent<SChessMan>();
        for(int i = 0;i < 8;i++)
        {
            for(int j = 0;j < 8;j++)
            {
                
                if (movePlateMap[i, j] )
                {
                    if (gameBoard[i, j] != null)
                    {
                        Debug.LogError("怎麼可能擺放棋子的時候在有東西的地方還生monvePlate啊?");
                    }
                    //在所有可能的地方都設virtualBlock看看
                    //gameBoard[i, j] = virtualBlock;
                    virtualBlockX = i;
                    virtualBlockY = j;
                }
                //先處裡myKing
                if (wkscm.kingIsAttacked)
                {
                    UpdateAttackSquares();
                    if (!blackAtkSquare[wkscm.xBoard, wkscm.yBoard])
                    {
                        movePlateMap[i, j] = false;
                    }
                }
                if (bkscm.kingIsAttacked)
                {
                    UpdateAttackSquares();
                    if (!whiteAtkSquare[bkscm.xBoard, bkscm.yBoard])
                    {
                        movePlateMap[i, j] = false;
                    }
                }
                //最後記得刪掉
                //gameBoard[i, j] = null;
                virtualBlockX = -1;
                virtualBlockY = -1;


            }
        }
        UpdateAttackSquares();
        
    }


    public void DimLineMovePlate(int xDir, int yDir, GameObject opponantKing, bool[,] movePlateMap)
    {
        int x = opponantKing.GetComponent<SChessMan>().xBoard;
        int y = opponantKing.GetComponent<SChessMan>().yBoard;
        x += xDir;
        y += yDir;

        //避免死當，加一個safetyCounter
        int safetyCounter = 0;
        while (PositionIsOnBoard(x, y) && GetPosition(x, y) == null)
        {
            movePlateMap[x, y] = false;
            x += xDir;
            y += yDir;
            safetyCounter++;
            if (safetyCounter > 8)
            {
                Debug.LogError("DimLineMovePlate: safetyCounter exceeded limit, possible infinite loop.");
                break;
            }
        }
    }




    public void DimQueenMovePlate(GameObject opponantKing, bool[,] movePlateMap)
    {
        DimLineMovePlate(1, 0, opponantKing, movePlateMap);
        DimLineMovePlate(1, 1, opponantKing, movePlateMap);
        DimLineMovePlate(0, 1, opponantKing, movePlateMap);
        DimLineMovePlate(-1, 1, opponantKing, movePlateMap);
        DimLineMovePlate(-1, 0, opponantKing, movePlateMap);
        DimLineMovePlate(-1, -1, opponantKing, movePlateMap);
        DimLineMovePlate(0, -1, opponantKing, movePlateMap);
        DimLineMovePlate(1, -1, opponantKing, movePlateMap);
        
    }

    public void DimRookMovePlate(GameObject opponantKing, bool[,] movePlateMap)
    {
        DimLineMovePlate(1, 0, opponantKing, movePlateMap);
        DimLineMovePlate(0, 1, opponantKing, movePlateMap);
        DimLineMovePlate(-1, 0, opponantKing, movePlateMap);
        DimLineMovePlate(0, -1, opponantKing, movePlateMap);
    }

    public void DimBishopMovePlate(GameObject opponantKing, bool[,] movePlateMap)
    {
        DimLineMovePlate(1, 1, opponantKing, movePlateMap);
        DimLineMovePlate(-1, 1, opponantKing, movePlateMap);
        DimLineMovePlate(-1, -1, opponantKing, movePlateMap);
        DimLineMovePlate(1, -1, opponantKing, movePlateMap);
    }






    public void DimPointMovePlate(int xIncrement, int yIncrement, GameObject opponantKing, bool[,] movePlateMap)
    {
        int x = opponantKing.GetComponent<SChessMan>().xBoard + xIncrement;
        int y = opponantKing.GetComponent<SChessMan>().yBoard + yIncrement;
        if (PositionIsOnBoard(x, y))
        {
            movePlateMap[x, y] = false;
        }
    }

    public void DimKnightMovePlate(GameObject opponantKing, bool[,] movePlateMap)
    {
        DimPointMovePlate(2, 1, opponantKing, movePlateMap);
        DimPointMovePlate(1, 2, opponantKing, movePlateMap);
        DimPointMovePlate(-1, 2, opponantKing, movePlateMap);
        DimPointMovePlate(-2, 1, opponantKing, movePlateMap);
        DimPointMovePlate(-2, -1, opponantKing, movePlateMap);
        DimPointMovePlate(-1, -2, opponantKing, movePlateMap);
        DimPointMovePlate(1, -2, opponantKing, movePlateMap);
        DimPointMovePlate(2, -1, opponantKing, movePlateMap);
    }








    public void SpawnMovePlate(int x, int y, bool isAttack)
    {
        GameObject obj = Instantiate(movePlate, new Vector3(x, y, -2), Quaternion.identity);
        SMovePlate smp = obj.GetComponent<SMovePlate>();
        smp.isAttack = isAttack;
        smp.xBoard = x;
        smp.yBoard = y;
        //把SetCoord()一道Game.cs
        SetCoord(x, y, obj, boardIsFlipped);
        //Debug.Log($"生成MovePlate在({x},{y})");
    }









    public (GameObject wKing, GameObject bKing) FindTwoKings()
    {
        GameObject whiteKing = null;
        GameObject blackKing = null;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {

                if (gameBoard[i, j] != null)
                {
                    if (gameBoard[i, j].GetComponent<SChessMan>().pieceType == PieceType.WhiteKing)
                    {
                        whiteKing = gameBoard[i, j];
                    }
                    else if(gameBoard[i, j].GetComponent<SChessMan>().pieceType == PieceType.BlackKing)
                    {
                        blackKing = gameBoard[i, j];
                    }
                }


            }
        }
        return (whiteKing, blackKing);
    }
    public void CheckIfKingIsAttacked()
    {
        
        (GameObject wKing, GameObject bKing) kings = FindTwoKings();
        if(kings.wKing == null || kings.bKing == null)
        {
            Debug.LogError("怎麼可能沒有king啊?");
            return;
        }
        SChessMan wkscm = kings.wKing.GetComponent<SChessMan>();
        if (blackAtkSquare[wkscm.xBoard, wkscm.yBoard])
        {
            wkscm.kingIsAttacked = true;
        }
        else 
        {
            wkscm.kingIsAttacked = false; 
        }
        SChessMan bkscm = kings.bKing.GetComponent<SChessMan>();
        if (whiteAtkSquare[bkscm.xBoard, bkscm.yBoard])
        {
            bkscm.kingIsAttacked = true;
        }
        else
        {
            bkscm.kingIsAttacked = false;
        }
    }











    public void SaveGameBoard()
    {
        PieceType?[,] typeBoard = new PieceType?[8, 8];
        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                if (gameBoard[i, j] != null)
                {
                    typeBoard[i, j] = GetPosition(i, j).GetComponent<SChessMan>().pieceType;
                }
            }
        }
        //gameBoards.Add(typeBoard);
        if (gameBoards.Count > howManyMoves)
        {
            gameBoards.RemoveRange(howManyMoves, gameBoards.Count - howManyMoves);
            
        }
        gameBoards.Add(typeBoard);
        howManyMoves = gameBoards.Count - 1;
    }

    public void UndoRedo(bool trueIsUedo)
    {
        PieceType?[,] toTheGame;
        if (trueIsUedo)
        {
            theEnd.SetActive(false);
            SChessMan.DestroyMovePlate();
            if (howManyMoves <= 0)
            {
                Debug.Log("目前gameboard為初始狀態");
                return;
            }
            toTheGame = gameBoards[howManyMoves - 1];
            //gameBoards.RemoveAt(lastIndex);
            howManyMoves--;
        }
        else
        {
            theEnd.SetActive(false);
            SChessMan.DestroyMovePlate();
            if (gameBoards.Count < howManyMoves+2)
            {
                Debug.Log("沒東西可以Redo了");
                return;
            }
            
            toTheGame = gameBoards[howManyMoves+1];
            //gameBoards.RemoveAt(lastIndex);
            howManyMoves++;
        }
        


        //清除gameBoard(可能重新寫Create在右邊?)
        //確定他們的isUsed和isReal之類的
        (GameObject wk, GameObject wq, GameObject wr, GameObject wb, GameObject wn,
        GameObject bk, GameObject bq, GameObject br, GameObject bb, GameObject bn) pieces = ResetGameBoard();
        //(GameObject wKing, GameObject bKing) kings = FindTwoKings();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (toTheGame[i, j] != null)
                {
                    switch(toTheGame[i, j])
                    { 
                        case PieceType.WhiteKing: MovePiece(pieces.wk, i, j); break;
                        case PieceType.WhiteQueen: MovePiece(pieces.wq, i, j); break;
                        case PieceType.WhiteRook: MovePiece(pieces.wr, i, j); break;
                        case PieceType.WhiteBishop: MovePiece(pieces.wb, i, j); break;
                        case PieceType.WhiteKnight: MovePiece(pieces.wn, i, j); break;
                        case PieceType.WhitePawn: CreateSChessPiece(PieceType.WhitePawn, i, j, true); break;
                        case PieceType.BlackKing: MovePiece(pieces.bk, i, j); break;
                        case PieceType.BlackQueen: MovePiece(pieces.bq, i, j); break;
                        case PieceType.BlackRook: MovePiece(pieces.br, i, j); break;
                        case PieceType.BlackBishop: MovePiece(pieces.bb, i, j); break;
                        case PieceType.BlackKnight: MovePiece(pieces.bn, i, j); break;
                        case PieceType.BlackPawn: CreateSChessPiece(PieceType.BlackPawn, i, j, true); break;


                    }
                }
            }
        }
        //全部重新Update
        if (howManyMoves % 2 == 0)
        {
            currentPlayer = "white";
        }
        else
        {
            currentPlayer = "black";
        }

        UpdateAttackSquares();
        UpdateOppoPieceSquares();
        CheckIfKingIsAttacked();
        CheckIfGameIsOver();


    }
    private void MovePiece(GameObject piece,  int i, int j)
    {
        SChessMan pscm = piece.GetComponent<SChessMan>();
        pscm.xBoard = i;
        pscm.yBoard = j;
        pscm.isReal = true;
        pscm.isUsed = true;
        pscm.Activate();
        SetPosition(piece);
        SetCoord(pscm.xBoard, pscm.yBoard, piece, boardIsFlipped);
    }

    public void CheckIfGameIsOver()
    {
        maybeGameOver = true;
        (GameObject wKing, GameObject bKing) kings = FindTwoKings();
        
        KingMovePlate(currentPlayer == "white"? kings.wKing: kings.bKing, true);
        if (maybeGameOver)
        {
            ClaimGameOver(currentPlayer);
        }

    }

    public void ClaimGameOver(string currentPlayer)
    {
        //僅關閉棋子的功能
        GameObject[] sChessPieces = GameObject.FindGameObjectsWithTag("SChessPiece");
        foreach(GameObject piece in sChessPieces)
        {
            if (piece.TryGetComponent<Collider2D>(out var col2D))
            {
                col2D.enabled = false;
            }
        }
        theEnd.SetActive(true);
        theEnd.GetComponent<End>().Activate(currentPlayer);
    }

}
