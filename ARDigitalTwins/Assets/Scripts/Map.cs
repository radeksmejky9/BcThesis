using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using Unity.VisualScripting;


public class Map : MonoBehaviour
{
    public string apiKey;
    public float lat = 50.660200f;
    public float lon = 14.041132f;
    public int zoom = 12;
    public enum resolution { low = 1, high = 2 };
    public resolution mapResolution = resolution.high;
    public enum type { roadmap, satellite, gybrid, terrain };
    public type mapType = type.roadmap;
    private string url = "";
    private int mapWidth = 1000;
    private int mapHeight = 1000;
    private bool mapIsLoading = false;
    private Rect rect;
    private float latLast = 0;
    private float lonLast = 0;
    private int zoomLast = 12;
    private resolution mapResolutionLast = resolution.high;
    private type mapTypeLast = type.roadmap;
    private bool updateMap = true;

    [SerializeField]
    private Transform[] points;

    void Start()
    {
        StartCoroutine(GetGoogleMap());
        rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
        mapWidth = (int)Math.Round(rect.width);
        mapHeight = (int)Math.Round(rect.height);
        points = this.gameObject.GetComponentsInChildren<Transform>();
    }

    void Update()
    {
        if (zoomLast != zoom && !mapIsLoading)
        {
            StartCoroutine(GetGoogleMap());
            rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
            mapWidth = (int)Math.Round(rect.width);
            mapHeight = (int)Math.Round(rect.height);
        }
    }

    IEnumerator GetGoogleMap()
    {
        url = "https://maps.googleapis.com/maps/api/staticmap?center=" + lat + "," + lon + "&zoom=" + zoom + "&size=" + mapWidth + "x" + mapHeight + "&scale=" + mapResolution + "&maptype=" + mapType + "&style=feature:poi|element:labels|visibility:off&key=" + apiKey;
        mapIsLoading = true;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            mapIsLoading = false;
            Debug.Log(url);
            gameObject.GetComponent<RawImage>().texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            latLast = lat;
            lonLast = lon;
            zoomLast = zoom;
            mapResolutionLast = mapResolution;
            mapTypeLast = mapType;
            updateMap = true;
        }
    }

    public void Zoom(bool zoomingIn)
    {
        if (!mapIsLoading)
        {
            if (this.zoom < 18 && zoomingIn || this.zoom > 7 && !zoomingIn)
            {
                zoom += zoomingIn ? 1 : -1;
                RemapPoints(zoomingIn);
            }
        }
    }

    public void RemapPoints(bool zoomingIn)
    {
        if (points.Length > 0)
            foreach (var point in points)
            {
                var posX = point.localPosition.x;
                var posY = point.localPosition.y;

                posX = zoomingIn ? posX * 2 : posX / 2;
                posY = zoomingIn ? posY * 2 : posY / 2;

                point.transform.localPosition = new Vector3(
                    posX,
                    posY,
                    point.localPosition.z
                );

                point.gameObject.SetActive(
                    posX > mapWidth / 2 ||
                    posX < -mapWidth / 2 ||
                    posY > mapHeight / 2 ||
                    posY < -mapHeight / 2 ? false : true);
            }
    }


}