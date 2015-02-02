using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

// TODO: refactor so _height - pos.y is abstracted away

public class CrossHair : MonoBehaviour
{
    public Texture2D crosshairTexture;
    public Calibration _calibrator;
    public int minNumberOfPoints = 8;
    public Camera _mainCamera;
    public Camera _testCamera;

    List<Vector3> _objectPositions = new List<Vector3>();
    List<Vector3> _imagePositions = new List<Vector3>();
    private bool _occludeWorld;
    private int _width;
    private int _height;
    private double _reprojError;
    private int? _dragging;

    void Start()
    {
        _occludeWorld = false;
        _reprojError = 0.0;
        _height = (int)Screen.height;
        _width = (int)Screen.width;
        _calibrator = new Calibration(_mainCamera, _testCamera);
    }

    void OnGUI()
    {
        var pos = Input.mousePosition;
        if (_occludeWorld)
        {
            Texture2D blackTexture = new Texture2D(1, 1);
            blackTexture.SetPixel(0, 0, Color.black);
            GUI.DrawTexture(new Rect(0, 0, _width, _height), blackTexture);
        }
        
        _imagePositions.ForEach(delegate(Vector3 position)
        {
            int imagePointMarkerWidth = 5;
            GUI.DrawTexture(new Rect(position.x - imagePointMarkerWidth, position.y - imagePointMarkerWidth, imagePointMarkerWidth * 2, imagePointMarkerWidth * 2), crosshairTexture);
        });

        if (ImagePointHighlighted())
        {
            int imagePointMarkerWidth = 10;
            Vector3 position = _imagePositions[ImagePointMouseHooveringOver().Value];
            GUI.DrawTexture(new Rect(position.x - imagePointMarkerWidth, position.y - imagePointMarkerWidth, imagePointMarkerWidth * 2, imagePointMarkerWidth * 2), crosshairTexture);
        }

        GUI.Box(new Rect(10, 10, 150, 90), "# points: " + _imagePositions.Count.ToString() + "\nReProj error: " + string.Format("{0:f2}", _reprojError));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (Input.GetMouseButtonDown(0))
        {
            if (ImagePointHighlighted())
            {
                // do something with highlighted point - allow click and drag?
                _dragging = ImagePointMouseHooveringOver();
            }
            else
            {
                if (_imagePositions.Count == _objectPositions.Count)
                {
                    // We have the same number of object and image positions, so we are starting a new correspondence. First is the object position
                    CaptureWorldPoint();
                }
                else
                {
                    // we already have an object position, now we collect the 2D correspondence
                    CaptureImagePoint();
                    TriggerCalibration();
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _dragging = null;
            TriggerCalibration();
        }

        if (_dragging != null)
        {
            var pos = Input.mousePosition;
            pos.y = _height - pos.y;
            _imagePositions[_dragging.Value] = pos;
        }
    }

    private bool ImagePointHighlighted()
    {
        return ImagePointMouseHooveringOver() != null;
    }

    private int? ImagePointMouseHooveringOver()
    {
        Vector3 pos = Input.mousePosition;
        int? imagePosMatch = null;
        for (int i = 0; i < _imagePositions.Count; i++)
        {
            Vector3 imagePos = _imagePositions[i];
            if (Mathf.Abs(imagePos.x - pos.x) + Mathf.Abs(imagePos.y - (_height - pos.y)) < 5)
            {
                imagePosMatch = i;
            }
        }
        return imagePosMatch;
    }

    private void CaptureWorldPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit3d;
        if (Physics.Raycast(ray, out hit3d))
        {
            CreateSphereAt(hit3d.point);
            _objectPositions.Add(hit3d.point);
            _occludeWorld = true;
        }
    }

    private void CaptureImagePoint()
    {
        Vector3 pos = Input.mousePosition;
        pos.y = _height - pos.y; // note the screen pos starts bottom left. We want top left origin
        _imagePositions.Add(pos);
        _occludeWorld = false;
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

        _reprojError = _calibrator.calibrateFromCorrespondences(_imagePositions, _objectPositions);
    }

    public List<Vector3> GetImagePoints()
    {
        return _imagePositions;
    }

    public List<Vector3> GetWorldPoints()
    {
        return _objectPositions;
    }

    public void ReplayRecordedPoints(List<Vector3> worldPoints, List<Vector3> imgPoints)
    {
        _imagePositions = imgPoints;
        _objectPositions = worldPoints;
        foreach (var point in worldPoints)
            CreateSphereAt(point);
        TriggerCalibration();
    }
}