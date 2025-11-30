using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using MongoDB.Driver;


public class Map : MonoBehaviour
{
    public static Map Instance { get; private set; }

    public GameObject Controls;
    public GameObject MapUI;
    public float Lat { get => lat; set { latLast = lat; lat = value; } }
    public float Lon { get => lon; set { lonLast = lon; lon = value; } }
    private float latLast = 0;
    private float lonLast = 0;
    public int ZoomLevel { get => zoomLevel; set { zoomLevelLast = zoomLevel; zoomLevel = value; } }
    private int zoomLevelLast = 16;

    [SerializeField]
    private int zoomLevel = 16;
    [SerializeField]
    private float lat;
    [SerializeField]
    private float lon;
    [SerializeField]
    private int mapResolution = 2;
    [SerializeField]
    private float coefLon = 0.00001375f;
    [SerializeField]
    private float coefLat = 0.0000086f;
    [SerializeField]
    private GameObject pointPrefab;
    public string mapType = "roadmap";
    private string url = "";
    private int mapWidth = 1000;
    private int mapHeight = 1000;
    private bool mapIsLoading = false;
    private Rect rect;
    private RawImage mapImage;

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
        StartCoroutine(GetGoogleMap());
        rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
        mapWidth = (int)Math.Round(rect.width);
        mapHeight = (int)Math.Round(rect.height);
        mapImage = gameObject.GetComponent<RawImage>();
        RemapPoints();
        zoomLevelLast = zoomLevel;
        lonLast = lon;
        latLast = lat;
    }

    void Update()
    {
        if (zoomLevelLast != zoomLevel && !mapIsLoading)
        {
            RefetchPoints();
            StartCoroutine(GetGoogleMap());
            zoomLevelLast = zoomLevel;
        }
        if (latLast != lat && !mapIsLoading || lonLast != lon && !mapIsLoading)
        {
            RefetchPoints();
            StartCoroutine(GetGoogleMap());
            lonLast = lon;
            latLast = lat;
        }
    }

    public void RefetchPoints()
    {
        var points = gameObject.GetComponentsInChildren<Point>();
        foreach (var point in points)
        {
            Destroy(point.gameObject);
        }
        DBConnector.Instance.RefetchFileMetadata();
        RemapPoints();
    }

    IEnumerator GetGoogleMap()
    {
        url = DBConnector.Instance.apiUrl + "/maps/staticmap?lat=" + Lat + "&lon=" + Lon + "&zoom=" + zoomLevel + "&size=" + mapWidth + "x" + mapHeight;
        Debug.Log("Requesting map: " + url);
        mapIsLoading = true;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            mapIsLoading = false;
            Debug.Log(url);
            mapImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
        RemapPoints();
    }

    public void Zoom(bool isZooming)
    {
        if (!mapIsLoading)
        {
            if (ZoomLevel < 18 && isZooming || ZoomLevel > 7 && !isZooming)
            {
                ZoomLevel = zoomLevel + (isZooming ? 1 : -1);
            }
        }
    }


    public void RemapPoints()
    {
        var points = gameObject.GetComponentsInChildren<Point>();
        if (points.Length > 0)
            foreach (var point in points)
            {
                point.adjustPosition(zoomLevel, mapHeight, mapWidth, Lat, Lon, coefLat, coefLon);
            }
    }

    public void InitPoints(List<DBConnector.ModelMetadata> modelMetadata)
    {
        foreach (DBConnector.ModelMetadata model in modelMetadata)
        {
            if (model != null)
            {
                GameObject point = Instantiate(pointPrefab, gameObject.transform) as GameObject;
                point.GetComponent<Point>().SetModelMetadata(model);
                point.GetComponent<Button>().onClick.AddListener(() => ObjectModal.Instance.modalUIObject.SetActive(true));
                point.GetComponent<Button>().onClick.AddListener(() => Map.Instance.MapUI.SetActive(false));
                point.GetComponent<Button>().onClick.AddListener(() => Map.Instance.Controls.SetActive(false));
                point.GetComponent<Button>().onClick.AddListener(
                    () => ObjectModal.Instance.UpdateModal(
                        model.Name,
                        model.Description,
                        DBConnector.Instance.apiUrl + "/files/" + model.ImgFilename + "/download",
                        DBConnector.Instance.apiUrl + "/files/" + model.GlbFilename + "/download"));
                point.GetComponent<Button>().onClick.AddListener(
                    () => RatingModal.Instance.UpdateModal(
                        model.ID,
                        model.Name,
                        model.Description,
                        DBConnector.Instance.apiUrl + "/files/" + model.ImgFilename + "/download"));
            }
        }
        RemapPoints();
    }

    public void Move(int direction)
    {
        float zoomRatio = 0.0025f * (float)Math.Pow(2, 16 - zoomLevel);
        switch (direction)
        {
            case 0:
                Lat = lat + zoomRatio;
                break;
            case 1:
                Lat = lat - zoomRatio;
                break;
            case 2:
                Lon = lon - zoomRatio;
                break;
            case 3:
                Lon = lon + zoomRatio;
                break;
        }
    }

}