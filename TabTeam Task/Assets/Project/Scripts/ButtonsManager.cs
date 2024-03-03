using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;

public class ButtonsManager : MonoBehaviour
{
    [SerializeField] private GameObject buttonParent;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private PopupManager popupManager;

    [SerializeField] private string apiUrl = "https://65e217a2a8583365b317e3cd.mockapi.io/buttons";
    private Dictionary<int, GameObject> buttons = new Dictionary<int, GameObject>();

    public void OnCreateButtonPress()
    {
        StartCoroutine(PostRequest(apiUrl));
    }

    public void OnDeleteButtonPress()
    {
        popupManager.ShowPopup(DeleteButton);
    }

    public void OnUpdateButtonPress()
    {
        popupManager.ShowPopup(UpdateButton);
    }

    public void OnRefreshButtonPress()
    {
        popupManager.ShowPopup(RefreshButtons);
    }

    private void DeleteButton(int id)
    {
        if (buttons.ContainsKey(id))
        {
            StartCoroutine(DeleteRequest(apiUrl + "/" + id, id));
        }
    }

    private void UpdateButton(int id)
    {
        if (buttons.ContainsKey(id))
        {
            StartCoroutine(GetRequest(apiUrl + "/" + id, buttonData =>
            {
                buttonData.text = "Updated Button";
                buttonData.color = new float[] { 255f, 1f, 1f };
                buttonData.animationType = true;

                StartCoroutine(PutRequest(apiUrl + "/" + id, buttonData, id));
            },
            () =>
            {
                // If the button is not on the server, remove it from the application
                PlayAnimation(buttons[id], "ButtonDisappear");
                buttons.Remove(id);
            }));
        }
    }

    private void RefreshButtons(int id)
    {
        if (id != 0)
        {
            StartCoroutine(GetRequest(apiUrl + "/" + id, buttonData =>
            {
                if (buttons.ContainsKey(id))
                {
                    buttons[id].GetComponentInChildren<TMP_Text>().text = buttonData.text;
                    buttons[id].GetComponent<Image>().color = HSLToColor(buttonData.color);
                }
            },
            () =>
            {
                // If the button is not on the server, remove it from the application
                PlayAnimation(buttons[id], "ButtonDisappear");
                buttons.Remove(id);
            }));
        }
        else
        {
            StartCoroutine(GetRequest(apiUrl));
        }
    }

    private IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string response = webRequest.downloadHandler.text;

                // Wrap the JSON response in an object containing the list
                ButtonDataList buttonDataList = JsonUtility.FromJson<ButtonDataList>("{\"buttonData\":" + response + "}");

                // Create a list of button IDs on the server
                List<int> serverButtonIds = buttonDataList.buttonData.Select(buttonData => int.Parse(buttonData.id)).ToList();

                // Check every button in the application
                foreach (var button in new Dictionary<int, GameObject>(buttons))
                {
                    // If the button is not on the server, remove it from the application
                    if (!serverButtonIds.Contains(button.Key))
                    {
                        PlayAnimation(button.Value, "ButtonDisappear");
                        buttons.Remove(button.Key);
                    }
                }

                // Update or add buttons to the application
                foreach (var buttonData in buttonDataList.buttonData)
                {
                    if (buttons.ContainsKey(int.Parse(buttonData.id)))
                    {
                        // Update the existing button
                        buttons[int.Parse(buttonData.id)].GetComponentInChildren<TMP_Text>().text = buttonData.text;
                        buttons[int.Parse(buttonData.id)].GetComponent<Image>().color = HSLToColor(buttonData.color);
                    }
                    else
                    {
                        // Create a new button
                        GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
                        newButton.GetComponentInChildren<TMP_Text>().text = buttonData.text;
                        newButton.GetComponent<Image>().color = HSLToColor(buttonData.color);

                        SetButtonValues(buttonData, newButton, "ButtonAppear");
                        buttons.Add(int.Parse(buttonData.id), newButton);
                    }
                }
            }
            else
            {
                Debug.Log(webRequest.error);
            }
        }
    }

    private IEnumerator GetRequest(string url, Action<ButtonData> onSuccess, Action onNotFound = null)
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
                if (webRequest.responseCode == 404)
                {
                    // If the button is not found on the server, call the onNotFound handler
                    onNotFound?.Invoke();
                }
                else
                {
                    Debug.Log(webRequest.error);
                }
            }
        }
    }

    private IEnumerator PostRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(url, ""))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string response = webRequest.downloadHandler.text;
                ButtonData buttonData = JsonUtility.FromJson<ButtonData>(response);

                GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);

                SetButtonValues(buttonData, newButton, "ButtonAppear");

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
                PlayAnimation(buttons[id], "ButtonDisappear");
                buttons.Remove(id);
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
                SetButtonValues(buttonData, buttons[id], "ButtonPulse");
            }
            else
            {
                Debug.Log(webRequest.error);
            }
        }
    }

    private void SetButtonValues(ButtonData buttonData, GameObject button, string animation)
    {
        button.GetComponentInChildren<TMP_Text>().text = buttonData.text;
        button.GetComponent<Image>().color = HSLToColor(buttonData.color);

        if (buttonData.animationType) //if animationType == true: Play "ButtonAppear" animation
            PlayAnimation(button, animation);
    }

    private void PlayAnimation(GameObject button, string animation)
    {
        button.GetComponent<Animator>().SetTrigger(animation);
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
public class ButtonDataList
{
    public List<ButtonData> buttonData;
}

[Serializable]
public class ButtonData
{
    public float[] color;
    public string text;
    public bool animationType;
    public string id;
}