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
        if (currentObject != null)
            while (assets.transform.childCount > 0) Destroy(assets.transform.GetChild(0).gameObject);
        StartCoroutine(DownloadModel(url, assets.gameObject));
    }

    IEnumerator DownloadModel(string url, GameObject gameObject)
    {
        Debug.Log("Downloading model");
        var oldAssets = gameObject.GetComponents<GLTFast.GltfAsset>();
        foreach (var old in oldAssets)
        {
            Destroy(old);
        }
        var gltf = gameObject.AddComponent<GLTFast.GltfAsset>();
        gltf.Url = url;

        yield break;
    }


}
