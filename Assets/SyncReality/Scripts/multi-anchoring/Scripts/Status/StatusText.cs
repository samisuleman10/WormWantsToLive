using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class StatusText : MonoBehaviour
{
    private TextMeshProUGUI _text;
    string text;
    Color color;

    private void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    public void SetStausText(string text, Color color)
    {
        this.text = text;
        this.color = color;

        //DoThis();
        Invoke(nameof(DoThis),1);
        
    }

    private void DoThis()
    {
        Debug.Log("From StatusText, _text == null" + _text == null);
        Debug.Log("From StatusText, text = " + text);
        Debug.Log("From StatusText, color = " + color.ToString());
        _text.SetText(text);
        _text.color = color;
    }
}
