using System;
using TMPro;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPanel;
    public TMP_InputField inputField;
    public ButtonsManager buttonActions;

    private Action<int> confirmAction;

    public void ShowPopup(Action<int> confirmAction)
    {
        this.confirmAction = confirmAction;
        popupPanel.SetActive(true);
    }

    public void HidePopup()
    {
        popupPanel.SetActive(false);
    }

    public void ConfirmInput()
    {
        int id = 0;
        if (!string.IsNullOrEmpty(inputField.text))
        {
            id = int.Parse(inputField.text);
        }
        confirmAction(id);
        HidePopup();
    }

}
