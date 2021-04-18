using UnityEngine;

public class PopupTooltipManager : MonoBehaviour
{
    public PopupTooltip copyLevelKeySuccessful;
    public PopupTooltip pasteLevelKeySuccessful;
    public PopupTooltip pasteLevelKeyFailed;

    private static PopupTooltipManager instance;

    public static PopupTooltipManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }
}
