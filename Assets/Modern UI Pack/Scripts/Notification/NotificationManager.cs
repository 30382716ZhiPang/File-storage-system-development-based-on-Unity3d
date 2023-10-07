using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Michsky.MUIP
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class NotificationManager : MonoBehaviour
    {
        // Content
        public Sprite icon;
        public string title = "Notification Title";
        [TextArea] public string description = "Notification description";

        // Resources
        public Animator notificationAnimator;
        public Image iconObj;
        public TextMeshProUGUI titleObj;
        public TextMeshProUGUI descriptionObj;

        // Settings
        public bool enableTimer = true;
        public float timer = 3f;
        public bool useCustomContent = false;
        public bool useStacking = false;
        [HideInInspector] public bool isOn;
        public StartBehaviour startBehaviour = StartBehaviour.Disable;
        public CloseBehaviour closeBehaviour = CloseBehaviour.Disable;

        // Events
        public UnityEvent onOpen;
        public UnityEvent onClose;

        public enum StartBehaviour { None, Disable }
        public enum CloseBehaviour { None, Disable, Destroy }

        void Awake()
        {
            isOn = false;

            if (useCustomContent == false) { UpdateUI(); }
            if (notificationAnimator == null) { notificationAnimator = gameObject.GetComponent<Animator>(); }
            if (startBehaviour == StartBehaviour.Disable) { gameObject.SetActive(false); }
            if (useStacking == true)
            {
                try
                {
                    NotificationStacking stacking = transform.GetComponentInParent<NotificationStacking>();
                    stacking.notifications.Add(this);
                    stacking.enableUpdating = true;
                }

                catch { Debug.LogError("<b>[Notification]</b> 'Stacking' is enabled but 'Notification Stacking' cannot be found in parent.", this); }
            }
        }

        public void Open()
        {
            if (isOn == true)
                return;

            gameObject.SetActive(true);
            isOn = true;

            StopCoroutine("StartTimer");
            StopCoroutine("DisableNotification");

            notificationAnimator.Play("In");
            onOpen.Invoke();

            if (enableTimer == true) { StartCoroutine("StartTimer"); }
        }

        public void Close()
        {
            if (isOn == false)
                return;

            isOn = false;
            notificationAnimator.Play("Out");
            onClose.Invoke();

            StartCoroutine("DisableNotification");
        }

        // Obsolete
        public void OpenNotification() { Open(); }
        public void CloseNotification() { Close(); }

        public void UpdateUI()
        {
            if (iconObj != null) { iconObj.sprite = icon; }
            if (titleObj != null) { titleObj.text = title; }
            if (descriptionObj != null) { descriptionObj.text = description; }
        }

        IEnumerator StartTimer()
        {
            yield return new WaitForSeconds(timer);

            CloseNotification();
            StartCoroutine("DisableNotification");
        }

        IEnumerator DisableNotification()
        {
            yield return new WaitForSeconds(1f);

            if (closeBehaviour == CloseBehaviour.Disable) { gameObject.SetActive(false); isOn = false; }
            else if (closeBehaviour == CloseBehaviour.Destroy) { Destroy(gameObject); }
        }
    }
}