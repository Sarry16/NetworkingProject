using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemObtainedNotification : MonoBehaviour
{
    public class NotificationSettings
    {
        public string text;
    }

    static Queue<NotificationSettings> notificationQueue = new Queue<NotificationSettings>();

    bool isBusy = false;
    bool isShowing = false;

    float timeAtShow = 0;

    [SerializeField] TextMeshProUGUI textHolder;
    [SerializeField] float duration;
    [SerializeField] float durationBetween;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		//ABSTRACTION
        UpdateNextElement();
        UpdateShowing();
    }

    public static void AddToQueue(string text)
    {
        NotificationSettings settings = new NotificationSettings();
        settings.text = text;
        notificationQueue.Enqueue(settings);
    }

    void UpdateNextElement()
    {
        if (isBusy) return;
        if (notificationQueue.Count == 0) return;
        var settings = notificationQueue.Dequeue();
        textHolder.text = settings.text;
        ShowNextElement();
    }

    void ShowNextElement()
    {
        timeAtShow = Time.realtimeSinceStartup;
        isBusy = true;
        textHolder.gameObject.SetActive(true);
        isShowing = true;
    }

    void UpdateShowing()
    {
        if (!isBusy) return;

        if (!isShowing && isBusy) {
            if (Time.realtimeSinceStartup - timeAtShow >= durationBetween) isBusy = false;
            return;
        }

        if(Time.realtimeSinceStartup -  timeAtShow >= duration)
        {
            textHolder.gameObject.SetActive(false);
            isShowing = false;
            timeAtShow = Time.realtimeSinceStartup;
        }
    }

    public void TEST()
    {
        AddToQueue("YES");
    }
}
