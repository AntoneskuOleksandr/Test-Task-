using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ButtonsManager : MonoBehaviour
{
    public GameObject buttonParent;
    public GameObject buttonPrefab;
    private string apiUrl = "https://65e217a2a8583365b317e3cd.mockapi.io/buttons";
    private Dictionary<int, GameObject> buttons = new Dictionary<int, GameObject>();

    public void OnCreateButtonPress()
    {
        StartCoroutine(PostRequest(apiUrl));
    }

    public void OnDeleteButtonPress(int id)
    {
        if (buttons.ContainsKey(id))
        {
            StartCoroutine(DeleteRequest(apiUrl + "/" + id, id));
        }
    }

    private IEnumerator SetButtonImage(string url, Button button)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            button.image.sprite = sprite;
        }
    }

    private IEnumerator PostRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, ""))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string response = webRequest.downloadHandler.text;
                ButtonData buttonData = JsonUtility.FromJson<ButtonData>(response);

                GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
                newButton.GetComponentInChildren<TMP_Text>().text = buttonData.name;

                StartCoroutine(SetButtonImage(buttonData.avatar, newButton.GetComponent<Button>()));

                buttons.Add(int.Parse(buttonData.id), newButton);
            }
            else
            {
                Debug.Log(webRequest.error);
            }
        }
    }


    private IEnumerator DeleteRequest(string url, int id)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Delete(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Button deleted successfully");
                Destroy(buttons[id]);
                buttons.Remove(id);
            }
            else
            {
                Debug.Log(webRequest.error);
            }
        }
    }
}

[System.Serializable]
public class ButtonData
{
    public string createdAt;
    public string name;
    public string avatar;
    public string id;
}
