using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class Movement : MonoBehaviour
{
    [SerializeField]
    private Transform assets;

    private Transform cam;
    private GameObject currentCube;
    private Transform model;
    void Start()
    {
        model = assets.parent;
        cam = Camera.main.transform;
    }

    public void Move()
    {
        if (currentCube != null && assets.parent != null)
        {
            assets.parent = model;
            Destroy(currentCube);
        }

        assets.SetPositionAndRotation(new Vector3(
                cam.position.x + model.position.x,
                cam.position.y + model.position.y,
                cam.position.z + model.position.z
                ), new quaternion(0, 0, 0, 0));

        currentCube = CreateAnchor();

        Quaternion objectTransformRotation = Quaternion.Euler(0, assets.rotation.y, 0);

        float camYRotation = Camera.main.transform.rotation.eulerAngles.y;
        Quaternion camYRotationOnly = Quaternion.Euler(0, camYRotation, 0);
        currentCube.transform.rotation = objectTransformRotation * camYRotationOnly;
        assets.gameObject.SetActive(true);


    }

    private GameObject CreateAnchor()
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<Renderer>().enabled = false;
        cube.transform.position = cam.position;
        cube.transform.parent = model;
        assets.parent = cube.transform;
        return cube;
    }
}
