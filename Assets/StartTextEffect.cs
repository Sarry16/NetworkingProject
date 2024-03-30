using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartTextEffect : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI startText;

    float currentSeconds = 0;
    bool effectStarted;

    public event Action OnEffectFinished;

    // Start is called before the first frame update
    void Start()
    {
        startText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (effectStarted)
        {
            string last = startText.text;
            currentSeconds -= Time.deltaTime;
            startText.text = Mathf.CeilToInt(currentSeconds).ToString();
            if(currentSeconds < 0)
            {
                effectStarted = false;
                startText.gameObject.SetActive(false);
                OnEffectFinished?.Invoke();
            } else
            {
                if(startText.text != last)
                {
                    if (GameManager.lanSession && !GameManager.isClient)
                    {
                        var msg = new Net_StartEffect(startText.text, 1);
                        GameManager.serverObject.SendToClient(msg);
                    }
                }
            }

        }

    }

    public void StartEffect(float seconds)
    {
        effectStarted = true;
        startText.text = Mathf.CeilToInt(seconds).ToString();
        startText.gameObject.SetActive(true);
        currentSeconds = seconds;

        if (GameManager.lanSession && !GameManager.isClient)
        {
            var msg = new Net_StartEffect(startText.text, 1);
            GameManager.serverObject.SendToClient(msg);
        }
    }

    public void ForceUpdateStartText(string number, int active)
    {
        startText.text = number;
        if(active == 0) startText.gameObject.SetActive(false);
        else startText.gameObject.SetActive(true);
    }
}
