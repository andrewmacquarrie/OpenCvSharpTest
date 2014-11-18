using UnityEngine;
using System.Collections;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System;

public class CalibrationCpp : MonoBehaviour {

    public Camera projectorCamera;

    public void calibrateFromCorrespondences(List<Vector3> _imagePositions, List<Vector3> _objectPositions, bool usingNormalized)
    {
        double height = (double)Screen.height;
        double width = (double)Screen.width;
        int pointsCount = _imagePositions.Count;
        int ImageNum = 1;
        int[] pointCountsValue = new int[ImageNum];
        pointCountsValue[0] = pointsCount;

        IList<Point3d> coords = new List<Point3d>();

        Debug.Log("3D object coords");
        for (int i = 0; i < pointsCount; i++)
        {
            coords.Add(new Point3d(_objectPositions[i].x, _objectPositions[i].y, _objectPositions[i].z));
        }
        IList<IEnumerable<Point3d>> objectPoints = new List<IEnumerable<Point3d>>();
        objectPoints.Add(coords);

        Console.WriteLine("2D image coords");
        IList<Point2d> imgPoints = new List<Point2d>();
        for (int i = 0; i < _imagePositions.Count; i++)
        {
            imgPoints.Add(new Point2d(_imagePositions[i].x, _imagePositions[i].y));
        }
        IList<IEnumerable<Point2d>> imagePoints = new List<IEnumerable<Point2d>>();
        imagePoints.Add(imgPoints);

        //float fl = viewHeight / (2.0 * Mathf.Tan(0.5f * projectorCamera.fieldOfView * Mathf.Deg2Rad));

        Size size = new Size(width, height);

        double[] intrGuess;

            float fov = 60.0f;
            float focalLength = (float)height / (2.0f * Mathf.Tan(0.5f * fov * Mathf.Deg2Rad));

            double fx = focalLength;
            double fy = focalLength;

            double cy = height / 2.0;
            double cx = width / 2.0;

            if (usingNormalized)
            {
                fx = fx / width;
                fy = fy / height;
                cy = cy / height;
                cx = cx / width;
                size = new Size(1, 1);
            }

            intrGuess = new double[] { fx, 0.0, cx, 
            0.0, fy, cy, 
            0.0, 0.0, 1.0 };


        double[] distCoef = new double[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
        Vec3d[] rvecs = new Vec3d[1];
        Vec3d[] tvecs = new Vec3d[1];
        double[,] intrGuess2 = new double[3,3];
        intrGuess2[0,0] = intrGuess[0]; intrGuess2[0,1] = intrGuess[1]; intrGuess2[0,2] = intrGuess[2];
        intrGuess2[1,0] = intrGuess[3]; intrGuess2[1,1] = intrGuess[4]; intrGuess2[1,2] = intrGuess[5];
        intrGuess2[2,0] = intrGuess[6]; intrGuess2[2,1] = intrGuess[7]; intrGuess2[2,2] = intrGuess[8];

        IEnumerable<IEnumerable<Point3d>> t3 = objectPoints;
        IEnumerable<IEnumerable<Point2d>> t2 = imagePoints;

        OpenCvSharp.CPlusPlus.Cv2.CalibrateCamera(t3, t2, size, intrGuess2, distCoef, out rvecs, out tvecs, OpenCvSharp.CalibrationFlag.UseIntrinsicGuess);

        double x = tvecs[0].Item0;
        double y = tvecs[0].Item1;
        double z = tvecs[0].Item2;

        projectorCamera.transform.position = new Vector3((float)x, (float)y, (float)z);

    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
