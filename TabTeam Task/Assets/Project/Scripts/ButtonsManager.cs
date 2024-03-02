using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ButtonsManager : MonoBehaviour
{
    [SerializeField] private GameObject buttonParent;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private PopupManager popupManager;

    private string apiUrl = "https://65e217a2a8583365b317e3cd.mockapi.io/buttons";
    private Dictionary<int, GameObject> buttons = new Dictionary<int, GameObject>();

    public void OnCreateButtonPress()
    {
        StartCoroutine(PostRequest(apiUrl));
    }

    public void OnDeleteButtonPress()
    {
        popupManager.ShowPopup(DeleteButton);
    }

    public void DeleteButton(int id)
    {
        if (buttons.ContainsKey(id))
        {
            StartCoroutine(DeleteRequest(apiUrl + "/" + id, id));
        }
    }

    public void OnUpdateButtonPress()
    {
        popupManager.ShowPopup(UpdateButton);
    }

    public void UpdateButton(int id)
    {
        if (buttons.ContainsKey(id))
        {
            StartCoroutine(GetRequest(apiUrl + "/" + id, buttonData =>
            {
                buttonData.text = "Updated Button";
                StartCoroutine(PutRequest(apiUrl + "/" + id, buttonData, id));
            }));
        }
    }

    private IEnumerator GetRequest(string url, Action<ButtonData> onSuccess)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string response = webRequest.downloadHandler.text;
                ButtonData buttonData = JsonUtility.FromJson<ButtonData>(response);
                onSuccess?.Invoke(buttonData);
            }
            else
            {
                Debug.Log(webRequest.error);
            }
        }
    }

    private IEnumerator PutRequest(string url, ButtonData buttonData, int id)
    {
        string json = JsonUtility.ToJson(buttonData);

        using (UnityWebRequest webRequest = UnityWebRequest.Put(url, json))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Button updated successfully");
                buttons[id].GetComponentInChildren<TMP_Text>().text = buttonData.text;
            }
            else
            {
                Debug.Log(webRequest.error);
            }
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
                newButton.GetComponentInChildren<TMP_Text>().text = buttonData.text;

                newButton.GetComponent<Image>().color = HSLToColor(buttonData.color);

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

    private Color HSLToColor(float[] hsl)
    {
        float h = hsl[0] / 100;
        float s = hsl[1];
        float l = hsl[2];

        float r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            float q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            float p = 2 * l - q;
            r = HueToRGB(p, q, h + 1 / 3f);
            g = HueToRGB(p, q, h);
            b = HueToRGB(p, q, h - 1 / 3f);
        }

        return new Color(r, g, b);
    }

    private float HueToRGB(float p, float q, float t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1 / 6f) return p + (q - p) * 6 * t;
        if (t < 1 / 2f) return q;
        if (t < 2 / 3f) return p + (q - p) * (2 / 3f - t) * 6;
        return p;
    }
}

[Serializable]
public class ButtonData
{
    public float[] color;
    public string text;
    public string animationType;
    public string id;
}