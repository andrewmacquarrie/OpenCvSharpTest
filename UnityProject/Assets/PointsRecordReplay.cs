using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;
using System.Xml.Serialization;
using System.IO;
using UnityEditor;

public class PointsRecordReplay : MonoBehaviour {
    public CrossHair pointsHolder;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.R))
        {
            var fileName = EditorUtility.SaveFilePanel("Choose the file path", "", "pointsRecording", "xml");

            var imagePoints = pointsHolder.GetImagePoints();
            var worldPoints = pointsHolder.GetWorldPoints();
            var normalizedImagePoints = pointsHolder.GetNormalizedImagePoints();
            SaveToFile(imagePoints, normalizedImagePoints, worldPoints, fileName);

            Debug.Log("Recording complete");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            var fileName = EditorUtility.OpenFilePanel("Choose the file path", "", "xml");
            var recording = Recording.LoadFromFile(fileName);
            pointsHolder.ReplayRecordedPoints(recording.worldPointsV3, recording.imagePointsV3);

            Debug.Log("Loading complete");
        }
	}

    // (imagePoints, normalizedImagePoints, worldPoints, filenamePrefix
    private static void SaveToFile(List<Vector3> imagePoints, List<Vector3> normalizedImagePoints, List<Vector3> worldPoints, string fileName)
    {
        var recording = new Recording(imagePoints, normalizedImagePoints, worldPoints);
        recording.SaveToFile(fileName);
    }
}
