using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ObjectModal : MonoBehaviour
{
    public static ObjectModal Instance { get; private set; }
    [SerializeField]
    private TMPro.TMP_Text nameText;
    [SerializeField]
    private TMPro.TMP_Text descriptionText;
    [SerializeField]
    public UnityEngine.UI.Image img;
    [SerializeField]
    private Button spawnButton;
    [SerializeField]
    public GameObject modalUIObject;

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

    public void UpdateModal(string name, string description, string imgUrl, string glbUrl)
    {
        if (name == null || description == null || imgUrl == null || glbUrl == null) return;
        nameText.text = name;
        descriptionText.text = description;
        StartCoroutine(DownloadImage(imgUrl));
        spawnButton.onClick.AddListener(() => ObjectManager.Instance.CreateObject(glbUrl));

    }
    private IEnumerator DownloadImage(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error downloading image: " + request.error);
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                img.sprite = sprite;
                img.enabled = true;
            }
        }
    }


}
