using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrossHair : MonoBehaviour
{
    public Texture2D crosshairTexture;
    public Calibration plugin;
    List<Vector3> _objectPositions = new List<Vector3>();
    List<Vector3> _imagePositions = new List<Vector3>();
    List<Vector3> _normalizedImagePositions = new List<Vector3>();
    public int minNumberOfPoints = 8;
    private bool occludeWorld;

    // Use this for initialization
    void Start()
    {
        occludeWorld = false;
    }

    // Update is called once per frame
    void OnGUI()
    {
        var hh = crosshairTexture.height / 2;
        var hw = crosshairTexture.width / 2;

        var pos = Input.mousePosition;
        if (occludeWorld)
        {
            Texture2D blackTexture = new Texture2D(1, 1);
            blackTexture.SetPixel(0, 0, Color.black);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
        }
        GUI.DrawTexture(new Rect(pos.x - hw, Screen.height - pos.y - hh, crosshairTexture.width, crosshairTexture.height), crosshairTexture);

        _imagePositions.ForEach(delegate(Vector3 position)
        {
            int imagePointMarkerWidth = 5;
            GUI.DrawTexture(new Rect(position.x - imagePointMarkerWidth, position.y - imagePointMarkerWidth, imagePointMarkerWidth * 2, imagePointMarkerWidth * 2), crosshairTexture);
        });


        GUI.Box(new Rect(10, 10, 100, 90), _imagePositions.Count.ToString());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (Input.GetMouseButtonDown(0))
        {
            if (_imagePositions.Count == _objectPositions.Count)
            {
                // We have the same number of object and image positions, so we are starting a new correspondence. First is the object position
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit3d;
                if (Physics.Raycast(ray, out hit3d))
                {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position = hit3d.point;
                    _objectPositions.Add(hit3d.point);
                    occludeWorld = true;
                }
            }
            else
            {
                // we already have an object position, now we collect the 2D correspondence
                Vector3 pos = Input.mousePosition;
                pos.y = Screen.height - pos.y; // note the screen pos starts bottom left. We want top left origin
                _imagePositions.Add(pos);
               _normalizedImagePositions.Add(normalise(pos));

                if (_imagePositions.Count >= minNumberOfPoints)
                {
                    plugin.calibrateFromCorrespondences(_normalizedImagePositions, _objectPositions, true);
                }
                occludeWorld = false;
            }
        }
    }

    private Vector3 normalise(Vector3 pos)
    {
        return new Vector3(pos.x / (float) Screen.width, pos.y / (float) Screen.height);
    }
}