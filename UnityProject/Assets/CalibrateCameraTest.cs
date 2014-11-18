﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using System.IO;
using OpenCvSharp;

namespace Assets
{
    class CalibrateCameraTest : MonoBehaviour
    {

        public void Update()
        {

            if (Input.GetKeyDown(KeyCode.T))
            {
                const int ImageNum = 3;
                const int PatRow = 7;
                const int PatCol = 10;
                const int PatSize = PatRow * PatCol;
                const int AllPoints = ImageNum * PatSize;
                const float ChessSize = 24.0f;

                IplImage[] srcImg = new IplImage[ImageNum];

                for (int i = 0; i < ImageNum; i++)
                {
                    srcImg[i] = new IplImage(string.Format("C:\\Projects\\OpenCvSharpTest\\UnityProject\\Assets\\CalibrationData\\{0:D2}.jpg", i), LoadMode.Color);
                }

                CvPoint3D32f[, ,] objects = new CvPoint3D32f[ImageNum, PatRow, PatCol];
                for (int i = 0; i < ImageNum; i++)
                {
                    for (int j = 0; j < PatRow; j++)
                    {
                        for (int k = 0; k < PatCol; k++)
                        {
                            objects[i, j, k] = new CvPoint3D32f
                            {
                                X = j * ChessSize,
                                Y = k * ChessSize,
                                Z = 0.0f
                            };
                        }
                    }
                }
                CvMat objectPoints = new CvMat(AllPoints, 3, MatrixType.F32C1, objects);

                CvSize patternSize = new CvSize(PatCol, PatRow);

                int foundNum = 0;
                List<CvPoint2D32f> allCorners = new List<CvPoint2D32f>(AllPoints);
                int[] pointCountsValue = new int[ImageNum];
                using (CvWindow window = new CvWindow("Calibration", WindowMode.AutoSize))
                {
                    for (int i = 0; i < ImageNum; i++)
                    {
                        CvPoint2D32f[] corners;
                        bool found = Cv.FindChessboardCorners(srcImg[i], patternSize, out corners);
                        Debug.Log(i + "...");
                        if (found)
                        {
                            Debug.Log("ok");
                            foundNum++;
                        }
                        else
                        {
                            Debug.Log("fail");
                        }

                        using (IplImage srcGray = new IplImage(srcImg[i].Size, BitDepth.U8, 1))
                        {
                            Cv.CvtColor(srcImg[i], srcGray, ColorConversion.BgrToGray);
                            Cv.FindCornerSubPix(srcGray, corners, corners.Length, new CvSize(3, 3), new CvSize(-1, -1), new CvTermCriteria(20, 0.03));
                            Cv.DrawChessboardCorners(srcImg[i], patternSize, corners, found);
                            pointCountsValue[i] = corners.Length;

                            window.ShowImage(srcImg[i]);
                            Cv.WaitKey(0);
                        }
                        allCorners.AddRange(corners);
                    }
                    if (foundNum != ImageNum)
                    {
                        Debug.Log("FAILED");
                    }
                }

                CvMat imagePoints = new CvMat(AllPoints, 1, MatrixType.F32C2, allCorners.ToArray());
                CvMat pointCounts = new CvMat(ImageNum, 1, MatrixType.S32C1, pointCountsValue);

                CvMat intrinsic = new CvMat(3, 3, MatrixType.F64C1);
                CvMat distortion = new CvMat(1, 4, MatrixType.F64C1);
                CvMat rotation = new CvMat(ImageNum, 3, MatrixType.F64C1);
                CvMat translation = new CvMat(ImageNum, 3, MatrixType.F64C1);

                Cv.CalibrateCamera2(objectPoints, imagePoints, pointCounts, srcImg[0].Size, intrinsic, distortion, rotation, translation, CalibrationFlag.Default);

                CvMat subImagePoints, subObjectPoints;
                Cv.GetRows(imagePoints, out subImagePoints, 0, PatSize);
                Cv.GetRows(objectPoints, out subObjectPoints, 0, PatSize);
                CvMat rotation_ = new CvMat(1, 3, MatrixType.F32C1);
                CvMat translation_ = new CvMat(1, 3, MatrixType.F32C1);

                Cv.FindExtrinsicCameraParams2(subObjectPoints, subImagePoints, intrinsic, distortion, rotation_, translation_, false);

                using (var fs = new CvFileStorage("camera.xml", null, OpenCvSharp.FileStorageMode.Write))
                {
                    fs.Write("intrinsic", intrinsic);
                    fs.Write("rotation", rotation_);
                    fs.Write("translation", translation_);
                    fs.Write("distortion", distortion);
                }

            }
        }

        private string ShowMatrix(CvMat mat){
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

        private string ShowMatrixBig(CvMat mat)
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
    }
}
