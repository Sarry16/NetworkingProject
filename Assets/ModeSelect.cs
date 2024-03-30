using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeSelect : MonoBehaviour
{
    [SerializeField] TMP_Dropdown mode;
    [SerializeField] TMP_InputField port;
    [SerializeField] TMP_InputField ipv4;

    // Start is called before the first frame update
    void Start()
    {
        mode.onValueChanged.AddListener(Change);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void StartGame()
    {
        if (mode.value == 0) GameManager.Current.LocalPlay();
        else if (mode.value == 1) GameManager.Current.ServerPlay();
        else GameManager.Current.ClientPlay();
    }

    void Change(int mode)
    {
        switch(mode)
        {
            case 0:
                port.gameObject.SetActive(false);
                ipv4.gameObject.SetActive(false);
                break;
            case 1:
                port.gameObject.SetActive(true);
                ipv4.gameObject.SetActive(false);
                break;
            case 2:
                port.gameObject.SetActive(true);
                ipv4.gameObject.SetActive(true);
                break;
        }
    }
}
