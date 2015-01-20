using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CrossHair : MonoBehaviour
{
    public Texture2D crosshairTexture;
    public Calibration plugin;
    List<Vector3> _objectPositions = new List<Vector3>();
    List<Vector3> _imagePositions = new List<Vector3>();
    List<Vector3> _normalizedImagePositions = new List<Vector3>();
    public int minNumberOfPoints = 8;
    private bool occludeWorld;
    private bool usingNormalised = false;
    private int _width;
    private int _height;
    private double _reprojError;

    // Use this for initialization
    void Start()
    {
        occludeWorld = false;
        _reprojError = 0.0;

#if UNITY_EDITOR
        Vector2 hw = Handles.GetMainGameViewSize();
        _height = (int)hw.y;
        _width = (int)hw.x;
#endif
#if UNITY_EDITOR == false
        _height = (int)Screen.height;
        _width = (int)Screen.width;
#endif
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
            GUI.DrawTexture(new Rect(0, 0, _width, _height), blackTexture);
        }
        GUI.DrawTexture(new Rect(pos.x - hw, _height - pos.y - hh, crosshairTexture.width, crosshairTexture.height), crosshairTexture);

        _imagePositions.ForEach(delegate(Vector3 position)
        {
            int imagePointMarkerWidth = 5;
            GUI.DrawTexture(new Rect(position.x - imagePointMarkerWidth, position.y - imagePointMarkerWidth, imagePointMarkerWidth * 2, imagePointMarkerWidth * 2), crosshairTexture);
        });


        GUI.Box(new Rect(10, 10, 150, 90), "# points: " + _imagePositions.Count.ToString() + "\nReProj error: " + string.Format("{0:f2}", _reprojError));
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
                    CreateSphereAt(hit3d.point);
                    _objectPositions.Add(hit3d.point);
                    occludeWorld = true;
                }
            }
            else
            {
                // we already have an object position, now we collect the 2D correspondence
                Vector3 pos = Input.mousePosition;
                pos.y = _height - pos.y; // note the screen pos starts bottom left. We want top left origin
                _imagePositions.Add(pos);
               _normalizedImagePositions.Add(normalise(pos));

                TriggerCalibration();

                occludeWorld = false;
            }
        }
    }

    private static void CreateSphereAt(Vector3 point)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = point;
    }

    private void TriggerCalibration()
    {
        if (_imagePositions.Count < minNumberOfPoints)
            return;
        if (_imagePositions.Count != _objectPositions.Count)
            return;

        if(usingNormalised)
            _reprojError = plugin.calibrateFromCorrespondences(_normalizedImagePositions, _objectPositions, true);
        else
            _reprojError = plugin.calibrateFromCorrespondences(_imagePositions, _objectPositions, false);
    }

    private Vector3 normalise(Vector3 pos)
    {
        return new Vector3(pos.x / (float) _width, pos.y / (float) _height);
    }

    public List<Vector3> GetImagePoints()
    {
        return _imagePositions;
    }

    public List<Vector3> GetWorldPoints()
    {
        return _objectPositions;
    }

    public void ReplayRecordedPoints(List<Vector3> worldPoints, List<Vector3> imgPoints, List<Vector3> normImgPoints, bool usingNorm)
    {
        _imagePositions = imgPoints;
        _objectPositions = worldPoints;
        _normalizedImagePositions = normImgPoints;
        usingNormalised = usingNorm;
        foreach (var point in worldPoints)
            CreateSphereAt(point);
        TriggerCalibration();
    }

    public List<Vector3> GetNormalizedImagePoints()
    {
        return _normalizedImagePositions;
    }

    public bool  GetNormalizedFlag()
    {
        return usingNormalised;
    }
}