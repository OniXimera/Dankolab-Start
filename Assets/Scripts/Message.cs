using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Message : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    public void ShowMessage(string text)
    {
        gameObject.SetActive(true);
        _text.text = text;
    }

    public void CloseMessage()
    {
        gameObject.SetActive(false);
    }
}
