using UnityEngine;
using System.Collections;

public class TextureCube : MonoBehaviour {

	// Use this for initialization
    void Start()
    {

        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = null;
        if (mf != null)
        {
            mesh = mf.mesh;
            if (mesh == null || mesh.uv.Length != 24)
            {
                Debug.Log("Script needs to be attached to built-in cube");
                return;
            }
        }

        if (mesh == null)
            return;

        var uvs = mesh.uv;

        // Front
        uvs[0] = new Vector2(0.0f, 0.0f);
        uvs[1] = new Vector2(0.333f, 0.0f);
        uvs[2] = new Vector2(0.0f, 0.333f);
        uvs[3] = new Vector2(0.333f, 0.333f);

        // Top
        uvs[8] = new Vector2(0.334f, 0.0f);
        uvs[9] = new Vector2(0.666f, 0.0f);
        uvs[4] = new Vector2(0.334f, 0.333f);
        uvs[5] = new Vector2(0.666f, 0.333f);

        // Back
        uvs[10] = new Vector2(0.667f, 0.0f);
        uvs[11] = new Vector2(1.0f, 0.0f);
        uvs[6] = new Vector2(0.667f, 0.333f);
        uvs[7] = new Vector2(1.0f, 0.333f);

        // Bottom
        uvs[12] = new Vector2(0.0f, 0.334f);
        uvs[14] = new Vector2(0.333f, 0.334f);
        uvs[15] = new Vector2(0.0f, 0.666f);
        uvs[13] = new Vector2(0.333f, 0.666f);

        // Left
        uvs[16] = new Vector2(0.334f, 0.334f);
        uvs[18] = new Vector2(0.666f, 0.334f);
        uvs[19] = new Vector2(0.334f, 0.666f);
        uvs[17] = new Vector2(0.666f, 0.666f);

        // Right        
        uvs[20] = new Vector2(0.667f, 0.334f);
        uvs[22] = new Vector2(1.00f, 0.334f);
        uvs[23] = new Vector2(0.667f, 0.666f);
        uvs[21] = new Vector2(1.0f, 0.666f);

        mesh.uv = uvs;
    }

    IEnumerator Fade()
    {
        for (float f = 1f; f <= 180; f += 1f)
        {
            this.transform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), 1.0f);
            yield return new WaitForSeconds(.001f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("s"))
        {
            StartCoroutine("Fade");
        }
    }
}

