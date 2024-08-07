using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PickupWeaponPanel : PanelWithButton
{
    public Button EquipOnPrimary;
    public Button EquipOnSecondary;
    public Button Leave;

    
    TextMeshProUGUI m_ItemNameText;     //武器名   
    GameObject m_WeaponPrefab;          //待拾取的武器物体
   
    Player m_Player;                    //玩家脚本   
    WeaponPickUp m_WeaponPickup;        //拾取武器脚本


    //用于点击按钮函数的枚举
    private enum ButtonAction
    {
        EquipOnPrimary,
        EquipOnSecondary,
        Leave
    }




    #region Unity内部函数
    protected override void Awake()
    {
        base.Awake();

        m_ItemNameText = GetComponentInChildren<TextMeshProUGUI>();
        if (m_ItemNameText == null)
        {
            Debug.LogError("ItemName text is not assigned in the " + name);
            return;
        }


        if (EquipOnPrimary == null || EquipOnSecondary == null || Leave == null)
        {
            Debug.LogError("Some buttons are not assigned in the " + name);
            return;
        }


        //默认按钮为“装备在主武器”按钮（必须放在Awake函数中）
        firstSelectedButton = EquipOnPrimary.gameObject;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        //界面完全淡出后调用此函数
        OnFadeOutFinished += ClosePanel;
        //界面淡出后再设置布尔，防止还没完全淡出玩家就再次按下按钮导致打不开界面
        OnFadeOutFinished += SetIsOpenableForInteractPanel;
    }

    private void Start()
    {
        //将按钮和函数绑定起来
        EquipOnPrimary.onClick.AddListener(() => OnButtonClicked(ButtonAction.EquipOnPrimary));
        EquipOnSecondary.onClick.AddListener(() => OnButtonClicked(ButtonAction.EquipOnSecondary));
        Leave.onClick.AddListener(() => OnButtonClicked(ButtonAction.Leave));  
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        OnFadeOutFinished -= ClosePanel;
        OnFadeOutFinished -= SetIsOpenableForInteractPanel;
    }
    #endregion


    #region 按钮相关
    private void OnButtonClicked(ButtonAction action)
    {
        switch (action)
        {
            case ButtonAction.EquipOnPrimary:
                
                UIManager.Instance.OpenConfirmPanel(() => EquipWeapon(true), this);     //打开确认面板
                
                break;

            case ButtonAction.EquipOnSecondary:
                
                UIManager.Instance.OpenConfirmPanel(() => EquipWeapon(false), this);    //打开确认面板
                
                break;

            case ButtonAction.Leave:

                Fade(CanvasGroup, FadeOutAlpha, FadeDuration, false);              

                break;

            default:
                Debug.Log("No Button is pressed.");
                break;
        }
    }



    private void EquipWeapon(bool isPrimary)
    {
        if (m_Player != null && m_WeaponPrefab != null)
        {
            m_Player.ChangeWeapon(m_WeaponPrefab.name, isPrimary);

            //装备完武器后淡出界面
            Fade(CanvasGroup, FadeOutAlpha, FadeDuration, false);

            //装备完武器后删除地上的武器物体
            DestroyWeaponGameObject();
        }
    }

    private void DestroyWeaponGameObject()
    {
        if (m_WeaponPickup != null)
        {
            //删除地上的武器
            Destroy(m_WeaponPickup.gameObject);
        }
    }
    #endregion


    #region 其余函数
    private void SetIsOpenableForInteractPanel()
    {
        InteractPanel.Instance.SetIsOpenable(true);                 //设置布尔，以便玩家在OnTriggerStay2D中再次打开界面
    }
    #endregion


    #region Setters
    public void SetItemName(string itemName)
    {
        if (m_ItemNameText != null && itemName != null)
        {
            m_ItemNameText.text = itemName;
        }
    }

    public void SetPlayerAndWeapon(Player thisPlayer, GameObject thisWeapon, WeaponPickUp thisWeaponPickup)
    {
        //传递这些脚本用于装备武器和销毁地上的武器
        m_Player = thisPlayer;
        m_WeaponPrefab = thisWeapon;
        m_WeaponPickup = thisWeaponPickup;
    }
    #endregion
}