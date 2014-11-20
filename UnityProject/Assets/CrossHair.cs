using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrossHair : MonoBehaviour
{
    public Texture2D crosshairTexture;
    public TestScript plugin;
    public CalibrationCpp cppPlugin;
    List<Vector3> _objectPositions = new List<Vector3>();
    List<Vector3> _imagePositions = new List<Vector3>();
    List<Vector3> _normalizedImagePositions = new List<Vector3>();
    public int minNumberOfPoints = 10;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void OnGUI()
    {
        var hh = crosshairTexture.height / 2;
        var hw = crosshairTexture.width / 2;

        var pos = Input.mousePosition;
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
                    plugin.calibrateFromCorrespondences(_imagePositions, _objectPositions, false);
                }
            }
        }
    }

    private Vector3 normalise(Vector3 pos)
    {
        return new Vector3(pos.x / (float) Screen.width, pos.y / (float) Screen.height);
    }
}