using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;

public class ObjectManager : MonoBehaviour
{
    public static ObjectManager Instance { get; private set; }
    [SerializeField]
    private Transform assets;
    private Transform cam;
    private Transform model;
    private GameObject anchor;
    private GameObject currentObject;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        model = assets.parent;
        cam = Camera.main.transform;
    }

    public void AlignModel()
    {
        if (anchor != null && assets.parent != null)
        {
            assets.parent = model;
            Destroy(anchor);
        }

        anchor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        anchor.GetComponent<Renderer>().enabled = false;
        anchor.transform.position = cam.position;
        anchor.transform.parent = model;

        assets.parent = anchor.transform;
        Quaternion objectTransformRotation = Quaternion.Euler(0, assets.rotation.y, 0);
        Quaternion anchorY = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);

        assets.SetPositionAndRotation(new Vector3(
                cam.position.x + model.position.x,
                cam.position.y + model.position.y,
                cam.position.z + model.position.z
                ), new Quaternion(0, 0, 0, 0));
        anchor.transform.rotation = objectTransformRotation * anchorY;
    }

    public void CreateObject(string url)
    {
        model.position = Vector3.zero;
        assets.position = Vector3.zero;
        if (currentObject != null)
            Destroy(currentObject);
        StartCoroutine(DownloadModel(url));
    }

    IEnumerator DownloadModel(string url)
    {
        var gltf = gameObject.AddComponent<GLTFast.GltfAsset>();
        gltf.Url = url;
        yield break;
    }

}
