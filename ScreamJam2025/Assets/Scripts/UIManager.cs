using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Slider drunkennessBar;
    [SerializeField] private Slider balanceBar;
    [SerializeField] private Image drunkennessFill;
    [SerializeField] private TextMeshProUGUI candyText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
        
        balanceBar.minValue = player.baseX - (player.wobbleAmplitude * player.fallingThreshold);
        balanceBar.maxValue = player.baseX + (player.wobbleAmplitude * player.fallingThreshold);
        balanceBar.value = player.baseX;

        candyText.text = $"Candy: {player.candyCount}";
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDrunkenness();
        UpdateCandy();
        UpdateBalance();
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

    private void UpdateBalance()
    {
        balanceBar.value = player.transform.position.x;
    }

    public void ResetBalanceBar()
    {
        balanceBar.minValue = player.baseX - (player.wobbleAmplitude * player.fallingThreshold);
        balanceBar.maxValue = player.baseX + (player.wobbleAmplitude * player.fallingThreshold);
        balanceBar.value = player.baseX;
    }
}
