using JetBrains.Annotations;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;  // = GameObject.FindGameObjectWithTag("GameController");
    //public GameObject movePlate; //movePlate代表一個棋子之後能走到哪裡，可能點擊之後下棋


    //為哪個棋子的MovePlate，一個棋子可能有多個MovePlate
    GameObject reference = null; //沒寫visibility代表private


    //應該是MovePlate的座標，因為MovePlate是棋子之後能走到哪裡的標記
    private int xBoard = -1; //預設在x on the board會是-1，代表還不存在在board上
    private int yBoard = -1;


    //true為攻擊，false為移動
    public bool isAttack = false;
    public void Start() //物件生成之函式完成時時觸發，是否是Attack在當下即可知曉
    {
        if (isAttack)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 0f, 0f, 255f);
        }
        //controller是GameObject，要調用Game的東西要使用GetComponant找下面的Script。reference也是同理
        controller = GameObject.FindGameObjectWithTag("GameController");
    }


    public void OnMouseUp()
    {
        
        if (isAttack)
        {
            //找到掛在GameController上的Game.cs，並呼叫MovePlate.cs裡的reference的ChessMan.cs裡的Move()函式
            GameObject cp = controller.GetComponent<Game>().GetPosition(xBoard, yBoard); //用的是MovePlate自己的xBoard yBoard
            Destroy(cp);
        }
        //註銷reference在gameBoard上的登記，但reference仍然存在於原本的地方
        ChessMan rcm = reference.GetComponent<ChessMan>();
        controller.GetComponent<Game>().SetPositionEmpty(rcm.GetXBoard(), rcm.GetYBoard());

        //把reference的棋子移動到MovePlate的位置
        rcm.SetXBoard(xBoard); //用的是MovePlate自己的xBoard yBoard
        rcm.SetYBoard(yBoard);
        Game.SetCoord(rcm.GetXBoard(), rcm.GetYBoard(), reference); //把棋子放到棋盤上

        controller.GetComponent<Game>().SetPosition(reference); //把reference放到gameBoard上

        //再來要刪除所有MovePlate，DestroyMovePlate()
        reference.GetComponent<ChessMan>().DestroyMovePlate();

        //三元運算子更快?
        controller.GetComponent<Game>().currentPlayer =
            controller.GetComponent<Game>().currentPlayer == "white" ? "black" : "white";
        //string currentPlayer = controller.GetComponent<Game>().currentPlayer;
        //if (currentPlayer == "white")
        //{
        //    controller.GetComponent<Game>().currentPlayer = "black";
        //}else if(currentPlayer == "black")
        //{
        //    controller.GetComponent<Game>().currentPlayer = "white";
        //}
    }
    public void SetPosition(int x, int y) //MovePlate的座標
    {
        xBoard = x;
        yBoard = y;
    }
    public void SetReference(GameObject obj) => reference = obj; //哪個棋子的MovePlate
    public GameObject GetReference() => reference; //哪個棋子的MovePlate
    //以後get set可以改用property








    
    
}
