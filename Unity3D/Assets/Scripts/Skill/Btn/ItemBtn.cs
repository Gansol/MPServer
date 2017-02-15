using UnityEngine;
using System.Collections;

public class ItemBtn : MonoBehaviour
{

    private string itemID;
    private short skillType;
    private float lerpSpeed = 8f;
    private float _lerpSpeed = 0.1f;
    private float _upDistance = 30f;
    private float _energyValue = 0.2f;

    public void init(string itemID,short skillType, float lerpSpeed, float upDistance, float energyValue)
    {
        this.itemID = itemID;
        this.skillType = skillType;
        this._lerpSpeed = lerpSpeed;
        this._upDistance = upDistance;
        this._energyValue = energyValue;
    }

    //void OnEnable()
    //{
    //    EventDelegate.Set(GetComponent<UIButton>().onClick, GetComponent<ItemBtn>().OnClick);
    //}

    void Start()
    {

    }

    public void OnClick()
    {
        if (enabled)
        {
            Global.photonService.SendSkillItem(System.Convert.ToInt16(itemID), skillType);
            gameObject.transform.parent.GetChild(0).gameObject.SetActive(true);
            // 如果道具不足 不切換 不顯示
            gameObject.SetActive(false);
        }

    }
}
