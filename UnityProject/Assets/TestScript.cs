using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Text;

public class TestScript : MonoBehaviour {

    public Camera projectorCamera;
    public Camera _mainCamera;
    private CvMat prevIntrinsic;

    public void calibrateFromCorrespondences(List<Vector3> _imagePositions, List<Vector3> _objectPositions, bool usingNormalized)
    {
        double height = (double) Screen.height;
        double width = (double) Screen.width;
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
                Z = _objectPositions[i].z * -1
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

        //float fl = viewHeight / (2.0 * Mathf.Tan(0.5f * projectorCamera.fieldOfView * Mathf.Deg2Rad));

        Size size = new Size(width, height);

        CvMat intrinsic;
        if (prevIntrinsic == null)
        {
            //double fx = 37.469987986050846; // OLD COMMENT (now using ~fov of camera in unity): not sure http://www.optoma.co.uk/projectordetails.aspx?PTypeDB=Business&PC=EH200ST
            //double fy = 37.469987986050846;

            float fov = 60.0f;
            float focalLength = (float) height / (2.0f * Mathf.Tan(0.5f * fov * Mathf.Deg2Rad));

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

            double[] intrGuess = new double[] { fx, 0.0, cx, 
            0.0, fy, cy, 
            0.0, 0.0, 1.0 };
            intrinsic = new CvMat(3, 3, MatrixType.F64C1, intrGuess);
        }
        else
        {
            intrinsic = prevIntrinsic;
        }

        CvMat distortion = new CvMat(1, 4, MatrixType.F64C1);
        CvMat rotation = new CvMat(ImageNum, 3, MatrixType.F64C1);
        CvMat translation = new CvMat(ImageNum, 3, MatrixType.F64C1);

        Cv.CalibrateCamera2(objectPoints, imagePoints, pointCounts, size, intrinsic, distortion, rotation, translation, CalibrationFlag.UseIntrinsicGuess | CalibrationFlag.ZeroTangentDist);
        prevIntrinsic = intrinsic;

       CvMat rotation_ = new CvMat(1, 3, MatrixType.F32C1);
       CvMat translation_ = new CvMat(1, 3, MatrixType.F32C1);

       translation_[0, 0] = _mainCamera.transform.position.x;
       translation_[0, 1] = _mainCamera.transform.position.y;
       translation_[0, 2] = _mainCamera.transform.position.z;

       Cv.FindExtrinsicCameraParams2(objectPoints, imagePoints, intrinsic, distortion, rotation_, translation_, true);

        // NB code is mostly from:
        // https://github.com/shimat/opencvsharp/blob/master/sample/CStyleSamplesCS/Samples/CalibrateCamera.cs

        CvMat rotInv = new CvMat(3, 1, MatrixType.F32C1);

        rotation_.Invert(rotInv as CvArr);

        double x = translation[0, 0];
        double y = translation[0, 1];
        double z = translation[0, 2];

        double rx = rotation[0, 0];
        double ry = rotation[0, 1];
        double rz = rotation[0, 2];

        projectorCamera.transform.position = new Vector3((float)x, (float)y, (float)z);
        //.Translate(new Vector3((float) x, (float) y, (float) z), Space.World);
        //projectorCamera.transform.eulerAngles = new Vector3((float)rx, (float)ry, (float)rz);
        //.Rotate(new Vector3((float)rx, (float)ry, (float)rz), Space.World);

	}
	
	// Update is called once per frame
	void Update () {
	
	}


    private string ShowMatrix(CvMat mat)
    {
        StringBuilder sb = new StringBuilder();
        for (int x = 0; x < mat.Rows; x++)
        {
            for (int y = 0; y < mat.Cols; y++)
            {
                sb.Append(mat[x, y] + " ");
            }
            sb.AppendLine("");
        }
        return sb.ToString();
    }

    private string Form(CvPoint3D32f[,] ar)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < ar.Length; i++)
        {
            sb.Append(Form(ar[0,i]));
        }
        return sb.ToString();
    }

    private string Form(CvPoint3D32f s)
    {
        return "" + s.X + ", " + s.Y + ", " + s.Z + ", ";
    }
}