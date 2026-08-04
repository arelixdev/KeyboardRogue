using TMPro;
using UnityEngine;

public class KeyView : MonoBehaviour
{
    public char Character { get; private set; }
    public int Row { get; private set; }
    public int Col { get; private set; }

    private TMP_Text label;

    public void SetCharacter(char character, int row, int col)
    {
        Character = character;
        Row = row;
        Col = col;

        if (label == null)
            label = GetComponentInChildren<TMP_Text>();

        label.text = character.ToString();
        name = $"Key_{character}";
    }
}
