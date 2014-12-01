using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class TestScript : MonoBehaviour {

    public Camera projectorCamera;
    public Camera _mainCamera;
    private CvMat prevIntrinsic;

    public void calibrateFromCorrespondences(List<Vector3> _imagePositions, List<Vector3> _objectPositions, bool usingNormalized)
    {
        StringBuilder sb = new StringBuilder();
        StringBuilder sbPython = new StringBuilder();

        double height = (double)Screen.height;
        double width = (double)Screen.width;
        int pointsCount = _imagePositions.Count;
        int ImageNum = 1;
        int[] pointCountsValue = new int[ImageNum];
        pointCountsValue[0] = pointsCount;

        CvPoint3D32f[,] objects = new CvPoint3D32f[ImageNum, pointsCount];

        sb.Append("objectPoints = [");
        for (int i = 0; i < pointsCount; i++)
        {
            objects[0, i] = new CvPoint3D32f
            {
                X = _objectPositions[i].x,
                Y = _objectPositions[i].y,
                Z = _objectPositions[i].z * -1
            };
            sb.AppendLine(_objectPositions[i].x + ", " + _objectPositions[i].y + ", " + _objectPositions[i].z + ";");
        }
        sb.AppendLine("]';");

        CvMat objectPoints = new CvMat(pointsCount, 3, MatrixType.F32C1, objects);
        List<CvPoint2D32f> allCorners = new List<CvPoint2D32f>(pointsCount);
        sb.Append("imagePoints = [");
        for (int i = 0; i < _imagePositions.Count; i++)
        {
            allCorners.Add(new CvPoint2D32f(_imagePositions[i].x, _imagePositions[i].y));
            sb.AppendLine(_imagePositions[i].x + ", " + _imagePositions[i].y + ";");
        }
        sb.AppendLine("]';");


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

            double[] intrGuess = new double[] { fx, 0.0, cx, 
            0.0, fy, cy, 
            0.0, 0.0, 1.0 };
            intrinsic = new CvMat(3, 3, MatrixType.F64C1, intrGuess);

            sb.AppendLine("fc = [" + focalLength + ", " + focalLength + "];");
            sb.AppendLine(" cc = [" + cx + ", " + cy + "];");
        }
        else
        {
            intrinsic = prevIntrinsic;
        }

        using (StreamWriter outfile = new StreamWriter("pointData.m"))
        {
            outfile.Write(sb.ToString());
        }

        CvMat distortion = new CvMat(1, 4, MatrixType.F64C1);
        CvMat rotation = new CvMat(ImageNum, 3, MatrixType.F64C1);
        CvMat translation = new CvMat(ImageNum, 3, MatrixType.F64C1);

        Cv.CalibrateCamera2(objectPoints, imagePoints, pointCounts, size, intrinsic, distortion, rotation, translation, CalibrationFlag.FixIntrinsic | CalibrationFlag.UseIntrinsicGuess );
        prevIntrinsic = intrinsic;

        CvMat rotation_ = new CvMat(1, 3, MatrixType.F32C1);
        CvMat translation_ = new CvMat(1, 3, MatrixType.F32C1);

        Cv.FindExtrinsicCameraParams2(objectPoints, imagePoints, intrinsic, distortion, rotation_, translation_, false);

        // NB code is mostly from:
        // https://github.com/shimat/opencvsharp/blob/master/sample/CStyleSamplesCS/Samples/CalibrateCamera.cs

        //CvMat rotInv = new CvMat(3, 3, MatrixType.F32C1);
        //rotationFull.Invert(rotInv as CvArr);

        CvMat rotationFull = new CvMat(3, 3, MatrixType.F32C1);
        Cv.Rodrigues2(rotation_, rotationFull);

        float[] LHSflipBackMatrix = new float[] { 1.0f, 0.0f, 0.0f, 
            0.0f, 1.0f, 0.0f, 
            0.0f, 0.0f, -1.0f };
        CvMat LHSflipBackMatrixM = new CvMat(3, 3, MatrixType.F32C1, LHSflipBackMatrix);
        CvMat rotLHS = rotationFull * LHSflipBackMatrixM; // invert Z (as we did before when savings points) to get from RHS -> LHS

        CvMat rotTran = rotLHS.Transpose();
        CvMat transTran = translation_.Transpose();
        CvMat rotFin = (rotTran * -1);

        CvMat transFinal = rotFin * transTran;

        using (StreamWriter outfile = new StreamWriter("../eulerRot.py"))
        {
            outfile.WriteLine("from numpy import array, cross, dot, float64, hypot, sign, transpose, zeros");
            outfile.WriteLine("ROTATION = zeros((3, 3), float64)");
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    outfile.WriteLine("ROTATION[" + i + ", " + j + "] = " + rotTran[i, j]);
                }
            }
        }

        // projectionMatrix: [cameraMatrix] * [R|t].
        double[] rt = new double[] {rotLHS[0,0], rotLHS[0,1], rotLHS[0,2], translation_[0,1],
            rotLHS[1,0], rotLHS[1,1], rotLHS[1,2], translation_[1,1], 
            rotLHS[2,0], rotLHS[2,1], rotLHS[2,2], translation_[2,1], };
        CvMat rtM = new CvMat(3, 4, MatrixType.F64C1, rt);
        CvMat projMat = intrinsic * rtM;

        CvPoint3D64f euler = new CvPoint3D64f();
        Cv.DecomposeProjectionMatrix(projMat, new CvMat(3, 3, MatrixType.F32C1), new CvMat(3, 3, MatrixType.F32C1), new CvMat(4, 1, MatrixType.F32C1), new CvMat(3, 3, MatrixType.F32C1), new CvMat(3, 3, MatrixType.F32C1), new CvMat(3, 3, MatrixType.F32C1), out euler);

        double x = transFinal[0, 0];
        double y = transFinal[0, 1];
        double z = transFinal[0, 2];

        double rx = euler.X;
        double ry = euler.Y;
        double rz = euler.Z;

        sbPython.AppendLine("EULERX = " + euler.X);
        sbPython.AppendLine("EULERY = " + euler.Y);
        sbPython.AppendLine("EULERZ = " + euler.Z);
        using (StreamWriter outfile = new StreamWriter("../eulerAngles.py"))
        {
            outfile.Write(sbPython.ToString());
        }

        projectorCamera.transform.position = new Vector3((float)x, (float)y, (float)z);
        //.Translate(new Vector3((float) x, (float) y, (float) z), Space.World);
        projectorCamera.transform.eulerAngles = new Vector3((float)rx, (float)ry, (float)rz);
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