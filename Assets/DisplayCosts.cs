using TMPro;
using UnityEngine;

public class DisplayCosts : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI GCost=null;
    [SerializeField] private TextMeshProUGUI HCost=null;
    [SerializeField] private TextMeshProUGUI FCost=null;

    public void SetGCost(string gCost)
    {
        GCost.text = gCost;
    }

    public void SetHCost(string hCost)
    {
        HCost.text = hCost;
    }

    public void SetFCost(string fCost)
    {
        FCost.text = fCost;
    }

    public void ClearText()
    {
        GCost.text = "";
        HCost.text = "";
        FCost.text = "";
    }
}