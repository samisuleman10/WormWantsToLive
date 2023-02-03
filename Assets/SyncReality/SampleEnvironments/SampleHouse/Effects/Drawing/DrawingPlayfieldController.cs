using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingPlayfieldController : MonoBehaviour
{
    public List<LineRenderer> lineRenderers = new List<LineRenderer>();
    public GameObject linePrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addNewLine(Vector3 p)
    {
        var l = Instantiate(linePrefab, Vector3.zero, transform.rotation, transform);
        l.transform.localEulerAngles = new Vector3(-90, 0, 0);
        l.GetComponent<LineRenderer>().positionCount = 1;
        l.GetComponent<LineRenderer>().SetPosition(0, p + transform.up * 0.005f);
        lineRenderers.Add(l.GetComponent<LineRenderer>());
    }

    public void Drag(Vector3 p)
    {
        var lr = lineRenderers[lineRenderers.Count - 1];
        lr.positionCount++;
        lr.SetPosition(lr.positionCount - 1, p + transform.up * 0.005f);
    }
}
