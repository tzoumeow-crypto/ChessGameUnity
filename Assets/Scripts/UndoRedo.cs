using UnityEngine;

public class UndoRedo : MonoBehaviour
{
    public GameObject controller;
    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        Controller con = controller.GetComponent<Controller>();
        if (gameObject.name == "undo_0")
        {
            Debug.Log("呼叫Undo");
            con.UndoRedo(true);
        }
        else if (gameObject.name == "redo_0")
        {
            con.UndoRedo(false);
        }
    }
}
