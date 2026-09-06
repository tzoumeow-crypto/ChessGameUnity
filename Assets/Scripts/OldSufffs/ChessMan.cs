using UnityEngine;

//據說是控制單一棋子行為

public class ChessMan : MonoBehaviour
{
    //pieceType，用於在Inspector直接下拉是選單
    public PieceType pieceType;
    //reference
    public GameObject controller;  // = GameObject.FindGameObjectWithTag("GameController");
    //public GameObject movePlate; //movePlate代表一個棋子之後能走到哪裡，可能點擊之後下棋

    //Positions
    private int xBoard = -1; //預設在x on the board會是-1，代表還不存在在board上
    private int yBoard = -1; //Tuple好像在Unity不能用，爛透了

    
    private string _player; //誰的棋子
    public string player { get => _player; set => _player = value; }

    //所有不同棋子的皮，要在Componant那邊有reference
    public Sprite whiteKing, whiteQueen, whiteBishop, whiteKnight, whiteRook, whitePawn;
    public Sprite blackKing, blackQueen, blackBishop, blackKnight, blackRook, blackPawn;



    //外部取得this的xBoard和yBoard
    public int GetXBoard() => xBoard;
    public int GetYBoard() => yBoard;
    //外部設定this的xBoard和yBoard
    public void SetXBoard(int x) => xBoard = x;
    public void SetYBoard(int y) => yBoard = y;

    




    public void Activate()
    {
        //由於FindGameObjectWithTag函式需要存取 Unity 引擎底層的場景資料，Unity不能在欄位上就直接初始化，初始劃一定要在方法內。
        //一般 private GameObject[] whitePieces = new GameObject[16];還是可以的。
        controller = GameObject.FindGameObjectWithTag("GameController");
        //SetCoord()，把棋子放到棋盤上
        Game.SetCoord(xBoard, yBoard, gameObject);
        

        //public T GetComponent<T>(); // T 代表你傳入的任何型別，<>內吃的是型別
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();

        //switch控制sprite，使用Enum，用法是[Enum的那個class].[Enum的那個值]，而非變數
        //原本的方法是利用this.name控制

        switch (pieceType)
        {
            case PieceType.WhiteKing:   renderer.sprite = whiteKing;    _player = "white"; break;
            case PieceType.WhiteQueen:  renderer.sprite = whiteQueen;   _player = "white"; break;
            case PieceType.WhiteBishop: renderer.sprite = whiteBishop;  _player = "white"; break;
            case PieceType.WhiteKnight: renderer.sprite = whiteKnight;  _player = "white"; break;
            case PieceType.WhiteRook:   renderer.sprite = whiteRook;    _player = "white"; break;
            case PieceType.WhitePawn:   renderer.sprite = whitePawn;    _player = "white"; break;
            case PieceType.BlackKing:   renderer.sprite = blackKing;    _player = "black"; break;
            case PieceType.BlackQueen:  renderer.sprite = blackQueen;   _player = "black"; break;
            case PieceType.BlackBishop: renderer.sprite = blackBishop;  _player = "black"; break;
            case PieceType.BlackKnight: renderer.sprite = blackKnight;  _player = "black"; break;
            case PieceType.BlackRook:   renderer.sprite = blackRook;    _player = "black"; break;
            case PieceType.BlackPawn:   renderer.sprite = blackPawn;    _player = "black"; break;
            default: break;
        }
    }



    private void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        Debug.Log($"Now is {controller.GetComponent<Game>().currentPlayer} to play");
        //摧毀原本存在於board上的所有movePlate
        DestroyMovePlate();
        //創造所有應該出現的MovePlates
        CreateMovePlates();
        
    }


    public void DestroyMovePlate()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]);
        }
    }

    public void CreateMovePlates()
    {
        //controller是GameObject，要調用Game的東西要使用GetComponant找下面的Script
        controller = GameObject.FindGameObjectWithTag("GameController");
        Game sc = controller.GetComponent<Game>();
        switch (sc.currentPlayer)
        {
            case "white":
                switch (pieceType)
                {
                    case PieceType.WhiteKing:
                        sc.KingMovePlate(gameObject);
                        break;
                    case PieceType.WhiteQueen:
                        sc.QueenMovePlate(gameObject);
                        break;
                    case PieceType.WhiteBishop:
                        sc.BishopMovePlate(gameObject);
                        break;
                    case PieceType.WhiteKnight:
                        sc.KnightMovePlate(gameObject);
                        break;
                    case PieceType.WhiteRook:
                        sc.RookMovePlate(gameObject);
                        break;
                    case PieceType.WhitePawn:
                        sc.PawnMovePlate(gameObject);
                        break;
                }
                break;
            case "black":
                switch (pieceType)
                {
                    case PieceType.BlackKing:
                        sc.KingMovePlate(gameObject);
                        break;
                    case PieceType.BlackQueen:
                        sc.QueenMovePlate(gameObject);
                        break;
                    case PieceType.BlackBishop:
                        sc.BishopMovePlate(gameObject);
                        break;
                    case PieceType.BlackKnight:
                        sc.KnightMovePlate(gameObject);
                        break;
                    case PieceType.BlackRook:
                        sc.RookMovePlate(gameObject);
                        break;
                    case PieceType.BlackPawn:
                        sc.PawnMovePlate(gameObject);
                        break;
                }
                break;

        }

        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            movePlates[i].GetComponent<MovePlate>().SetReference(gameObject);
            movePlates[i].transform.position = new Vector3(movePlates[i].transform.position.x, movePlates[i].transform.position.y, -2);
        }
    }







}
