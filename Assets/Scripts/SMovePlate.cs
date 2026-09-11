using UnityEngine;

public class SMovePlate : MonoBehaviour
{
    public GameObject controller;  // = GameObject.FindGameObjectWithTag("GameController");
    //public GameObject movePlate; //movePlate代表一個棋子之後能走到哪裡，可能點擊之後下棋


    //為哪個棋子的MovePlate，一個棋子可能有多個MovePlate
    GameObject reference = null; //沒寫visibility代表private


    //應該是MovePlate的座標，因為MovePlate是棋子之後能走到哪裡的標記
    public int xBoard { get; set;} = -1; //預設在x on the board會是-1，代表還不存在在board上
    public int yBoard { get; set;} = -1;


    //true為攻擊，false為移動
    //給king用的
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
        


        Controller con = controller.GetComponent<Controller>();
        SChessMan rscm = reference.GetComponent<SChessMan>();

        //con.SaveGameBoard();
        //con.howManyMoves++;
        //點擊後還需要設定Queen、Rook、Knight、Bishop的reference的isUsed為true、isReal為true
        //點擊後要判定一下敵方king是否被拖入，重設kingIsAttacked
        //Pawn的話不用管isUsed，只設定isReal為true，因為Pawn是可以重複使用的
        switch (reference.GetComponent<SChessMan>().pieceType)
        {
            //擺放棋子
            case PieceType.WhiteQueen:
            case PieceType.BlackQueen:
            case PieceType.WhiteRook:
            case PieceType.BlackRook:
            case PieceType.WhiteKnight:
            case PieceType.BlackKnight:
            case PieceType.WhiteBishop:
            case PieceType.BlackBishop:
                reference.GetComponent<SChessMan>().isUsed = true;
                reference.GetComponent<SChessMan>().isReal = true;
                break;
            case PieceType.WhitePawn:
                GameObject wp = con.CreateSChessPiece(PieceType.WhitePawn, xBoard, yBoard, true);
                SetUpAll(con);
                return;
                
            case PieceType.BlackPawn:
                GameObject bp = con.CreateSChessPiece(PieceType.BlackPawn, xBoard, yBoard, true);
                SetUpAll(con);
                return;





            //移動king
            case PieceType.WhiteKing:
            case PieceType.BlackKing:


                
                if (isAttack) //吃子後可以回收
                {
                    //找到掛在GameController上的Game.cs，並呼叫MovePlate.cs裡的reference的ChessMan.cs裡的Move()函式
                    GameObject cp = con.GetPosition(xBoard, yBoard); //用的是MovePlate自己的xBoard yBoard
                    con.SetPositionEmpty(xBoard, yBoard);
                    PieceType cpPieceType = cp.GetComponent<SChessMan>().pieceType;
                    Destroy(cp);
                    switch (cpPieceType)
                    {
                        case PieceType.WhiteRook: con.CreateSChessPiece(PieceType.WhiteRook, 8, 6, false); break;
                        case PieceType.WhiteKnight: con.CreateSChessPiece(PieceType.WhiteKnight, 8, 4, false); break;
                        case PieceType.WhiteBishop: con.CreateSChessPiece(PieceType.WhiteBishop, 8, 5, false); break;
                        case PieceType.WhiteQueen: con.CreateSChessPiece(PieceType.WhiteQueen, 8, 7, false); break;
                        case PieceType.BlackRook: con.CreateSChessPiece(PieceType.BlackRook, 9, 6, false); break;
                        case PieceType.BlackKnight: con.CreateSChessPiece(PieceType.BlackKnight, 9, 4, false); break;
                        case PieceType.BlackBishop: con.CreateSChessPiece(PieceType.BlackBishop, 9, 5, false); break;
                        case PieceType.BlackQueen: con.CreateSChessPiece(PieceType.BlackQueen, 9, 7, false); break;


                    }
                }


                //把opponantKing移動到現在movePlate的輻射對稱點
                GameObject opponantKing = null;
                GameObject myKing = null;
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (con.gameBoard[i, j] != null)
                        {
                            if (rscm.player == "white" && con.gameBoard[i, j].GetComponent<SChessMan>().pieceType == PieceType.BlackKing
                                ||rscm.player == "black" && con.gameBoard[i, j].GetComponent<SChessMan>().pieceType == PieceType.WhiteKing)
                            {
                                opponantKing = con.gameBoard[i, j];
                                //Debug.Log($"有抓到opponantKing為{opponantKing}");
                            }
                            if (rscm.player == "white" && con.gameBoard[i, j].GetComponent<SChessMan>().pieceType == PieceType.WhiteKing
                                || rscm.player == "black" && con.gameBoard[i, j].GetComponent<SChessMan>().pieceType == PieceType.BlackKing)
                            {
                                myKing = con.gameBoard[i, j];
                                //Debug.Log($"有抓到opponantKing為{opponantKing}");
                            }
                        }
                            
                    }

                }
                //先把東西刪了
                if(con.GetPosition(7 - xBoard, 7 - yBoard) != null)
                {
                    GameObject cp = con.GetPosition(7 - xBoard, 7 - yBoard);        //用的是MovePlate自己的xBoard yBoard
                    con.SetPositionEmpty(7- xBoard, 7 - yBoard);

                    PieceType cpPieceType = cp.GetComponent<SChessMan>().pieceType;
                    Destroy(cp);
                }
                

                SChessMan oscm = opponantKing.GetComponent<SChessMan>();
                SChessMan mscm = myKing.GetComponent<SChessMan>();
                con.SetPositionEmpty(oscm.xBoard, oscm.yBoard);
                oscm.xBoard = 7 - xBoard;
                oscm.yBoard = 7 - yBoard;
                Controller.SetCoord(oscm.xBoard, oscm.yBoard, opponantKing, con.boardIsFlipped); //把棋子放到棋盤上
                con.SetPosition(opponantKing);

                //Debug.Log($"原本的xBoard、yBoard為{xBoard}, {yBoard}");
                //Debug.Log($"把oppoKing拖去{oscm.xBoard}, {oscm.yBoard}");


                //在後面生成Pawn軌跡
                GameObject tp = con.CreateSChessPiece(mscm.player == "white"? PieceType.WhitePawn: PieceType.BlackPawn, rscm.xBoard, rscm.yBoard, true);


                //註銷reference在gameBoard上的登記，但reference仍然存在於原本的地方
                con.SetPositionEmpty(rscm.xBoard, rscm.yBoard);

                //把reference的棋子移動到MovePlate的位置
                rscm.xBoard = xBoard; //用的是MovePlate自己的xBoard yBoard
                rscm.yBoard = yBoard;
                Controller.SetCoord(rscm.xBoard, rscm.yBoard, reference, controller.GetComponent<Controller>().boardIsFlipped); //把棋子放到棋盤上

                con.SetPosition(reference); //把reference放到gameBoard上
                con.SetPosition(tp);//把pawn軌跡放到gameBoard上



                SetUpAll(con);
                return;
        }

        //註銷reference在gameBoard上的登記，但reference仍然存在於原本的地方
        con.SetPositionEmpty(rscm.xBoard, rscm.yBoard);
        
        //把reference的棋子移動到MovePlate的位置
        rscm.xBoard = xBoard; //用的是MovePlate自己的xBoard yBoard
        rscm.yBoard = yBoard;
        Controller.SetCoord(rscm.xBoard, rscm.yBoard, reference, controller.GetComponent<Controller>().boardIsFlipped); //把棋子放到棋盤上

        con.SetPosition(reference); //把reference放到gameBoard上



        SetUpAll(con);
    }

    public void SetReference(GameObject obj) => reference = obj; //哪個棋子的MovePlate
    public GameObject GetReference() => reference; //哪個棋子的MovePlate

    public void SetUpAll(Controller con)
    {
        //再來要刪除所有MovePlate，DestroyMovePlate()
        SChessMan.DestroyMovePlate();
        //三元運算子更快?
        con.currentPlayer = con.currentPlayer == "white" ? "black" : "white";
        con.UpdateOppoPieceSquares();
        con.UpdateAttackSquares();
        con.CheckIfKingIsAttacked();
        con.CheckIfGameIsOver();
        //把存檔拉到下面
        con.howManyMoves++;
        con.SaveGameBoard();
    }

    
    









}


