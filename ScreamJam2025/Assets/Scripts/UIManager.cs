using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Slider drunkennessBar;
    [SerializeField] private Image drunkennessFill;
    [SerializeField] private TextMeshProUGUI candyText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }
        drunkennessBar.minValue = 0;
        drunkennessBar.maxValue = player.maxDrunkenness;
        drunkennessBar.value = 0;

        candyText.text = $"Candy: {player.candyCount}";
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDrunkenness();
        UpdateCandy();
    }

    private void UpdateDrunkenness()
    {
        drunkennessBar.value = player.drunkennessMeter;
        float progress = player.drunkennessMeter / player.maxDrunkenness;
        
        if (progress > 0.67)
        {
            drunkennessFill.color = Color.red;
        } else if (progress > 0.33) {
            drunkennessFill.color = Color.orange;
        } else
        {
            drunkennessFill.color = Color.green;
        }
    }

    private void UpdateCandy()
    {
        candyText.text = $"Candy: {player.candyCount}";
    }
}
