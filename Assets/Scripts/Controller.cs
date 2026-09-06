using UnityEngine;

//遊戲名稱: Stalemate Chess
//規則1: 初始時白國王在c3、黑國王在f6，未來的所有時間內兩個王都會以棋盤中央輻射對稱的方式移動。

//規則2:每個玩家手上將會持有queen、rook、bishop、knight各一隻、以及Pawn無限多個。Pawn和king都沒有攻擊格。queen、rook、bishop、knight的攻擊格都與chess遊戲中相同。

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
    public bool boardIsFlipped = false;

    private GameObject[,] gameBoard = new GameObject[8, 8];

    public string currentPlayer { get; set; } = "white";
    public bool gameOver { get; set; } = false;




    public void Start()
    {
        SetNumberText(boardIsFlipped);
        ResetGameBoard();
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
            if (rKeyTimer >= 3)
            {
                ResetGameBoard();
                rKeyTimer = 0.0f; // 重置計時器，避免下一幀繼續重複觸發
            }
        }
        // 3. 中途只要手放開 R 鍵，立刻將計時歸零
        if (Input.GetKeyUp(KeyCode.R))
        {
            rKeyTimer = 0.0f;
        }
    }




    //現在是個人人皆可調用的工具(好欸)
    //藉由xBoard和yBoard，計算出棋子在世界[座標]的位置。是座標!!!
    //注意不要與Game.cs裡的SetPosition混淆，SstPosition適用於紀錄剛剛從Create()造出來的Gameobject在chess
    public static void SetCoord(int anyXBoard, int anyYBoard, GameObject obj, bool boardIsFlipped )
    {
        if(boardIsFlipped)
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
        //Debug.Log($"{obj}放置在({x},{y})");
    }

    public void ResetGameBoard()
    {
        //清空棋盤
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                gameBoard[x, y] = null;
            }
        }
        CreateSChessPiece(PieceType.WhiteKing, 2, 2, true);
        CreateSChessPiece(PieceType.BlackKing, 5, 5, true);
        //在棋盤右邊建立黑棋與白棋的選擇
        CreateSChessPiece(PieceType.WhiteQueen, 8, 7, false);
        CreateSChessPiece(PieceType.WhiteRook, 8, 6, false);
        CreateSChessPiece(PieceType.WhiteBishop, 8, 5, false);
        CreateSChessPiece(PieceType.WhiteKnight, 8, 4, false);
        CreateSChessPiece(PieceType.WhitePawn, 8, 3, false);
        
        CreateSChessPiece(PieceType.BlackQueen, 9, 7, false);
        CreateSChessPiece(PieceType.BlackRook, 9, 6, false);
        CreateSChessPiece(PieceType.BlackBishop, 9, 5, false);
        CreateSChessPiece(PieceType.BlackKnight, 9, 4, false);
        CreateSChessPiece(PieceType.BlackPawn, 9, 3, false);
    }

    public void CreateSChessPiece(PieceType pieceType, int xBoard, int yBoard, bool isReal)
    {
        GameObject chessPiece = Instantiate(SChessPiece, new Vector3(xBoard, yBoard, -1), Quaternion.identity);
        chessPiece.GetComponent<SChessMan>().pieceType = pieceType;
        chessPiece.GetComponent<SChessMan>().isReal = isReal;
        chessPiece.GetComponent<SChessMan>().xBoard = xBoard;
        chessPiece.GetComponent<SChessMan>().yBoard = yBoard;
        chessPiece.GetComponent<SChessMan>().Activate();
        SetCoord(xBoard, yBoard, chessPiece, boardIsFlipped);
        if (isReal)
        {
            SetPosition(chessPiece);
        }
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
                SChessMan scm = piece.GetComponent < SChessMan > ();
                if (scm != null)
                {
                    SetCoord(scm.xBoard, scm.yBoard, piece, boardIsFlipped);
                }
            }
        }
    }

}
