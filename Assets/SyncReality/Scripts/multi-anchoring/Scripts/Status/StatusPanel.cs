using TMPro;
using UnityEngine;


public class StatusPanel : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private int _index;

    [SerializeField]
    private GameObject statusText;

    private void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        StatusManager.Instance.onStatusAdded += CreateStausText;
        SelfTest();
    }

    private void CreateStausText(string text, Color color)
    {
        var nesStatusText_0 = Instantiate(statusText, transform);
        var statusText_0 = nesStatusText_0.GetComponent<StatusText>();
        statusText_0.SetStausText(text, color);
    }

    private void SelfTest()
    {
        CreateStausText("Status 1", Color.red);
        CreateStausText("Status 2", Color.green);
    }

    //[ContextMenu("SelfTest")]
    //private void SelfTest()
    //{
    //    var text_0 = "Anchor is saved.";
    //    var color_0 = Color.green;
    //    var nesStatusText_0 = Instantiate(statusText, transform);
    //    var statusText_0 = nesStatusText_0.GetComponent<StatusText>();
    //    statusText_0.SetStausText(text_0, color_0);

    //    var text_1 = "Anchor is deleted.";
    //    var color_1 = Color.red;
    //    var nesStatusText_1 = Instantiate(statusText, transform);
    //    var statusText_1 = nesStatusText_1.GetComponent<StatusText>();
    //    statusText_1.SetStausText(text_1, color_1);

    //    var text_2 = "Safety Guardian has all values needed.";
    //    var color_2 = Color.yellow;
    //    var nesStatusText_2 = Instantiate(statusText, transform);
    //    var statusText_2 = nesStatusText_2.GetComponent<StatusText>();
    //    statusText_2.SetStausText(text_2, color_2);

    //    var text_3 = "Object translation force start.";
    //    var color_3 = Color.green;
    //    var nesStatusText_3 = Instantiate(statusText, transform);
    //    var statusText_3 = nesStatusText_3.GetComponent<StatusText>();
    //    statusText_3.SetStausText(text_3, color_3);
    //}

    //private void SelfTest()
    //{
    //    StatusManager.Instance.OnStatusAdded("Anchor is saved.");
    //    StatusManager.Instance.OnStatusAdded("Anchor is deleted.");
    //    StatusManager.Instance.OnStatusAdded("Safety Guardian has all values needed.");
    //    StatusManager.Instance.OnStatusAdded("Object translation force start.");
    //}

    private void OnDestroy()
    {
        StatusManager.Instance.onStatusAdded -= AddStatus;
    }

    private void AddStatus(string status, Color color)
    {
        
        _text.text += "Status " + _index + ": " + status + "\n";
        _index++;
    }
}
