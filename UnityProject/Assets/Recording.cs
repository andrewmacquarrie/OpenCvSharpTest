using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets
{
    [Serializable]
    public class Recording
    {
        // needed for serialization
        public Recording() { }

        public Recording(List<Vector3> ip, List<Vector3> nip, List<Vector3> wp){
            imagePoints = ConvertToSerializableList(ip);
            normalizedImagePoints = ConvertToSerializableList(nip);
            worldPoints = ConvertToSerializableList(wp);
        }

        public List<SerializableVector3> worldPoints;
        public List<SerializableVector3> imagePoints;
        public List<SerializableVector3> normalizedImagePoints;

        public List<Vector3> worldPointsV3
        {
            get {
                return ConvertToV3(worldPoints);
            }
        }

        public List<Vector3> imagePointsV3
        {
            get
            {
                return ConvertToV3(imagePoints);
            }
        }

        public List<Vector3> normalizedImagePointsV3
        {
            get
            {
                return ConvertToV3(normalizedImagePoints);
            }
        }

        private List<Vector3> ConvertToV3(List<SerializableVector3> sv3List)
        {
            var convList = new List<Vector3>();
            foreach (var sv3 in sv3List)
            {
                convList.Add(sv3.Vector3);
            }
            return convList;
        }

        private static List<SerializableVector3> ConvertToSerializableList(List<Vector3> imagePoints)
        {
            List<SerializableVector3> serializableImageList = new List<SerializableVector3>();
            foreach (var imagePoint in imagePoints)
            {
                serializableImageList.Add(new SerializableVector3(imagePoint));
            }
            return serializableImageList;
        }

        public void SaveToFile(string fileName)
        {
            System.IO.File.WriteAllText(fileName, this.SerializeObject());
        }

        public static Recording LoadFromFile(string fileName)
        {
            var data = System.IO.File.ReadAllText(fileName);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Recording));
            using (StringReader textReader = new StringReader(data))
            {
                return (Recording) xmlSerializer.Deserialize(textReader);
            }
        }

    }

    
    
}
