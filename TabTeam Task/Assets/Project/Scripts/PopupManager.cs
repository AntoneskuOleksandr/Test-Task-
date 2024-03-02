using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPanel; // ������ ������������ ����
    public TMP_InputField inputField; // ���� �����
    public ButtonsManager buttonActions; // ������ ButtonActions

    // ������� ��� ����������� ������������ ����
    public void ShowPopup()
    {
        popupPanel.SetActive(true);
    }

    // ������� ��� ������� ������������ ����
    public void HidePopup()
    {
        popupPanel.SetActive(false);
    }

    // ������� ��� ������������� �����
    public void ConfirmInput()
    {
        int id = int.Parse(inputField.text);
        buttonActions.OnDeleteButtonPress(id);
        HidePopup();
    }
}
