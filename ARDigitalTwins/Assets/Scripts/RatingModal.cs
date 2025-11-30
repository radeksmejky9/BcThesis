using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
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
    [SerializeField]
    public Button ratingButton;
    [SerializeField]
    private TMPro.TMP_InputField comment_input;

    private string object_id;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ratingButton.onClick.AddListener(() => StartCoroutine(SubmitRating()));
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

    public void UpdateModal(string ID, string name, string description, string imgUrl)
    {
        if (name == null || description == null || imgUrl == null) return;
        nameText.text = name;
        object_id = ID;
        StartCoroutine(DownloadImage(imgUrl));
    }
    private IEnumerator SubmitRating()
    {
        string url = $"{DBConnector.Instance.apiUrl}/files/{object_id}/ratings";

        string json = $"{{\"stars\":{projectRating},\"comment\":\"{comment_input.text}\"}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Rating submitted: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                Debug.LogError("URL: " + url);
                Debug.LogError("JSON: " + json);
            }
        }
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
