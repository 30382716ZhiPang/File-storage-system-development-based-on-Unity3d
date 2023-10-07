using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class ButtonManager : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        // Content
        public Sprite buttonIcon;
        public string buttonText = "Button";
        [Range(0.1f, 10)] public float iconScale = 1;
        [Range(10, 200)] public float textSize = 24;

        // Auto Size
        public bool autoFitContent = true;
        public Padding padding;
        [Range(0, 100)] public int spacing = 15;
        public HorizontalLayoutGroup disabledLayout;
        public HorizontalLayoutGroup normalLayout;
        public HorizontalLayoutGroup highlightedLayout;
        public HorizontalLayoutGroup mainLayout;
        public ContentSizeFitter mainFitter;
        public ContentSizeFitter targetFitter;
        public RectTransform targetRect;

        // Resources
        public CanvasGroup normalCG;
        public CanvasGroup highlightCG;
        public CanvasGroup disabledCG;
        public TextMeshProUGUI normalText;
        public TextMeshProUGUI highlightedText;
        public TextMeshProUGUI disabledText;
        public Image normalImage;
        public Image highlightImage;
        public Image disabledImage;
        public AudioSource soundSource;
        public GameObject rippleParent;

        // Settings
        public bool isInteractable = true;
        public bool enableIcon = false;
        public bool enableText = true;
        public bool useCustomContent = false;
        public bool useCustomIconSize = false;
        public bool useCustomTextSize = false;
        public bool useUINavigation = false;
        public bool enableButtonSounds = false;
        public bool useHoverSound = true;
        public bool useClickSound = true;
        public AudioClip hoverSound;
        public AudioClip clickSound;
        public bool useRipple = true;
        [Range(0.1f, 1)] public float doubleClickPeriod = 0.25f;
        [Range(0.25f, 15)] public float fadingMultiplier = 8;
        public AnimationSolution animationSolution = AnimationSolution.ScriptBased;

        // Events
        public UnityEvent onClick = new UnityEvent();
        public UnityEvent onDoubleClick = new UnityEvent();
        public UnityEvent onHover = new UnityEvent();
        public UnityEvent onLeave = new UnityEvent();

        // Ripple
        public RippleUpdateMode rippleUpdateMode = RippleUpdateMode.UnscaledTime;
        public Sprite rippleShape;
        [Range(0.1f, 5)] public float speed = 1f;
        [Range(0.5f, 25)] public float maxSize = 4f;
        public Color startColor = new Color(1f, 1f, 1f, 0.2f);
        public Color transitionColor = new Color(1f, 1f, 1f, 0f);
        public bool renderOnTop = false;
        public bool centered = false;

        // Helpers
        bool isPointerOn;
        bool waitingForDoubleClickInput;

#if UNITY_EDITOR
        public bool isPreset;
#endif

        public enum AnimationSolution { Custom, ScriptBased }
        public enum RippleUpdateMode { Normal, UnscaledTime }
        [System.Serializable] public class Padding { public int left = 20; public int right = 20; public int top = 5; public int bottom = 5; }

        void OnEnable()
        {
            UpdateUI();
        }

        void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) { return; }
#endif
            if (animationSolution == AnimationSolution.ScriptBased)
            {
                Animator tempAnimator = GetComponent<Animator>();
                if (tempAnimator != null) { Destroy(tempAnimator); }
            }

            if (gameObject.GetComponent<Image>() == null)
            {
                Image raycastImg = gameObject.AddComponent<Image>();
                raycastImg.color = new Color(0, 0, 0, 0);
                raycastImg.raycastTarget = true;
            }

            if (useUINavigation == true) { AddUINavigation(); }
            if (normalCG == null) { normalCG = new GameObject().AddComponent<CanvasGroup>(); normalCG.gameObject.AddComponent<RectTransform>(); normalCG.transform.SetParent(transform); normalCG.gameObject.name = "Normal"; }
            if (highlightCG == null) { highlightCG = new GameObject().AddComponent<CanvasGroup>(); highlightCG.gameObject.AddComponent<RectTransform>(); highlightCG.transform.SetParent(transform); highlightCG.gameObject.name = "Highlight"; }
            if (disabledCG == null) { disabledCG = new GameObject().AddComponent<CanvasGroup>(); disabledCG.gameObject.AddComponent<RectTransform>(); disabledCG.transform.SetParent(transform); disabledCG.gameObject.name = "Disabled"; }

            if (useRipple == true && rippleParent != null) { rippleParent.SetActive(false); }
            else if (useRipple == false && rippleParent != null) { Destroy(rippleParent); }

            StartCoroutine("LayoutFix");
        }

        public void UpdateUI()
        {
            if (autoFitContent == false) 
            {
                if (mainFitter != null) { mainFitter.enabled = false; }
                if (mainLayout != null) { mainLayout.enabled = false; }
                if (targetFitter != null) 
                { 
                    targetFitter.enabled = false;

                    if (targetRect != null)
                    {
                        targetRect.anchorMin = new Vector2(0, 0);
                        targetRect.anchorMax = new Vector2(1, 1);
                        targetRect.offsetMin = new Vector2(0, 0);
                        targetRect.offsetMax = new Vector2(0, 0);
                    }
                }
            }

            else
            {
                if (mainFitter != null) { mainFitter.enabled = true; }
                if (mainLayout != null) { mainLayout.enabled = true; }
                if (targetFitter != null) { targetFitter.enabled = true; }
            }

            if (disabledLayout != null) { disabledLayout.padding = new RectOffset(padding.left, padding.right, padding.top, padding.bottom); disabledLayout.spacing = spacing; }
            if (normalLayout != null) { normalLayout.padding = new RectOffset(padding.left, padding.right, padding.top, padding.bottom); normalLayout.spacing = spacing; }
            if (highlightedLayout != null) { highlightedLayout.padding = new RectOffset(padding.left, padding.right, padding.top, padding.bottom); highlightedLayout.spacing = spacing; }

            if (enableText == true)
            {
                if (normalText != null)
                {
                    normalText.gameObject.SetActive(true);
                    normalText.text = buttonText;
                    if (useCustomTextSize == false) { normalText.fontSize = textSize; }
                }

                if (highlightedText != null)
                {
                    highlightedText.gameObject.SetActive(true);
                    highlightedText.text = buttonText;
                    if (useCustomTextSize == false) { highlightedText.fontSize = textSize; }
                }

                if (disabledText != null)
                {
                    disabledText.gameObject.SetActive(true);
                    disabledText.text = buttonText;
                    if (useCustomTextSize == false) { disabledText.fontSize = textSize; }
                }
            }

            else if (enableText == false)
            {
                if (normalText != null) { normalText.gameObject.SetActive(false); }
                if (highlightedText != null) { highlightedText.gameObject.SetActive(false); }
                if (disabledText != null) { disabledText.gameObject.SetActive(false); }
            }

            if (enableIcon == true)
            {
                Vector3 tempScale = new Vector3(iconScale, iconScale, iconScale);
                if (normalImage != null) { normalImage.transform.parent.gameObject.SetActive(true); normalImage.sprite = buttonIcon; normalImage.transform.localScale = tempScale; }
                if (highlightImage != null) { highlightImage.transform.parent.gameObject.SetActive(true); highlightImage.sprite = buttonIcon; ; highlightImage.transform.localScale = tempScale; }
                if (disabledImage != null) { disabledImage.transform.parent.gameObject.SetActive(true); disabledImage.sprite = buttonIcon; ; disabledImage.transform.localScale = tempScale; }
            }

            else
            {
                if (normalImage != null) { normalImage.transform.parent.gameObject.SetActive(false); }
                if (highlightImage != null) { highlightImage.transform.parent.gameObject.SetActive(false); }
                if (disabledImage != null) { disabledImage.transform.parent.gameObject.SetActive(false); }
            }

#if UNITY_EDITOR
            if (Application.isPlaying == false && autoFitContent == true)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
                if (disabledCG != null) { LayoutRebuilder.ForceRebuildLayoutImmediate(disabledCG.GetComponent<RectTransform>()); }
                if (normalCG != null) { LayoutRebuilder.ForceRebuildLayoutImmediate(normalCG.GetComponent<RectTransform>()); }
                if (highlightCG != null) { LayoutRebuilder.ForceRebuildLayoutImmediate(highlightCG.GetComponent<RectTransform>()); }
            }
#endif

            if (Application.isPlaying == false || gameObject.activeInHierarchy == false) { return; }
            if (isInteractable == false) { StartCoroutine("SetDisabled"); }
            else if (isInteractable == true && disabledCG.alpha == 1) { StartCoroutine("SetNormal"); }

            StartCoroutine("LayoutFix");
        }

        public void SetText(string text) { buttonText = text; UpdateUI(); }
        public void SetIcon(Sprite icon) { buttonIcon = icon; UpdateUI(); }
        
        public void Interactable(bool value) 
        { 
            isInteractable = value;

            if (gameObject.activeInHierarchy == false) { return; }
            if (isInteractable == false) { StartCoroutine("SetDisabled"); }
            else if (isInteractable == true && disabledCG.alpha == 1) { StartCoroutine("SetNormal"); }
        }

        public void AddUINavigation()
        {
            Button navButton = gameObject.AddComponent<Button>();
            navButton.transition = Selectable.Transition.None;
            Navigation customNav = new Navigation();
            customNav.mode = Navigation.Mode.Automatic;
            navButton.navigation = customNav;
        }

        public void CreateRipple(Vector2 pos)
        {
            if (rippleParent != null)
            {
                GameObject rippleObj = new GameObject();
                rippleObj.AddComponent<Image>();
                rippleObj.GetComponent<Image>().sprite = rippleShape;
                rippleObj.name = "Ripple";
                rippleParent.SetActive(true);
                rippleObj.transform.SetParent(rippleParent.transform);

                if (renderOnTop == true) { rippleParent.transform.SetAsLastSibling(); }
                else { rippleParent.transform.SetAsFirstSibling(); }

                if (centered == true) { rippleObj.transform.localPosition = new Vector2(0f, 0f); }
                else { rippleObj.transform.position = pos; }

                rippleObj.AddComponent<Ripple>();
                Ripple tempRipple = rippleObj.GetComponent<Ripple>();
                tempRipple.speed = speed;
                tempRipple.maxSize = maxSize;
                tempRipple.startColor = startColor;
                tempRipple.transitionColor = transitionColor;

                if (rippleUpdateMode == RippleUpdateMode.Normal) { tempRipple.unscaledTime = false; }
                else { tempRipple.unscaledTime = true; }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isInteractable == false || eventData.button != PointerEventData.InputButton.Left) { return; }
            if (enableButtonSounds == true && useClickSound == true && soundSource != null) { soundSource.PlayOneShot(clickSound); }

            // Invoke click actions
            onClick.Invoke();

            // Check for double click
            if (waitingForDoubleClickInput == true)
            {
                onDoubleClick.Invoke();
                waitingForDoubleClickInput = false;
                return;
            }

            waitingForDoubleClickInput = true;
            //StopCoroutine("CheckForDoubleClick");
            //StartCoroutine("CheckForDoubleClick");
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isInteractable == false) { return; }
            if (useRipple == true && isPointerOn == true)
#if ENABLE_LEGACY_INPUT_MANAGER
                CreateRipple(Input.mousePosition);
#elif ENABLE_INPUT_SYSTEM
                CreateRipple(Mouse.current.position.ReadValue());
#endif
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isInteractable == false)
                return;

            if (enableButtonSounds == true && useClickSound == true && soundSource != null) { soundSource.PlayOneShot(hoverSound); }
            if (animationSolution == AnimationSolution.ScriptBased) { StartCoroutine("SetHighlight"); }

            isPointerOn = true;
            onHover.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isInteractable == false)
                return;

            StartCoroutine("SetNormal");
            isPointerOn = false;
            onLeave.Invoke();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (isInteractable == false)
                return;

            StartCoroutine("SetHighlight");
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (isInteractable == false)
                return;

            StartCoroutine("SetNormal");
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (isInteractable == false)
                return;

            onClick.Invoke();
            StartCoroutine("SetNormal");
        }

        IEnumerator LayoutFix()
        {
            yield return new WaitForSecondsRealtime(0.025f);

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            if (disabledCG != null) { LayoutRebuilder.ForceRebuildLayoutImmediate(disabledCG.GetComponent<RectTransform>()); }
            if (normalCG != null) { LayoutRebuilder.ForceRebuildLayoutImmediate(normalCG.GetComponent<RectTransform>()); }
            if (highlightCG != null) { LayoutRebuilder.ForceRebuildLayoutImmediate(highlightCG.GetComponent<RectTransform>()); }
        }

        IEnumerator SetNormal()
        {
            StopCoroutine("SetHighlight");
            StopCoroutine("SetDisabled");

            while (normalCG.alpha < 0.99f)
            {
                normalCG.alpha += Time.unscaledDeltaTime * fadingMultiplier;
                highlightCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
                disabledCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
                yield return null;
            }

            normalCG.alpha = 1;
            highlightCG.alpha = 0;
            disabledCG.alpha = 0;
        }

        IEnumerator SetHighlight()
        {
            StopCoroutine("SetNormal");
            StopCoroutine("SetDisabled");

            while (highlightCG.alpha < 0.99f)
            {
                normalCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
                highlightCG.alpha += Time.unscaledDeltaTime * fadingMultiplier;
                disabledCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
                yield return null;
            }

            normalCG.alpha = 0;
            highlightCG.alpha = 1;
            disabledCG.alpha = 0;
        }

        IEnumerator SetDisabled()
        {
            StopCoroutine("SetNormal");
            StopCoroutine("SetHighlight");

            while (disabledCG.alpha < 0.99f)
            {
                normalCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
                highlightCG.alpha -= Time.unscaledDeltaTime * fadingMultiplier;
                disabledCG.alpha += Time.unscaledDeltaTime * fadingMultiplier;
                yield return null;
            }

            normalCG.alpha = 0;
            highlightCG.alpha = 0;
            disabledCG.alpha = 1;
        }

        IEnumerator CheckForDoubleClick()
        {
            yield return new WaitForSecondsRealtime(doubleClickPeriod);
            waitingForDoubleClickInput = false;
        }
    }
}