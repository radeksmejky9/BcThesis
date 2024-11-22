using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

public class DBConnector : MonoBehaviour
{
    public static DBConnector Instance { get; private set; }
    private string apiUrl = "http://192.168.37.142:5000/files";

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

    private IEnumerator GetFileMetadataCoroutine()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
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
                        Debug.Log($"{metadata.ID}, {metadata.FilePath}, {metadata.FileUrl}, {metadata.Filename}, {metadata.Latitude}, {metadata.Longitude}, {metadata.Name}");
                    }
                    Map.Instance.modelMetadata = filesMetadata;
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

        [JsonProperty("file_path")]
        public string FilePath { get; set; }

        [JsonProperty("file_url")]
        public string FileUrl { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("lat")]
        public float Latitude { get; set; }

        [JsonProperty("lon")]
        public float Longitude { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
