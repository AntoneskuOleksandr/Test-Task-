using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPanel; // Панель всплывающего окна
    public TMP_InputField inputField; // Поле ввода
    public ButtonsManager buttonActions; // Скрипт ButtonActions

    // Функция для отображения всплывающего окна
    public void ShowPopup()
    {
        popupPanel.SetActive(true);
    }

    // Функция для скрытия всплывающего окна
    public void HidePopup()
    {
        popupPanel.SetActive(false);
    }

    // Функция для подтверждения ввода
    public void ConfirmInput()
    {
        int id = int.Parse(inputField.text);
        buttonActions.OnDeleteButtonPress(id);
        HidePopup();
    }
}
