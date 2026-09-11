using UnityEngine;

public class SChessMan : MonoBehaviour
{
    public PieceType pieceType;
    //reference
    public GameObject controller;  // = GameObject.FindGameObjectWithTag("GameController");
    //public GameObject movePlate; //movePlate代表一個棋子之後能走到哪裡，可能點擊之後下棋

    //Positions
    public int xBoard { get; set; } = -1; //預設在x on the board會是-1，代表還不存在在board上
    public int yBoard { get; set; } = -1; //Tuple好像在Unity不能用，爛透了

    public bool isReal { get; set; } = true; //是否是棋盤上的棋子，還是右邊的選擇棋子
    public bool isUsed { get; set; }

    public bool kingIsAttacked { get; set; } = false; //是否被攻擊，給king用的



    public string player { get; set; } = ""; //誰的棋子

    //所有不同棋子的皮，要在Componant那邊有reference
    public Sprite whiteKing, whiteQueen, whiteBishop, whiteKnight, whiteRook, whitePawn;
    public Sprite blackKing, blackQueen, blackBishop, blackKnight, blackRook, blackPawn;


    public void Start()
    {
        if (isUsed)
        {

        }
    }



    public void Activate()
    {
        //由於FindGameObjectWithTag函式需要存取 Unity 引擎底層的場景資料，Unity不能在欄位上就直接初始化，初始劃一定要在方法內。
        //一般 private GameObject[] whitePieces = new GameObject[16];還是可以的。
        //controller = GameObject.FindGameObjectWithTag("GameController");
        switch(pieceType)
        {
            case PieceType.WhiteKing:   GetComponent<SpriteRenderer>().sprite = whiteKing;      player = "white"; break;
            case PieceType.WhiteQueen:  GetComponent<SpriteRenderer>().sprite = whiteQueen;     player = "white"; break;
            case PieceType.WhiteBishop: GetComponent<SpriteRenderer>().sprite = whiteBishop;    player = "white"; break;
            case PieceType.WhiteKnight: GetComponent<SpriteRenderer>().sprite = whiteKnight;    player = "white"; break;
            case PieceType.WhiteRook:   GetComponent<SpriteRenderer>().sprite = whiteRook;      player = "white"; break;
            case PieceType.WhitePawn:   GetComponent<SpriteRenderer>().sprite = whitePawn;      player = "white"; break;
            case PieceType.BlackKing:   GetComponent<SpriteRenderer>().sprite = blackKing;      player = "black"; break;
            case PieceType.BlackQueen:  GetComponent<SpriteRenderer>().sprite = blackQueen;     player = "black"; break;
            case PieceType.BlackBishop: GetComponent<SpriteRenderer>().sprite = blackBishop;    player = "black"; break;
            case PieceType.BlackKnight: GetComponent<SpriteRenderer>().sprite = blackKnight;    player = "black"; break;
            case PieceType.BlackRook:   GetComponent<SpriteRenderer>().sprite = blackRook;      player = "black"; break;
            case PieceType.BlackPawn:   GetComponent<SpriteRenderer>().sprite = blackPawn;      player = "black"; break;
        }

    }

    private void OnMouseDown()


    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        Controller con = controller.GetComponent<Controller>();

        //這裡才計算有點太慢了?改在SMovePlate就計算
        //con.UpdateAttackSquares();
        //con.UpdateOppoPieceSquares();
        DestroyMovePlate();
        if (isReal && (pieceType == PieceType.WhiteKing || pieceType == PieceType.BlackKing))
        {
            
            //Debug.Log($"Now is {con.currentPlayer} to play");
            //摧毀原本存在於board上的所有movePlate

            //創造所有應該出現的MovePlates
            if (player == con.currentPlayer)
            {
                con.KingMovePlate(gameObject, false);
                SetReference(gameObject);
            }
            

        }
        //如果是右邊的選擇棋子，且還沒被使用過，就可以被使用
        else if (!isReal && !isUsed)
        {
            
            //Debug.Log($"Now is {con.currentPlayer} to play");
            if(player == con.currentPlayer)
            {
                
                //創造所有應該出現的MovePlates
                CreateMovePlates();
                SetReference(gameObject);
            }
        }

    }


    public static void DestroyMovePlate()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        foreach (GameObject movePlate in movePlates)
        {
            Destroy(movePlate);
        }
    }

    //這裡Create的是一班攻擊piece的MovePlate，並不是King的MovePlate
    private void CreateMovePlates() 
    {
        //controller是GameObject，要調用Game的東西要使用GetComponant找下面的Script
        controller = GameObject.FindGameObjectWithTag("GameController");
        Controller con = controller.GetComponent<Controller>();
        con.CalMovePlate(gameObject);

        
        
    }

    private void SetReference(GameObject obj)
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            movePlates[i].GetComponent<SMovePlate>().SetReference(gameObject);
            movePlates[i].transform.position = new Vector3(movePlates[i].transform.position.x, movePlates[i].transform.position.y, -2);
        }
    }





}

