﻿using UnityEngine;
using System.Collections;

public class DrawLine : MonoBehaviour {
    private Vector3 _xyPos;
    private Material lineMaterial;

    void Start()
    {
        CreateLineMaterial();
    }

    private void CreateLineMaterial()
    {
        lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
            "SubShader { Pass { " +
            "    Blend SrcAlpha OneMinusSrcAlpha " +
            "    ZWrite Off Cull Off Fog { Mode Off } " +
            "    BindChannels {" +
            "      Bind \"vertex\", vertex Bind \"color\", color }" +
            "} } }");
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
    }

    public void SetXY(int x, int y)
    {
        _xyPos = new Vector3((float)x, (float)y, 0);
    }

    void Update()
    {
        _xyPos = Input.mousePosition;
    }

    IEnumerator OnPostRenderMeth()
    {
        float f = 0.1f;

        yield return new WaitForEndOfFrame();

        GL.PushMatrix();
        GL.LoadPixelMatrix();
        lineMaterial.SetPass(0);
        GL.LoadOrtho();
        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        GL.Vertex(new Vector3(_xyPos.x / (float)Screen.width, 0, 0));
        GL.Vertex(new Vector3(_xyPos.x / (float)Screen.width, 1, 0));

        GL.Color(Color.green);
        GL.Vertex(new Vector3(0, _xyPos.y / (float)Screen.height, 0));
        GL.Vertex(new Vector3(1, _xyPos.y / (float)Screen.height, 0));

        GL.End();
        GL.PopMatrix();
    }

    void OnPostRender()
    {
        StartCoroutine(OnPostRenderMeth());
        OnPostRenderMeth();
    }
}