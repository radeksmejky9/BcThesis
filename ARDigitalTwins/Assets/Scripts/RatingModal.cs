using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Playables;
using UnityEngine.UI;

public class RatingModal : MonoBehaviour
{
    public static RatingModal Instance { get; private set; }
    private int projectRating;

    public int ProjectRating
    {
        get { return projectRating; }
        set { projectRating = value; }
    }

    [SerializeField]
    private TMPro.TMP_Text nameText;
    [SerializeField]
    public UnityEngine.UI.Image img;
    [SerializeField]
    public GameObject modalUIObject;
    [SerializeField]
    public GameObject ratingNotification;

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

    public void ActivateModal()
    {
        if (nameText.text == "NameNull")
        {
            modalUIObject.SetActive(false);
            ratingNotification.SetActive(true);
        }
        else
        {
            modalUIObject.SetActive(true);
            ratingNotification.SetActive(false);
        }
    }

    public void UpdateModal(string name, string description, string imgUrl)
    {
        if (name == null || description == null || imgUrl == null) return;
        nameText.text = name;
        StartCoroutine(DownloadImage(imgUrl));
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
