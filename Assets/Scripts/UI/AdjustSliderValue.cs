using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class AdjustSliderValue : MonoBehaviour
{
    private TextMeshProUGUI textMeshProUGUI;
    [SerializeField]
    private RectTransform handle;
    [SerializeField]
    private float minSliderValue = 10f, maxSliderValue = 500f;
    // Start is called before the first frame update
    void Start()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        textMeshProUGUI.text =  Mathf.Round(handle.anchorMin.x * (maxSliderValue - minSliderValue) + minSliderValue).ToString();
    }
}
