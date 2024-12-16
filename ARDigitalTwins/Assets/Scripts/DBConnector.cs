using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

public class DBConnector : MonoBehaviour
{
    public static DBConnector Instance { get; private set; }
    public string apiUrl;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(GetFileMetadataCoroutine());
    }

    public void RefetchFileMetadata()
    {
        StartCoroutine(GetFileMetadataCoroutine());
    }

    private IEnumerator GetFileMetadataCoroutine()
    {
        Debug.Log(apiUrl + "/files");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl + "/files"))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {webRequest.error}");
            }
            else
            {
                Debug.Log("Received response: " + webRequest.downloadHandler.text);
                try
                {
                    List<ModelMetadata> filesMetadata = JsonConvert.DeserializeObject<List<ModelMetadata>>(webRequest.downloadHandler.text);
                    foreach (ModelMetadata metadata in filesMetadata)
                    {
                        Debug.Log($"{metadata.ID}, {metadata.GlbFilename}, {metadata.Latitude}, {metadata.Longitude}, {metadata.Name}");
                    }
                    Map.Instance.InitPoints(filesMetadata);
                }
                catch (JsonSerializationException jsonEx)
                {
                    Debug.LogError($"JSON Serialization Error: {jsonEx.Message}\n{jsonEx.StackTrace}");
                }
            }
        }
    }

    [System.Serializable]
    public class ModelMetadata
    {
        [JsonProperty("_id")]
        public string ID { get; set; }

        [JsonProperty("glb_filename")]
        public string GlbFilename { get; set; }
        [JsonProperty("img_filename")]
        public string ImgFilename { get; set; }

        [JsonProperty("lat")]
        public float Latitude { get; set; }

        [JsonProperty("lon")]
        public float Longitude { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
