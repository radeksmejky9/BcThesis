using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;


public class Map : MonoBehaviour
{
    public float Lat { get => lat; set { latLast = lat; lat = value; } }
    public float Lon { get => lon; set { lonLast = lon; lon = value; } }

    public int ZoomLevel { get => zoomLevel; set { zoomLevelLast = zoomLevel; zoomLevel = value; } }
    [SerializeField]
    private int zoomLevel = 16;
    [SerializeField]
    private string apiKey;
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
    public string mapType = "roadmap";
    private Point[] points;
    private string url = "";
    private int mapWidth = 1000;
    private int mapHeight = 1000;
    private bool mapIsLoading = false;
    private Rect rect;
    private float latLast = 0;
    private float lonLast = 0;
    private int zoomLevelLast = 16;
    private RawImage mapImage;

    void Start()
    {
        StartCoroutine(GetGoogleMap());
        rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
        mapWidth = (int)Math.Round(rect.width);
        mapHeight = (int)Math.Round(rect.height);
        mapImage = gameObject.GetComponent<RawImage>();
        points = gameObject.GetComponentsInChildren<Point>();
        RemapPoints();
        zoomLevelLast = zoomLevel;
        lonLast = lon;
        latLast = lat;
    }

    void Update()
    {
        if (zoomLevelLast != zoomLevel && !mapIsLoading)
        {
            StartCoroutine(GetGoogleMap());
            zoomLevelLast = zoomLevel;
        }
        if (latLast != lat && !mapIsLoading || lonLast != lon && !mapIsLoading)
        {
            StartCoroutine(GetGoogleMap());
            lonLast = lon;
            latLast = lat;
        }
    }

    IEnumerator GetGoogleMap()
    {
        url = "https://maps.googleapis.com/maps/api/staticmap?center=" + Lat + "," + Lon + "&zoom=" + zoomLevel + "&size=" + mapWidth + "x" + mapHeight + "&scale=" + mapResolution + "&maptype=" + mapType + "&style=feature:poi|element:labels|visibility:off&key=" + apiKey;
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
        if (points.Length > 0)
            foreach (var point in points)
            {
                point.adjustPosition(zoomLevel, mapHeight, mapWidth, Lat, Lon, coefLat, coefLon);
            }
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