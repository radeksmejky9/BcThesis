using System;
using UnityEngine;

public class Point : MonoBehaviour
{
    [SerializeField]
    private float lat;
    [SerializeField]
    private float lon;
    [SerializeField]
    private string name;
    [SerializeField]
    private string description;
    [SerializeField]
    private object imgFilename;

    public UnityEngine.UI.Image img;

    public void Start()
    {
        img = gameObject.GetComponent<UnityEngine.UI.Image>();

    }

    public void adjustPosition(int zoomLevel, int mapHeight, int mapWidth, float mapLat, float mapLon, float coefLat, float coefLon)
    {
        var posX = (-mapLon + this.lon) / coefLon * (float)Math.Pow(2, zoomLevel - 16);
        var posY = (-mapLat + this.lat) / coefLat * (float)Math.Pow(2, zoomLevel - 16);
        transform.localPosition = new Vector3(
                posX,
                posY + 25,
                transform.localPosition.z
            );

        if (posX > mapWidth / 2 ||
            posX < -mapWidth / 2 ||
            posY > mapHeight / 2 ||
            posY < -mapHeight / 2)
        {
            if (img != null)
            {
                img.enabled = false;
            }
        }
        else
        {
            if (img != null)
            {
                img.enabled = true;
            }
        }

    }

    public void SetModelMetadata(DBConnector.ModelMetadata modelMetadata)
    {
        this.lat = modelMetadata.Latitude;
        this.lon = modelMetadata.Longitude;
        this.name = modelMetadata.Name;
        this.description = modelMetadata.Description;
        this.imgFilename = modelMetadata.ImgFilename;
    }
}
