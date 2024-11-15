using System;
using UnityEngine;

public class Point : MonoBehaviour
{
    [SerializeField]
    private float lat;
    [SerializeField]
    private float lon;

    public void adjustPosition(int zoomLevel, int mapHeight, int mapWidth, float mapLat, float mapLon, float coefLat, float coefLon)
    {
        var posX = (-mapLon + this.lon) / coefLon;
        var posY = (-mapLat + this.lat) / coefLat;
        posX = posX * (float)Math.Pow(2, zoomLevel - 16);
        posY = posY * (float)Math.Pow(2, zoomLevel - 16);
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
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

}
