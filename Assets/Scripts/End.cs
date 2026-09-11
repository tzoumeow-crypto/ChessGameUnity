using UnityEngine;

public class End : MonoBehaviour
{
    public Sprite whiteWin, blackWin;
    
    public void Activate(string player)
    {
        switch (player)
        {
            case "white": GetComponent<SpriteRenderer>().sprite = blackWin; break;
            case "black": GetComponent<SpriteRenderer>().sprite = whiteWin; break;
        }
    }
    
}
