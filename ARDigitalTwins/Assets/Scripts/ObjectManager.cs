using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using Dummiesman;

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
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            byte[] modelData = request.downloadHandler.data;
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, "model.obj");
            System.IO.File.WriteAllBytes(filePath, modelData);
            OBJLoader objLoader = new OBJLoader();
            GameObject loadedModel = objLoader.Load(filePath);
            currentObject = loadedModel;
            AlignModel();
        }
        else
        {
            Debug.LogError("Error downloading the model: " + request.error);
        }
    }
}
