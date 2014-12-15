using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;
using System.Xml.Serialization;
using System.IO;

public class PointsRecordReplay : MonoBehaviour {
    public CrossHair pointsHolder;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.R))
        {
            var imagePoints = pointsHolder.GetImagePoints();
            SaveToFile(imagePoints, "imagePointsRecording.xml");

            var worldPoints = pointsHolder.GetWorldPoints();
            SaveToFile(worldPoints, "worldPointsRecording.xml");

            Debug.Log("Recording complete");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            pointsHolder.ReplayRecordedPoints(GetPointsFromFile("worldPointsRecording.xml"), GetPointsFromFile("imagePointsRecording.xml"));
        }
	}

    private List<Vector3> GetPointsFromFile(string filename)
    {
        var imagePointsString = System.IO.File.ReadAllText(filename);
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SerializableVector3>));
        using (StringReader textReader = new StringReader(imagePointsString))
        {
            var list = (List<SerializableVector3>)xmlSerializer.Deserialize(textReader);

            var convList = new List<Vector3>();
            foreach (var sv3 in list)
            {
                convList.Add(sv3.Vector3);
            }
            return convList;
        }
    }

    private static void SaveToFile(List<Vector3> imagePoints, string fileName)
    {
        List<SerializableVector3> serializableImageList = new List<SerializableVector3>();
        foreach (var imagePoint in imagePoints)
        {
            serializableImageList.Add(new SerializableVector3(imagePoint));
        }
        System.IO.File.WriteAllText(fileName, serializableImageList.SerializeObject());
    }


}
