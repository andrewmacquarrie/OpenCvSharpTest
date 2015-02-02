using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;
using System.Xml.Serialization;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PointsRecordReplay : MonoBehaviour {
    public CorrespondenceAcquisition pointsHolder;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.R))
        {
            string fileName;
            #if UNITY_EDITOR
                fileName = EditorUtility.SaveFilePanel("Choose the file path", "", "pointsRecording", "xml");
            #endif
            #if UNITY_EDITOR == false
                fileName = "recording.xml";
            #endif
            SaveToFile(pointsHolder.GetImagePoints(), pointsHolder.GetWorldPoints(), fileName);

            Debug.Log("Recording complete");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            string fileName;
            #if UNITY_EDITOR
                fileName = EditorUtility.OpenFilePanel("Choose the file path", "", "xml");
            #endif
            #if UNITY_EDITOR == false
                fileName = "recording.xml";
            #endif
            var recording = Recording.LoadFromFile(fileName);
            pointsHolder.ReplayRecordedPoints(recording.worldPointsV3, recording.ImagePointsV3((double)Screen.width, (double)Screen.height));

            Debug.Log("Loading complete");
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            string fileName;
            #if UNITY_EDITOR
                fileName = EditorUtility.OpenFilePanel("Choose the file path", "", "xml");
            #endif
            #if UNITY_EDITOR == false
                fileName = "UnitTest2.xml";
            #endif
            var recording = Recording.LoadFromFile(fileName);
            Screen.SetResolution(1906, 987, true);
            pointsHolder.ReplayRecordedPoints(recording.worldPointsV3, recording.ImagePointsV3((double)Screen.width, (double)Screen.height));

            Debug.Log("Loading complete");
        }
	}

    private static void SaveToFile(List<Vector3> imagePoints, List<Vector3> worldPoints, string fileName)
    {
        var recording = new Recording(imagePoints, worldPoints, (double)Screen.width, (double)Screen.height);
        recording.SaveToFile(fileName);
    }
}
