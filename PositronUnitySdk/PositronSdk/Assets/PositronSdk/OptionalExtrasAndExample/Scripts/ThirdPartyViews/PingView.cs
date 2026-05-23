using Positron;
using TMPro;
using UnityEngine;

public class PingView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    private void Awake()
    {
        PositronFacade.PingModel.estimated += OnEstimatedPing;
    }

    private void OnDestroy()
    {
        PositronFacade.PingModel.estimated -= OnEstimatedPing;
    }

    private void OnEstimatedPing()
    {
        _text.text = PositronFacade.PingModel.LatencyMs.ToString() + " ms";
    }
}