using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace Michsky.MUIP
{
    public class CustomDropdown : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler, IPointerClickHandler
    {
        // Resources
        public Animator dropdownAnimator;
        public GameObject triggerObject;
        public TextMeshProUGUI selectedText;
        public Image selectedImage;
        public Transform itemParent;
        public GameObject itemObject;
        public GameObject scrollbar;
        public VerticalLayoutGroup itemList;
        public Transform listParent;
        public AudioSource soundSource;
        [HideInInspector] public Transform currentListParent;

        // Settings
        public bool isInteractable = true;
        public bool enableIcon = true;
        public bool enableTrigger = true;
        public bool enableScrollbar = true;
        public bool setHighPriorty = true;
        public bool updateOnEnable = true;
        public bool outOnPointerExit = false;
        public bool isListItem = false;
        public bool invokeAtStart = false;
        public bool initAtStart = true;
        public bool enableDropdownSounds = false;
        public bool useHoverSound = true;
        public bool useClickSound = true;
        [Range(1, 50)] public int itemPaddingTop = 8;
        [Range(1, 50)] public int itemPaddingBottom = 8;
        [Range(1, 50)] public int itemPaddingLeft = 8;
        [Range(1, 50)] public int itemPaddingRight = 25;
        [Range(1, 50)] public int itemSpacing = 8;
        public int selectedItemIndex = 0;

        // Animation
        public AnimationType animationType;
        [Range(1, 25)] public float transitionSmoothness = 10;
        [Range(1, 25)] public float sizeSmoothness = 15;
        public float panelSize = 200;
        public RectTransform listRect;
        public CanvasGroup listCG;
        bool isInTransition = false;
        float closeOn;

        // Saving
        public bool saveSelected = false;
        public string saveKey = "My Dropdown";

        // Item list
        [SerializeField]
        public List<Item> items = new List<Item>();
        [System.Serializable]
        public class DropdownEvent : UnityEvent<int> { }
        [Space(8)] public DropdownEvent onValueChanged;

        // Audio
        public AudioClip hoverSound;
        public AudioClip clickSound;

        // Other variables
        [HideInInspector] public bool isOn;
        [HideInInspector] public int index = 0;
        [HideInInspector] public int siblingIndex = 0;
        [HideInInspector] public TextMeshProUGUI setItemText;
        [HideInInspector] public Image setItemImage;
        EventTrigger triggerEvent;
        Sprite imageHelper;
        string textHelper;

        public enum AnimationType { Modular, Custom }

        [System.Serializable]
        public class Item
        {
            public string itemName = "Dropdown Item";
            public Sprite itemIcon;
            [HideInInspector] public int itemIndex;
            public UnityEvent OnItemSelection = new UnityEvent();
        }

        void OnEnable()
        {
            if (animationType == AnimationType.Custom) { return; }
            else if (animationType == AnimationType.Modular && dropdownAnimator != null) { Destroy(dropdownAnimator); }

            if (listCG == null) { listCG = gameObject.GetComponentInChildren<CanvasGroup>(); }
            if (listRect == null) { listRect = listCG.GetComponent<RectTransform>(); }
            if (updateOnEnable == true && index < items.Count) { ChangeDropdownInfo(index); }

            listCG.alpha = 0;
            listCG.interactable = false;
            listCG.blocksRaycasts = false;

            closeOn = gameObject.GetComponent<RectTransform>().sizeDelta.y;
            listRect.sizeDelta = new Vector2(listRect.sizeDelta.x, closeOn);
        }

        void Awake()
        {
            if (initAtStart == true) { SetupDropdown(); }
            if (enableTrigger == true && triggerObject != null)
            {
                // triggerButton = gameObject.GetComponent<Button>();
                triggerEvent = triggerObject.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener((eventData) => { Animate(); });
                triggerEvent.GetComponent<EventTrigger>().triggers.Add(entry);
            }

            currentListParent = transform.parent;
        }

        void Update()
        {
            if (isInTransition == false)
                return;

            ProcessModularAnimation();
        }

        void ProcessModularAnimation()
        {
            if (isOn == true)
            {
                listCG.alpha += Time.unscaledDeltaTime * transitionSmoothness;
                listRect.sizeDelta = Vector2.Lerp(listRect.sizeDelta, new Vector2(listRect.sizeDelta.x, panelSize), Time.unscaledDeltaTime * sizeSmoothness);

                if (listRect.sizeDelta.y >= panelSize - 0.1f && listCG.alpha >= 1) { isInTransition = false; }
            }

            else
            {
                listCG.alpha -= Time.unscaledDeltaTime * transitionSmoothness;
                listRect.sizeDelta = Vector2.Lerp(listRect.sizeDelta, new Vector2(listRect.sizeDelta.x, closeOn), Time.unscaledDeltaTime * sizeSmoothness);

                if (listRect.sizeDelta.y <= closeOn + 0.1f && listCG.alpha <= 0) { isInTransition = false; }
            }
        }

        public void SetupDropdown()
        {
            if (dropdownAnimator == null) { dropdownAnimator = gameObject.GetComponent<Animator>(); }
            if (enableScrollbar == false && scrollbar != null) { Destroy(scrollbar); }
            if (setHighPriorty == true) { transform.SetAsLastSibling(); }
            if (itemList == null) { itemList = itemParent.GetComponent<VerticalLayoutGroup>(); }

            UpdateItemLayout();
            index = 0;

            foreach (Transform child in itemParent) { Destroy(child.gameObject); }
            for (int i = 0; i < items.Count; ++i)
            {
                GameObject go = Instantiate(itemObject, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(itemParent, false);
                go.name = items[i].itemName;

                setItemText = go.GetComponentInChildren<TextMeshProUGUI>();
                textHelper = items[i].itemName;
                setItemText.text = textHelper;

                Transform goImage = go.gameObject.transform.Find("Icon");
                setItemImage = goImage.GetComponent<Image>();

                if (items[i].itemIcon == null) { setItemImage.gameObject.SetActive(false); }
                else { imageHelper = items[i].itemIcon; setItemImage.sprite = imageHelper; }
              
                items[i].itemIndex = i;
                CustomDropdown.Item mainItem = items[i];

                Button itemButton = go.GetComponent<Button>();
                itemButton.onClick.AddListener(Animate);
                itemButton.onClick.AddListener(delegate
                {
                    ChangeDropdownInfo(index = mainItem.itemIndex);
                    onValueChanged.Invoke(index = mainItem.itemIndex);

                    if (saveSelected == true) { PlayerPrefs.SetInt("Dropdown_" + saveKey, mainItem.itemIndex); }
                });

                itemButton.onClick.AddListener(items[i].OnItemSelection.Invoke);
                if (invokeAtStart == true) { items[i].OnItemSelection.Invoke(); }
            }

            if (selectedImage != null && enableIcon == false) { selectedImage.gameObject.SetActive(false); }
            else if (selectedImage != null) { selectedImage.sprite = items[selectedItemIndex].itemIcon; }
            if (selectedText != null) { selectedText.text = items[selectedItemIndex].itemName; }
            if (saveSelected == true)
            {
                if (invokeAtStart == true) { items[PlayerPrefs.GetInt("Dropdown_" + saveKey)].OnItemSelection.Invoke(); }
                else { ChangeDropdownInfo(PlayerPrefs.GetInt("Dropdown_" + saveKey)); }
            }

            currentListParent = transform.parent;
        }

        public void ChangeDropdownInfo(int itemIndex)
        {
            if (selectedImage != null && enableIcon == true) { selectedImage.sprite = items[itemIndex].itemIcon; }
            if (selectedText != null) { selectedText.text = items[itemIndex].itemName; }
            if (enableDropdownSounds == true && useClickSound == true) { soundSource.PlayOneShot(clickSound); }

            selectedItemIndex = itemIndex;
        }

        public void Animate()
        {
            if (isOn == false && animationType == AnimationType.Modular)
            {
                isOn = true;
                isInTransition = true;
                listCG.blocksRaycasts = true;
                listCG.interactable = true;

                if (isListItem == true)
                {
                    siblingIndex = transform.GetSiblingIndex();
                    gameObject.transform.SetParent(listParent, true);
                }
            }

            else if (isOn == true && animationType == AnimationType.Modular)
            {
                isOn = false;
                isInTransition = true;
                listCG.blocksRaycasts = false;
                listCG.interactable = false;

                if (isListItem == true)
                {
                    gameObject.transform.SetParent(currentListParent, true);
                    gameObject.transform.SetSiblingIndex(siblingIndex);
                }
            }

            else if (isOn == false && animationType == AnimationType.Custom)
            {
                dropdownAnimator.Play("Stylish In");
                isOn = true;

                if (isListItem == true)
                {
                    siblingIndex = transform.GetSiblingIndex();
                    gameObject.transform.SetParent(listParent, true);
                }
            }

            else if (isOn == true && animationType == AnimationType.Custom)
            {
                dropdownAnimator.Play("Stylish Out");
                isOn = false;

                if (isListItem == true)
                {
                    gameObject.transform.SetParent(currentListParent, true);
                    gameObject.transform.SetSiblingIndex(siblingIndex);
                }
            }

            if (enableTrigger == true && isOn == false) { triggerObject.SetActive(false); }
            else if (enableTrigger == true && isOn == true) { triggerObject.SetActive(true); }

            if (enableTrigger == true && outOnPointerExit == true) { triggerObject.SetActive(false); }
            if (setHighPriorty == true) { transform.SetAsLastSibling(); }
        }

        public void CreateNewItem(string title, Sprite icon, bool notify)
        {
            Item item = new Item();
            item.itemName = title;
            item.itemIcon = icon;
            items.Add(item);

            if (notify == true)
                SetupDropdown();
        }

        public void CreateNewItem(string title, bool notify)
        {
            Item item = new Item();
            item.itemName = title;
            items.Add(item);

            if (notify == true)
                SetupDropdown();
        }

        public void CreateNewItem(string title)
        {
            Item item = new Item();
            item.itemName = title;
            items.Add(item);
            SetupDropdown();
        }

        public void RemoveItem(string itemTitle, bool notify)
        {
            var item = items.Find(x => x.itemName == itemTitle);
            items.Remove(item);
            if (notify == true) { SetupDropdown(); }
        }

        public void RemoveItem(string itemTitle)
        {
            var item = items.Find(x => x.itemName == itemTitle);
            items.Remove(item);
            SetupDropdown();
        }

        public void UpdateItemLayout()
        {
            if (itemList != null)
            {
                itemList.spacing = itemSpacing;
                itemList.padding.top = itemPaddingTop;
                itemList.padding.bottom = itemPaddingBottom;
                itemList.padding.left = itemPaddingLeft;
                itemList.padding.right = itemPaddingRight;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isInteractable == false) { return; }
            if (enableDropdownSounds == true && useClickSound == true) { soundSource.PlayOneShot(clickSound); }
            Animate();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isInteractable == false) { return; }
            if (enableDropdownSounds == true && useHoverSound == true) { soundSource.PlayOneShot(hoverSound); }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (outOnPointerExit == true && isOn == true)
            {
                Animate();
                isOn = false;

                if (isListItem == true) { gameObject.transform.SetParent(currentListParent, true); }
            }
        }
    }
}