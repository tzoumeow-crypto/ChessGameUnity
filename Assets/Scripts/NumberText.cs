using UnityEngine;

public class NumberText : MonoBehaviour
{
    public GameObject controller;

    public NumberTextEnum numberTextEnum;
    public Sprite number1, number2, number3, number4, number5, number6, number7, number8;
    public Sprite textA, textB, textC, textD, textE, textF, textG, textH;

    public int xBoard { get; set; }
    public int yBoard { get; set; }

    public void Activate()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");

        //改變自己的Sprite
        switch (numberTextEnum)
        {
            case NumberTextEnum.Number1: GetComponent<SpriteRenderer>().sprite = number1; break;
            case NumberTextEnum.Number2: GetComponent<SpriteRenderer>().sprite = number2; break;
            case NumberTextEnum.Number3: GetComponent<SpriteRenderer>().sprite = number3; break;
            case NumberTextEnum.Number4: GetComponent<SpriteRenderer>().sprite = number4; break;
            case NumberTextEnum.Number5: GetComponent<SpriteRenderer>().sprite = number5; break;
            case NumberTextEnum.Number6: GetComponent<SpriteRenderer>().sprite = number6; break;
            case NumberTextEnum.Number7: GetComponent<SpriteRenderer>().sprite = number7; break;
            case NumberTextEnum.Number8: GetComponent<SpriteRenderer>().sprite = number8; break;
            case NumberTextEnum.TextA: GetComponent<SpriteRenderer>().sprite = textA; break;
            case NumberTextEnum.TextB: GetComponent<SpriteRenderer>().sprite = textB; break;
            case NumberTextEnum.TextC: GetComponent<SpriteRenderer>().sprite = textC; break;
            case NumberTextEnum.TextD: GetComponent<SpriteRenderer>().sprite = textD; break;
            case NumberTextEnum.TextE: GetComponent<SpriteRenderer>().sprite = textE; break;
            case NumberTextEnum.TextF: GetComponent<SpriteRenderer>().sprite = textF; break;
            case NumberTextEnum.TextG: GetComponent<SpriteRenderer>().sprite = textG; break;
            case NumberTextEnum.TextH: GetComponent<SpriteRenderer>().sprite = textH; break;
        }
        transform.localScale = new Vector3(3f, 3f, 1);
    }
}
