//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//  Additions and extensions created by Lumos
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A scrollable edition of the standard PopupList. Allows for the displaying of many options in the same list; includes a scrollbar.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Scrollable Popup List")]
public class UIScrollablePopupList : UIWidgetContainer
{
    /// <summary>
    /// Current popup list. Only available during the OnSelectionChange event callback.
    /// </summary>

    static public UIScrollablePopupList current;

    const float animSpeed = 0.15f;

    public enum Position
    {
        Auto,
        Above,
        Below,
    }

    /// <summary>
    /// Atlas used by the sprites.
    /// </summary>
    public UIAtlas atlas;

    // Lumos:
    /// <summary>
    /// The atlas used by the scrollbar sprites.
    /// </summary>
    public UIAtlas scrollbarAtlas;

    /// <summary>
    /// The name of the scrollbar background sprite.
    /// </summary>
    public string scrollbarSpriteName;

    /// <summary>
    /// The name of the scrollbar foreground sprite.
    /// </summary>
    public string scrollbarForegroundName;

    /// <summary>
    /// Font used by the labels.
    /// </summary>
    public UIFont bitmapFont;

    /// <summary>
    /// True type font used by the labels. Alternative to specifying a bitmap font ('font').
    /// </summary>
    public Font trueTypeFont;

    /// <summary>
    /// Font used by the popup list. Conveniently wraps both dynamic and bitmap fonts into one property.
    /// </summary>
    public Object ambigiousFont
    {
        get
        {
            if (trueTypeFont != null) return trueTypeFont;
            if (bitmapFont != null) return bitmapFont;
            return font;
        }
        set
        {
            if (value is Font)
            {
                trueTypeFont = value as Font;
                bitmapFont = null;
                font = null;
            }
            else if (value is UIFont)
            {
                bitmapFont = value as UIFont;
                trueTypeFont = null;
                font = null;
            }
        }
    }

    /// <summary>
    /// Size of the font to use for the popup list's labels.
    /// </summary>
    public int fontSize = 16;

    /// <summary>
    /// Font style used by the dynamic font.
    /// </summary>
    public FontStyle fontStyle = FontStyle.Normal;

    /// <summary>
    /// Name of the sprite used to create the popup's background.
    /// </summary>
    public string backgroundSprite;

    /// <summary>
    /// Name of the sprite used to highlight items.
    /// </summary>
    public string highlightSprite;

    /// <summary>
    /// Popup list's display style.
    /// </summary>
    public Position position = Position.Auto;

    /// <summary>
    /// New line-delimited list of items.
    /// </summary>
    public List<string> items = new List<string>();

    /// <summary>
    /// Amount of padding added to labels.
    /// </summary>
    public Vector2 padding = new Vector3(4f, 4f);

    /// <summary>
    /// Color tint applied to labels inside the list.
    /// </summary>
    public Color textColor = Color.white;

    /// <summary>
    /// Color tint applied to the background.
    /// </summary>
    public Color backgroundColor = Color.white;

    /// <summary>
    /// Color tint applied to the highlighter.
    /// </summary>
    public Color highlightColor = new Color(225f / 255f, 200f / 255f, 150f / 255f, 1f);

    // Lumos: (can't be arsed to write summaries for these, sorry)
    public Color scrollbarBgDefColour;
    public Color scrollbarBgHovColour;
    public Color scrollbarBgPrsColour;
    public Color scrollbarFgDefColour;
    public Color scrollbarFgHovColour;
    public Color scrollbarFgPrsColour;

    /// <summary>
    /// Whether the popup list is animated or not. Disable for better performance.
    /// </summary>
    public bool isAnimated = true;

    /// <summary>
    /// Whether the popup list's values will be localized.
    /// </summary>
    public bool isLocalized = false;

    // Lumos: Some new vars around here
    /// <summary>
    /// The maximum height the list is allowed to reach before clipping.
    /// </summary>
    public int maxHeight = 100;

    public int scrollbarWidth = 30, scrollbarHeight = 300;


    /// <summary>
    /// Callbacks triggered when the popup list gets a new item selection.
    /// </summary>
    public List<EventDelegate> onChange = new List<EventDelegate>();

    // Currently selected item
    [HideInInspector]
    [SerializeField]
    string mSelectedItem;

    UIPanel mPanel;
    GameObject mChild;
    UISprite mBackground;
    UISprite mHighlight;
    UILabel mHighlightedLabel = null;
    List<UILabel> mLabelList = new List<UILabel>();
    float mBgBorder = 0f;
    // Lumos:
    UIPanel mClippingPanel;
    UISprite scrollbarSprite;
    UISprite scrForegroundSprite;

    // Deprecated functionality
    [HideInInspector]
    [SerializeField]
    GameObject eventReceiver;
    [HideInInspector]
    [SerializeField]
    string functionName = "OnSelectionChange";
    [HideInInspector]
    [SerializeField]
    float textScale = 0f;
    [HideInInspector]
    [SerializeField]
    UIFont font; // Use 'bitmapFont' instead

    // This functionality is no longer needed as the same can be achieved by choosing a
    // OnValueChange notification targeting a label's SetCurrentSelection function.
    // If your code was list.textLabel = myLabel, change it to:
    // EventDelegate.Add(list.onChange, lbl.SetCurrentSelection);
    [HideInInspector]
    [SerializeField]
    UILabel textLabel;

    public delegate void LegacyEvent(string val);
    LegacyEvent mLegacyEvent;

    [System.Obsolete("Use EventDelegate.Add(popup.onChange, YourCallback) instead, and UIPopupList.current.value to determine the state")]
    public LegacyEvent onSelectionChange { get { return mLegacyEvent; } set { mLegacyEvent = value; } }

    /// <summary>
    /// Whether the popup list is currently open.
    /// </summary>
    public bool isOpen { get { return mChild != null; } }

    /// <summary>
    /// Current selection.
    /// </summary>
    public string value
    {
        get
        {
            return mSelectedItem;
        }
        set
        {
            mSelectedItem = value;
            if (mSelectedItem == null) return;
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (mSelectedItem != null)
                TriggerCallbacks();
        }
    }

    [System.Obsolete("Use 'value' instead")]
    public string selection { get { return value; } set { this.value = value; } }

    /// <summary>
    /// Whether the popup list will be handling keyboard, joystick and controller events.
    /// </summary>
    bool handleEvents
    {
        get
        {
            UIKeyNavigation keys = GetComponent<UIKeyNavigation>();
            return (keys == null || !keys.enabled);
        }
        set
        {
            UIKeyNavigation keys = GetComponent<UIKeyNavigation>();
            if (keys != null) keys.enabled = !value;
        }
    }

    /// <summary>
    /// Whether the popup list is actually usable.
    /// </summary>
    bool isValid { get { return bitmapFont != null || trueTypeFont != null; } }

    /// <summary>
    /// Active font size.
    /// </summary>
    int activeFontSize { get { return (trueTypeFont != null || bitmapFont == null) ? fontSize : bitmapFont.defaultSize; } }

    /// <summary>
    /// Font scale applied to the popup list's text.
    /// </summary>
    float activeFontScale { get { return (trueTypeFont != null || bitmapFont == null) ? 1f : (float)fontSize / bitmapFont.defaultSize; } }

    /// <summary>
    /// Trigger all event notification callbacks.
    /// </summary>
    protected void TriggerCallbacks()
    {
        if (current != this)
        {
            UIScrollablePopupList old = current;
            current = this;

            // Legacy functionality
            if (mLegacyEvent != null) mLegacyEvent(mSelectedItem);

            if (EventDelegate.IsValid(onChange))
            {
                EventDelegate.Execute(onChange);
            }
            else if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
            {
                // Legacy functionality support (for backwards compatibility)
                eventReceiver.SendMessage(functionName, mSelectedItem, SendMessageOptions.DontRequireReceiver);
            }
            current = old;
        }
    }

    /// <summary>
    /// Remove legacy functionality.
    /// </summary>
    void OnEnable()
    {
        if (EventDelegate.IsValid(onChange))
        {
            eventReceiver = null;
            functionName = null;
        }

        // 'font' is no longer used
        if (font != null)
        {
            if (font.isDynamic)
            {
                trueTypeFont = font.dynamicFont;
                fontStyle = font.dynamicFontStyle;
                mUseDynamicFont = true;
            }
            else if (bitmapFont == null)
            {
                bitmapFont = font;
                mUseDynamicFont = false;
            }
            font = null;
        }

        // 'textScale' is no longer used
        if (textScale != 0f)
        {
            fontSize = (bitmapFont != null) ? Mathf.RoundToInt(bitmapFont.defaultSize * textScale) : 16;
            textScale = 0f;
        }

        // Auto-upgrade to the true type font
        if (trueTypeFont == null && bitmapFont != null && bitmapFont.isDynamic)
        {
            trueTypeFont = bitmapFont.dynamicFont;
            bitmapFont = null;
        }
    }

    bool mUseDynamicFont = false;

    void OnValidate()
    {
        Font ttf = trueTypeFont;
        UIFont fnt = bitmapFont;

        bitmapFont = null;
        trueTypeFont = null;

        if (ttf != null && (fnt == null || !mUseDynamicFont))
        {
            bitmapFont = null;
            trueTypeFont = ttf;
            mUseDynamicFont = true;
        }
        else if (fnt != null)
        {
            // Auto-upgrade from 3.0.2 and earlier
            if (fnt.isDynamic)
            {
                trueTypeFont = fnt.dynamicFont;
                fontStyle = fnt.dynamicFontStyle;
                fontSize = fnt.defaultSize;
                mUseDynamicFont = true;
            }
            else
            {
                bitmapFont = fnt;
                mUseDynamicFont = false;
            }
        }
        else
        {
            trueTypeFont = ttf;
            mUseDynamicFont = true;
        }
    }

    /// <summary>
    /// Send out the selection message on start.
    /// </summary>
    void Start()
    {
        // Auto-upgrade legacy functionality
        if (textLabel != null)
        {
            EventDelegate.Add(onChange, textLabel.SetCurrentSelection);
            textLabel = null;
#if UNITY_EDITOR
            NGUITools.SetDirty(this);
#endif
        }

        if (Application.isPlaying)
        {
            // Automatically choose the first item
            if (string.IsNullOrEmpty(mSelectedItem))
            {
                if (items.Count > 0) value = items[0];
            }
            else
            {
                string s = mSelectedItem;
                mSelectedItem = null;
                value = s;
            }
        }
    }

    /// <summary>
    /// Localize the text label.
    /// </summary>
    void OnLocalize() { if (isLocalized) TriggerCallbacks(); }

    /// <summary>
    /// Visibly highlight the specified transform by moving the highlight sprite to be over it.
    /// </summary>
    void Highlight(UILabel lbl, bool instant)
    {

        if (mHighlight != null)
        {
            // Don't allow highlighting while the label is animating to its intended position
            TweenPosition tp = lbl.GetComponent<TweenPosition>();
            if (tp != null && tp.enabled) return;

            mHighlightedLabel = lbl;

            UISpriteData sp = mHighlight.GetAtlasSprite();
            if (sp == null) return;

            float scaleFactor = atlas.pixelSize;
            float offsetX = sp.borderLeft * scaleFactor;
            float offsetY = sp.borderTop * scaleFactor;

            Vector3 pos = lbl.cachedTransform.localPosition + new Vector3(-offsetX, offsetY, 1f);

            if (instant || !isAnimated)
            {
                mHighlight.cachedTransform.localPosition = pos;
            }
            else
            {
                TweenPosition.Begin(mHighlight.gameObject, 0.1f, pos).method = UITweener.Method.EaseOut;
            }
        }
    }

    /// <summary>
    /// Event function triggered when the mouse hovers over an item.
    /// </summary>
    void OnItemHover(GameObject go, bool isOver)
    {
        if (isOver)
        {
            UILabel lbl = go.GetComponent<UILabel>();
            Highlight(lbl, false);
        }
    }

    /// <summary>
    /// Select the specified label.
    /// </summary>
    void Select(UILabel lbl, bool instant)
    {
        Highlight(lbl, instant);

        UIEventListener listener = lbl.gameObject.GetComponent<UIEventListener>();
        value = listener.parameter as string;

        UIPlaySound[] sounds = GetComponents<UIPlaySound>();

        for (int i = 0, imax = sounds.Length; i < imax; ++i)
        {
            UIPlaySound snd = sounds[i];

            if (snd.trigger == UIPlaySound.Trigger.OnClick)
            {
                NGUITools.PlaySound(snd.audioClip, snd.volume, 1f);
            }
        }

        Close(); // über h4x
    }

    /// <summary>
    /// Event function triggered when the drop-down list item gets clicked on.
    /// </summary>
    void OnItemPress(GameObject go, bool isPressed) { if (isPressed) Select(go.GetComponent<UILabel>(), true); }

    /// <summary>
    /// React to key-based input; Automatically closes after selecting something though.
    /// </summary>
    void OnKey(KeyCode key)
    {
        if (enabled && NGUITools.GetActive(gameObject) && handleEvents)
        {
            int index = mLabelList.IndexOf(mHighlightedLabel);
            if (index == -1) index = 0;

            if (key == KeyCode.UpArrow)
            {
                if (index > 0)
                {
                    Select(mLabelList[--index], false);

                }
            }
            else if (key == KeyCode.DownArrow)
            {
                if (index + 1 < mLabelList.Count)
                {
                    Select(mLabelList[++index], false);
                }
            }
            else if (key == KeyCode.Escape)
            {
                OnSelect(false);
            }
        }
    }

    /// <summary>
    /// Get rid of the popup dialog when the selection gets lost.
    /// </summary>
    void OnSelect(bool isSelected) { if (!isSelected) Close(); }

    /// <summary>
    /// Manually close the popup list.
    /// </summary>
    public void Close()
    {
        if (mChild != null)
        {
            // Lumos: Using the scrollbar does not close the list
            if (UICamera.hoveredObject)
            { // the hoveredObject is null if the user's clicked on nothing
                if (UICamera.hoveredObject == scrollbarSprite.gameObject || UICamera.hoveredObject == scrForegroundSprite.gameObject)
                {
                    //Debug.Log("Scrollbar check");
                    return;
                }
                else goto JustClose; // Yes, because I'm a horrible person and I want to make you cringe
            }

        JustClose:

            mLabelList.Clear();
            handleEvents = false;

            if (isAnimated)
            {
                UIWidget[] widgets = mChild.GetComponentsInChildren<UIWidget>();

                for (int i = 0, imax = widgets.Length; i < imax; ++i)
                {
                    UIWidget w = widgets[i];
                    Color c = w.color;
                    c.a = 0f;
                    TweenColor.Begin(w.gameObject, animSpeed, c).method = UITweener.Method.EaseOut;
                }

                Collider[] cols = mChild.GetComponentsInChildren<Collider>();
                for (int i = 0, imax = cols.Length; i < imax; ++i) cols[i].enabled = false;
                Destroy(mChild, animSpeed);
            }
            else Destroy(mChild);

            mBackground = null;
            mHighlight = null;
            mChild = null;
            mClippingPanel = null;
            scrollbarSprite = null;
            scrForegroundSprite = null;
        }
    }

    /// <summary>
    /// Helper function that causes the widget to smoothly fade in.
    /// </summary>
    void AnimateColor(UIWidget widget)
    {
        Color c = widget.color;
        widget.color = new Color(c.r, c.g, c.b, 0f);
        TweenColor.Begin(widget.gameObject, animSpeed, c).method = UITweener.Method.EaseOut;
    }

    /// <summary>
    /// Helper function that causes the widget to smoothly move into position.
    /// </summary>
    void AnimatePosition(UIWidget widget, bool placeAbove, float bottom)
    {
        Vector3 target = widget.cachedTransform.localPosition;
        Vector3 start = placeAbove ? new Vector3(target.x, bottom, target.z) : new Vector3(target.x, 0f, target.z);

        widget.cachedTransform.localPosition = start;

        GameObject go = widget.gameObject;
        TweenPosition.Begin(go, animSpeed, target).method = UITweener.Method.EaseOut;
    }

    /// <summary>
    /// Helper function that causes the widget to smoothly grow until it reaches its original size.
    /// </summary>
    void AnimateScale(UIWidget widget, bool placeAbove, float bottom)
    {
        GameObject go = widget.gameObject;
        Transform t = widget.cachedTransform;

        float minHeight = activeFontSize * activeFontScale + mBgBorder * 2f;
        t.localScale = new Vector3(1f, minHeight / widget.height, 1f);
        TweenScale.Begin(go, animSpeed, Vector3.one).method = UITweener.Method.EaseOut;

        if (placeAbove)
        {
            Vector3 pos = t.localPosition;
            t.localPosition = new Vector3(pos.x, pos.y - widget.height + minHeight, pos.z);
            TweenPosition.Begin(go, animSpeed, pos).method = UITweener.Method.EaseOut;
        }
    }

    /// <summary>
    /// Helper function used to animate widgets.
    /// </summary>
    void Animate(UIWidget widget, bool placeAbove, float bottom)
    {
        AnimateColor(widget);
        AnimatePosition(widget, placeAbove, bottom);
    }

    /// <summary>
    /// Display the drop-down list when the game object gets clicked on.
    /// </summary>
    void OnClick()
    {
        if (enabled && NGUITools.GetActive(gameObject) && mChild == null && atlas != null && isValid && items.Count > 0)
        {
            mLabelList.Clear();

            // Automatically locate the panel responsible for this object
            if (mPanel == null)
            {
                mPanel = UIPanel.Find(transform);
                if (mPanel == null) return;
            }

            // Disable the navigation script
            handleEvents = true;

            // Calculate the dimensions of the object triggering the popup list so we can position it below it
            Transform myTrans = transform;
            Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(myTrans.parent, myTrans);

            // Create the root object for the list
            mChild = new GameObject("Drop-down List");
            mChild.layer = gameObject.layer;

            Transform t = mChild.transform;
            t.parent = myTrans.parent;
            t.localPosition = bounds.min;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;

            // Add a sprite for the background
            mBackground = NGUITools.AddSprite(mChild, atlas, backgroundSprite);
            mBackground.pivot = UIWidget.Pivot.TopLeft;
            mBackground.depth = NGUITools.CalculateNextDepth(mPanel.gameObject);
            mBackground.color = backgroundColor;
            mBackground.gameObject.name = "SpriteBackground";
            mBackground.gameObject.AddComponent<UIDragScrollView>();
            // Lumos: Create the clipping panel
            GameObject mClipper = new GameObject("Panel");
            mClipper.layer = gameObject.layer;
            Transform t2 = mClipper.transform;
            t2.parent = mChild.transform;
            t2.localPosition = Vector3.zero;
            t2.localRotation = Quaternion.identity;
            t2.localScale = Vector3.one;

            // Lumos: Add the panel itself
            mClippingPanel = mClipper.AddComponent<UIPanel>();
            mClippingPanel.clipping = UIDrawCall.Clipping.SoftClip;
            mClippingPanel.leftAnchor.target = mBackground.transform;
            mClippingPanel.rightAnchor.target = mBackground.transform;
            mClippingPanel.topAnchor.target = mBackground.transform;
            mClippingPanel.bottomAnchor.target = mBackground.transform;
            mClippingPanel.ResetAnchors();
            mClippingPanel.UpdateAnchors();
            mClippingPanel.depth = NGUITools.CalculateNextDepth(mPanel.gameObject);
            //mClippingPanel.

            // Lumos: The panel also needs an UIScrollView to actually allow scrolling
            UIScrollView mScroller = mClipper.AddComponent<UIScrollView>();
            mScroller.contentPivot = UIWidget.Pivot.TopLeft;
            mScroller.movement = UIScrollView.Movement.Vertical;
            mScroller.scrollWheelFactor = 0.25f;
            mScroller.disableDragIfFits = true;
            mScroller.dragEffect = UIScrollView.DragEffect.None;
            mScroller.ResetPosition();
            mScroller.UpdatePosition();
            mScroller.RestrictWithinBounds(true);

            // We need to know the size of the background sprite for padding purposes
            Vector4 bgPadding = mBackground.border;
            mBgBorder = bgPadding.y;
            mBackground.cachedTransform.localPosition = new Vector3(0f, bgPadding.y, 0f);

            // Add a sprite used for the selection
            mHighlight = NGUITools.AddSprite(mClipper, atlas, highlightSprite);
            mHighlight.pivot = UIWidget.Pivot.TopLeft;
            mHighlight.color = highlightColor;
            mHighlight.gameObject.name = "Highlighter";

            UISpriteData hlsp = mHighlight.GetAtlasSprite();
            if (hlsp == null) return;

            float hlspHeight = hlsp.borderTop;
            float fontHeight = activeFontSize;
            float dynScale = activeFontScale;
            float labelHeight = fontHeight * dynScale;
            float x = 0f, y = -padding.y;
            int labelFontSize = (bitmapFont != null) ? bitmapFont.defaultSize : fontSize;
            List<UILabel> labels = new List<UILabel>();

            // Run through all items and create labels for each one
            for (int i = 0, imax = items.Count; i < imax; ++i)
            {
                string s = items[i];

                UILabel lbl = NGUITools.AddWidget<UILabel>(mClipper);
                lbl.pivot = UIWidget.Pivot.TopLeft;
                lbl.bitmapFont = bitmapFont;
                lbl.trueTypeFont = trueTypeFont;
                lbl.fontSize = labelFontSize;
                lbl.fontStyle = fontStyle;
                lbl.text = isLocalized ? Localization.Get(s) : s;
                lbl.color = textColor;
                lbl.cachedTransform.localPosition = new Vector3(bgPadding.x + padding.x, y, -1f);
                lbl.overflowMethod = UILabel.Overflow.ResizeFreely;
                lbl.MakePixelPerfect();
                // Lumos: Add a DragScrollView to the labels
                lbl.gameObject.AddComponent<UIDragScrollView>();
                if (dynScale != 1f) lbl.cachedTransform.localScale = Vector3.one * dynScale;
                labels.Add(lbl);

                y -= labelHeight;
                y -= padding.y;
                x = Mathf.Max(x, lbl.printedSize.x);

                // Add an event listener
                UIEventListener listener = UIEventListener.Get(lbl.gameObject);
                listener.onHover = OnItemHover;
                listener.onPress = OnItemPress;
                listener.parameter = s;

                // Move the selection here if this is the right label
                if (mSelectedItem == s || (i == 0 && string.IsNullOrEmpty(mSelectedItem)))
                    Highlight(lbl, true);

                // Add this label to the list
                mLabelList.Add(lbl);
            }

            //mScroller.ResetPosition();

            // The triggering widget's width should be the minimum allowed width
            x = Mathf.Max(x, bounds.size.x * dynScale - (bgPadding.x + padding.x) * 2f);

            float cx = x / dynScale;
            // Vector3 bcCenter = new Vector3(cx * 0.5f, -fontHeight * 0.5f, 0f);
            // Vector3 bcSize = new Vector3(cx, (labelHeight + padding.y) / dynScale, 1f);

            // Run through all labels and add colliders
            for (int i = 0, imax = labels.Count; i < imax; ++i)
            {
                UILabel lbl = labels[i];
                NGUITools.AddWidgetCollider(lbl.gameObject);
                BoxCollider bc = GetComponent<BoxCollider>();       // 修改
                lbl.GetComponent<BoxCollider>().size = bc.size;     // 修改
                lbl.GetComponent<BoxCollider>().center = bc.center; // 修改
                // bcCenter.z = bc.center.z;
                // bc.center = bcCenter;
                // bc.size = bcSize;   
            }

            x += (bgPadding.x + padding.x) * 2f;
            y -= bgPadding.y;

            // Scale the background sprite to envelop the entire set of items
            mBackground.width = Mathf.RoundToInt(gameObject.GetComponent<UISprite>().width * gameObject.transform.localScale.x) - scrollbarWidth;
            //mRealBackground.width = Mathf.RoundToInt(x);
            // Lumos: Actually, check if the height is not larger than the maximum
            //mBackground.height = Mathf.RoundToInt(-y + bgPadding.y);
            int targetHeight = Mathf.RoundToInt(-y + bgPadding.y);
            if (maxHeight == 0) maxHeight = targetHeight; // maxHeight = 0 disables it, so the entire list is shown
            mBackground.height = (targetHeight > maxHeight) ? maxHeight : targetHeight;
            //mRealBackground.height = targetHeight;
            //NGUITools.AddWidgetCollider(mRealBackground.gameObject);

            mScroller.ResetPosition();
            mScroller.UpdatePosition(); // not quite certain what these are
            mScroller.RestrictWithinBounds(true, true, true);
            t2.localPosition = Vector3.zero;

            // Scale the highlight sprite to envelop a single item
            float scaleFactor = 2f * atlas.pixelSize;
            float w = x - (bgPadding.x + padding.x) * 2f + hlsp.borderLeft * scaleFactor;
            float h = labelHeight + hlspHeight * scaleFactor;
            mHighlight.width = Mathf.RoundToInt(w) - 5; // Lumos: 5 pixels less, so it doesn't overlap the scrollbar
            mHighlight.height = Mathf.RoundToInt(h);

            bool placeAbove = (position == Position.Above);

            if (position == Position.Auto)
            {
                UICamera cam = UICamera.FindCameraForLayer(gameObject.layer);

                if (cam != null)
                {
                    Vector3 viewPos = cam.cachedCamera.WorldToViewportPoint(myTrans.position);
                    placeAbove = (viewPos.y < 0.5f);
                }
            }

            // If the list should be animated, let's animate it by expanding it
            if (isAnimated)
            {
                float bottom = y + labelHeight;
                Animate(mHighlight, placeAbove, bottom);
                for (int i = 0, imax = labels.Count; i < imax; ++i) Animate(labels[i], placeAbove, bottom);
                AnimateColor(mBackground);
                AnimateScale(mBackground, placeAbove, bottom);
            }

            // If we need to place the popup list above the item, we need to reposition everything by the size of the list
            if (placeAbove)
            {
                // Needs adjustment because we're not showing the entire list at the same time
                //t.localPosition = new Vector3(bounds.min.x, bounds.max.y - y - bgPadding.y, bounds.min.z);
                t.localPosition = new Vector3(bounds.min.x, bounds.min.y + mBackground.height + myTrans.GetComponent<UISprite>().height - bgPadding.y, bounds.min.z);
            }

            // Lumos: Let's now work on the list's vertical scrollbar
            // Spawn the scrollbar
            scrollbarSprite = NGUITools.AddSprite(mBackground.gameObject, scrollbarAtlas, scrollbarSpriteName);
            scrollbarSprite.depth = NGUITools.CalculateNextDepth(mPanel.gameObject);
            scrollbarSprite.color = scrollbarBgDefColour;
            scrollbarSprite.gameObject.name = "Scrollbar";
            scrollbarSprite.transform.localPosition = new Vector2(scrollbarSprite.transform.localPosition.x + scrollbarSprite.width / 4, scrollbarSprite.transform.localPosition.y);
            UIButtonColor scColour = scrollbarSprite.gameObject.AddComponent<UIButtonColor>();
            scColour.defaultColor = scrollbarSprite.color;
            scColour.hover = scrollbarBgHovColour;
            scColour.pressed = scrollbarBgPrsColour;
            NGUITools.AddWidgetCollider(scrollbarSprite.gameObject);
            // Anchor and position it
            scrollbarSprite.leftAnchor.target = mBackground.transform;
            scrollbarSprite.leftAnchor.relative = 1;
            scrollbarSprite.leftAnchor.absolute = -11; // 10 px minimum scrollbar width
            scrollbarSprite.rightAnchor.target = mBackground.transform;
            scrollbarSprite.rightAnchor.relative = 1;
            scrollbarSprite.rightAnchor.absolute = -1;
            scrollbarSprite.topAnchor.target = mBackground.transform;
            scrollbarSprite.topAnchor.relative = 1;
            scrollbarSprite.topAnchor.absolute = 0;
            scrollbarSprite.bottomAnchor.target = mBackground.transform;
            scrollbarSprite.bottomAnchor.relative = 0;
            scrollbarSprite.bottomAnchor.absolute = 0; // Just like modding Mount&Blade!
            scrollbarSprite.width = scrollbarWidth;
            scrollbarSprite.height = 0;
            scrollbarSprite.ResetAnchors();
            scrollbarSprite.UpdateAnchors();
            // Spawn the foreground
            scrForegroundSprite = NGUITools.AddSprite(scrollbarSprite.gameObject, scrollbarAtlas, scrollbarForegroundName);
            scrForegroundSprite.depth = NGUITools.CalculateNextDepth(mPanel.gameObject);
            scrForegroundSprite.color = scrollbarFgDefColour;
            scrForegroundSprite.gameObject.name = "Foreground";
            UIButtonColor scFColour = scrForegroundSprite.gameObject.AddComponent<UIButtonColor>();
            scFColour.defaultColor = scrForegroundSprite.color;
            scFColour.hover = scrollbarFgHovColour;
            scFColour.pressed = scrollbarFgPrsColour;
            NGUITools.AddWidgetCollider(scrForegroundSprite.gameObject);
            // Fix the foreground's anchors as well
            scrForegroundSprite.leftAnchor.target = scrollbarSprite.transform;
            scrForegroundSprite.rightAnchor.target = scrollbarSprite.transform;
            scrForegroundSprite.topAnchor.target = scrollbarSprite.transform;
            scrForegroundSprite.bottomAnchor.target = scrollbarSprite.transform;
            scrForegroundSprite.ResetAnchors();
            scrForegroundSprite.UpdateAnchors();
            // Add the UIScrollBar component to it so it works
            UIScrollBar scrollBehaviour = scrollbarSprite.gameObject.AddComponent<UIScrollBar>();
            scrollBehaviour.fillDirection = UIProgressBar.FillDirection.TopToBottom;
            scrollBehaviour.backgroundWidget = scrollbarSprite;
            scrollBehaviour.foregroundWidget = scrForegroundSprite;
            mScroller.verticalScrollBar = scrollBehaviour;
            mScroller.ResetPosition();
            scrollBehaviour.value = 0; // does this actually do anything?


        }
        //else OnSelect(false);
    }




}
