using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FakeClick : MonoBehaviour {

    public TestScript plugin;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.F))
        {
            List<Vector3> _objectPositions = new List<Vector3>();
            _objectPositions.Add(new Vector3(-7.10872650146484f, 8.20051956176758f, 6.13520765304565f));
            _objectPositions.Add(new Vector3(-7.65235328674316f, -5.568284034729f, 6.39236736297607f));
            _objectPositions.Add(new Vector3(8.67125129699707f, -4.53154373168945f, -2.66542959213257f));
            _objectPositions.Add(new Vector3(14.5806922912598f, 0.320883810520172f, 4.76360177993774f));
            _objectPositions.Add(new Vector3(11.7524862289429f, -0.457778573036194f, 8.64883232116699f));
            _objectPositions.Add(new Vector3(1.29042196273804f, -0.00464601069688797f, -1.17936754226685f));
            _objectPositions.Add(new Vector3(-2.48350143432617f, 0.534460484981537f, -0.982286274433136f));
            _objectPositions.Add(new Vector3(3.66202616691589f, 4.63078784942627f, 4.91419506072998f));
            _objectPositions.Add(new Vector3(1.15246772766113f, 8.87045574188232f, 4.88052654266357f));
            _objectPositions.Add(new Vector3(-22.1528415679932f, 7.09238386154175f, 54.8600120544434f));

            List<Vector3> _imagePositions = new List<Vector3>();
            _imagePositions.Add(new Vector3(256f, 12f));
            _imagePositions.Add(new Vector3(242f, 341f));
            _imagePositions.Add(new Vector3(749f, 362f));
            _imagePositions.Add(new Vector3(678f, 196f));
            _imagePositions.Add(new Vector3(595f, 206f));
            _imagePositions.Add(new Vector3(512f, 220f));
            _imagePositions.Add(new Vector3(371f, 203f));
            _imagePositions.Add(new Vector3(506f, 110f));
            _imagePositions.Add(new Vector3(456f, 14f));
            _imagePositions.Add(new Vector3(254f, 149f));

            plugin.calibrateFromCorrespondences(_imagePositions, _objectPositions, false);
        }

	}
}
