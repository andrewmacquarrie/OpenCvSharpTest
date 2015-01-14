using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Assets;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Calibration : MonoBehaviour {
    // NB some code adapted from OpenCVSharp camera calibration example:
    // https://github.com/shimat/opencvsharp/blob/master/sample/CStyleSamplesCS/Samples/CalibrateCamera.cs

    public Camera projectorCamera;
    public Camera _mainCamera;
    private CvMat prevIntrinsic;

    public void calibrateFromCorrespondences(List<Vector3> _imagePositions, List<Vector3> _objectPositions, bool usingNormalized)
    {
#if UNITY_EDITOR
        Vector2 hw = Handles.GetMainGameViewSize();
        double height = (double)hw.y;
        double width = (double)hw.x;
#endif
#if UNITY_EDITOR == false
        double height = (double)Screen.height;
        double width = (double)Screen.width;
#endif


        int pointsCount = _imagePositions.Count;
        int numImages = 1;
        int[] pointCountsValue = new int[numImages];
        pointCountsValue[0] = pointsCount;

        CvMat pointCounts = new CvMat(numImages, 1, MatrixType.S32C1, pointCountsValue);

        CvMat imagePoints = PutImagePointsIntoCVMat(_imagePositions, pointsCount);
        CvMat objectPoints = PutObjectPointsIntoCVMat(_objectPositions, pointsCount, numImages);

        Size size = new Size(width, height);
        if (usingNormalized)
            size = new Size(1, 1);
        
        CvMat intrinsic;
        if (prevIntrinsic == null)
            intrinsic = createIntrinsicMatrix(height, width, usingNormalized);
        else
            intrinsic = prevIntrinsic;

        CvMat distortion = new CvMat(1, 4, MatrixType.F64C1);
        
        CvMat rotation_ = new CvMat(1, 3, MatrixType.F32C1);
        CvMat translation_ = new CvMat(1, 3, MatrixType.F32C1);

        Cv.FindExtrinsicCameraParams2(objectPoints, imagePoints, intrinsic, distortion, rotation_, translation_, false);

        //CvMat rotation = new CvMat(numImages, 3, MatrixType.F64C1);
        //CvMat translation = new CvMat(numImages, 3, MatrixType.F64C1);

        Cv.CalibrateCamera2(objectPoints, imagePoints, pointCounts, size, intrinsic, distortion, rotation_, translation_,  CalibrationFlag.ZeroTangentDist | CalibrationFlag.UseIntrinsicGuess | CalibrationFlag.FixFocalLength | CalibrationFlag.FixK1 | CalibrationFlag.FixK2 | CalibrationFlag.FixK3 | CalibrationFlag.FixK4 | CalibrationFlag.FixK5 | CalibrationFlag.FixK6);
        prevIntrinsic = intrinsic;



        CvMat rotationFull = new CvMat(3, 3, MatrixType.F32C1);
        Cv.Rodrigues2(rotation_, rotationFull); // get full rotation matrix from rotation vector

        float[] LHSflipBackMatrix = new float[] { 1.0f, 0.0f, 0.0f, 
            0.0f, 1.0f, 0.0f, 
            0.0f, 0.0f, -1.0f };
        CvMat LHSflipBackMatrixM = new CvMat(3, 3, MatrixType.F32C1, LHSflipBackMatrix);
        CvMat rotLHS = rotationFull * LHSflipBackMatrixM; // invert Z (as we did before when savings points) to get from RHS -> LHS
        CvMat rotTran = rotLHS.Transpose(); // transpose is same as inverse for rotation matrix
        CvMat transTran = translation_.Transpose(); // to get in right format
        CvMat rotFinal = (rotTran * -1);

        CvMat transFinal = rotFinal * transTran;

        Rotation r = RotationConversion.RToEulerZXY(rotTran);

        ApplyTranslationAndRotationToCamera(transFinal, r);
        ApplyIntrinsicToCamera(intrinsic, height, width, usingNormalized);
    }

    private void ApplyIntrinsicToCamera(CvMat intrinsic, double height, double width, bool usingNormalized)
    {
        double fx = intrinsic[0, 0];
        double fy = intrinsic[1, 1];
        double cx = intrinsic[0, 2];
        double cy = intrinsic[1, 2];

        if (usingNormalized)
        {
            fx *= width;
            fy *= height;
            cx *= width;
            cy *= height;
        }

        // NB This is the vertical field of view; horizontal FOV varies depending on the viewport's aspect ratio
        // from http://docs.unity3d.com/ScriptReference/Camera-fieldOfView.html

        float fov = Mathf.Rad2Deg * 2.0f * Mathf.Atan((float) (height / (2.0 * fy)));
        projectorCamera.fieldOfView = fov;
        _mainCamera.fieldOfView = fov;

        // TODO: apply princial point if different from centre of image? Could put camera inside empty game object and translate x,y by PP difference?
    }

    private void ApplyTranslationAndRotationToCamera(CvMat translation, Rotation r)
    {
        double tx = translation[0, 0];
        double ty = translation[0, 1];
        double tz = translation[0, 2];

        projectorCamera.transform.position = new Vector3((float)tx, (float)ty, (float)tz);
        projectorCamera.transform.eulerAngles = new Vector3((float)r.X, (float)r.Y, (float)r.Z);

        _mainCamera.transform.position = new Vector3((float)tx, (float)ty, (float)tz);
        _mainCamera.transform.eulerAngles = new Vector3((float)r.X, (float)r.Y, (float)r.Z);
    }

    private CvMat createIntrinsicMatrix(double height, double width, bool usingNormalized)
    {
        // from https://docs.google.com/spreadsheet/ccc?key=0AuC4NW61c3-cdDFhb1JxWUFIVWpEdXhabFNjdDJLZXc#gid=0
        // taken from http://www.neilmendoza.com/projector-field-view-calculator/
        float hfov = 91.2705674249382f;
        float vfov = 59.8076333281726f;

        // err taken temp from calibration-basic in mapamok
        //float hfov = 57.35f;
        //float vfov = 34.20f;


        double fx = (double)((float)width / (2.0f * Mathf.Tan(0.5f * hfov * Mathf.Deg2Rad)));
        double fy = (double)((float)height / (2.0f * Mathf.Tan(0.5f * vfov * Mathf.Deg2Rad)));

        double cy = height / 2.0;
        double cx = width / 2.0;

        if (usingNormalized)
        {
            fx = fx / width;
            fy = fy / height;
            cy = cy / height;
            cx = cx / width;
        }

        double[] intrGuess = new double[] { fx, 0.0, cx, 
            0.0, fy, cy, 
            0.0, 0.0, 1.0 };

        return new CvMat(3, 3, MatrixType.F64C1, intrGuess);
    }

    private CvMat PutObjectPointsIntoCVMat(List<Vector3> _objectPositions, int pointsCount, int ImageNum)
    {
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

        return new CvMat(pointsCount, 3, MatrixType.F32C1, objects);
    }

    private CvMat PutImagePointsIntoCVMat(List<Vector3> _imagePositions, int pointsCount)
    {
        List<CvPoint2D32f> allCorners = new List<CvPoint2D32f>(pointsCount);

        for (int i = 0; i < _imagePositions.Count; i++)
        {
            allCorners.Add(new CvPoint2D32f(_imagePositions[i].x, _imagePositions[i].y));
        }

        return new CvMat(pointsCount, 1, MatrixType.F32C2, allCorners.ToArray());
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}