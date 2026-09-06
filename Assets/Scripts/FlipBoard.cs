using UnityEngine;

public class FlipBoard : MonoBehaviour
{
    public void OnMouseDown()
    {
        GameObject controller = GameObject.FindGameObjectWithTag("GameController");
        controller.GetComponent<Controller>().FlipBoard();
    }

}
