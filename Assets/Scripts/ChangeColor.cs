using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour {

    public MeshRenderer mainRenderer;
    public Color color;

    // Start is called before the first frame update
    void Start()
    {
        mainRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        var mat = mainRenderer.material;
        mat.color = color;
    }
}
