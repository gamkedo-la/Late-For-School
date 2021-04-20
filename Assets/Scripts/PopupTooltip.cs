using UnityEngine;
using UnityEngine.UI;

public class PopupTooltip : MonoBehaviour
{
    public GameObject prefab;
    public string text;
    public int fontSize;
    public Vector3 position;
    public Transform parent;
    public float fadeTime;
    public Vector3 direction;
    public float moveSpeed;
    public float textWidth;
    public float endScaleFactor;
    public Color textColor;

    private GameObject tooltip;
    private float timeLeft;
    private Vector3 startScale;

    public void Activate()
    {
        if (tooltip != null)
        {
            Destroy(tooltip);
        }
        tooltip = Instantiate(prefab, position, Quaternion.identity, parent);
        Text text = tooltip.GetComponentInChildren<Text>();
        text.text = this.text;
        text.fontSize = fontSize;
        text.color = textColor;

        RectTransform rt = text.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);

        timeLeft = fadeTime;
        startScale = tooltip.transform.localScale;
    }

    private void OnEnable()
    {
        // if enabled, this means it was disabled, so we need to make sure any 
        // in progress tooltip is destroyed

        if (tooltip != null)
        {
            Destroy(tooltip);
        }
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0 && tooltip != null)
        {
            Destroy(tooltip);
        }
        else if (tooltip != null)
        {
            // move pos
            Vector3 position = tooltip.transform.position;
            position += direction.normalized * moveSpeed * Time.deltaTime;
            tooltip.transform.position = position;

            // fade text
            Text text = tooltip.GetComponentInChildren<Text>();
            Color color = text.color;
            color.a = timeLeft / fadeTime;
            text.color = color;

            // scale text
            float sf = 1 - (1 - endScaleFactor) * (1 - timeLeft / fadeTime);
            tooltip.transform.localScale = new Vector3(startScale.x * sf, startScale.y * sf, startScale.z);
        }
    }
}
