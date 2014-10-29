using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;

public class TestScript : MonoBehaviour {

    public Camera projectorCamera;

    // used for intrinsic guess


    public void calibrateFromCorrespondences(List<Vector3> _imagePositions, List<Vector3> _objectPositions)
    {
        int pointsCount = _imagePositions.Count;
        int ImageNum = 1;
        int[] pointCountsValue = new int[ImageNum];
        pointCountsValue[0] = pointsCount;

        CvPoint3D32f[,] objects = new CvPoint3D32f[ImageNum, pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            objects[0, i] = new CvPoint3D32f
            {
                X = _objectPositions[i].x,
                Y = _objectPositions[i].y,
                Z = _objectPositions[i].z
            };
        }

        CvMat objectPoints = new CvMat(pointsCount, 3, MatrixType.F32C1, objects);
        List<CvPoint2D32f> allCorners = new List<CvPoint2D32f>(pointsCount);
        for (int i = 0; i < _imagePositions.Count; i++)
        {
            allCorners.Add(new CvPoint2D32f(_imagePositions[i].x, _imagePositions[i].y));
        }

        CvMat imagePoints = new CvMat(pointsCount, 1, MatrixType.F32C2, allCorners.ToArray());
        CvMat pointCounts = new CvMat(ImageNum, 1, MatrixType.S32C1, pointCountsValue);

        double fx = 7.42; // not sure http://www.optoma.co.uk/projectordetails.aspx?PTypeDB=Business&PC=EH200ST
        double fy = 7.42;
        double height = (double)Screen.height;
        double width = (double)Screen.width;
        double cx = height / 2.0;
        double cy = width / 2.0;
        double[] intrGuess = new double[] { fx, 0.0, cx, 
            0.0, fy, cy, 
            0.0, 0.0, 1.0 };

        CvMat intrinsic = new CvMat(3, 3, MatrixType.F64C1, intrGuess);
        CvMat distortion = new CvMat(1, 4, MatrixType.F64C1);
        CvMat rotation = new CvMat(ImageNum, 3, MatrixType.F64C1);
        CvMat translation = new CvMat(ImageNum, 3, MatrixType.F64C1);

        Cv.CalibrateCamera2(objectPoints, imagePoints, pointCounts, new Size(width,height), intrinsic, distortion, rotation, translation, CalibrationFlag.UseIntrinsicGuess);

        using (var fs = new CvFileStorage("camera.xml", null, OpenCvSharp.FileStorageMode.Write))
        {
            fs.Write("intrinsic", intrinsic);
            fs.Write("rotation", rotation);
            fs.Write("translation", translation);
            fs.Write("distortion", distortion);
        }

        // NB code is mostly from:
        // https://github.com/shimat/opencvsharp/blob/master/sample/CStyleSamplesCS/Samples/CalibrateCamera.cs

        double x = translation[0, 0];
        double y = translation[0, 1];
        double z = translation[0, 2];

        double rx = rotation[0, 0];
        double ry = rotation[0, 1];
        double rz = rotation[0, 2];

        projectorCamera.transform.Translate(new Vector3((float) x, (float) y, (float) z), Space.World);
        projectorCamera.transform.Rotate(new Vector3((float)rx, (float)ry, (float)rz), Space.World);

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}