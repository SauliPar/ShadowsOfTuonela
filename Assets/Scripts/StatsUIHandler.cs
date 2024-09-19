using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUIHandler : MonoBehaviour
{
    public PlayerStatistics PlayerStatistics;
    
    [Header("STR components")]
    [SerializeField] private Button strMinusButton;
    [SerializeField] private Button strPlusButton;
    [SerializeField] private TextMeshProUGUI strValue;
    
    [Header("ATT components")]
    [SerializeField] private Button attMinusButton;
    [SerializeField] private Button attPlusButton;
    [SerializeField] private TextMeshProUGUI attValue;
    
    [Header("DEF components")]
    [SerializeField] private Button defMinusButton;
    [SerializeField] private Button defPlusButton;
    [SerializeField] private TextMeshProUGUI defValue;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Initialize()
    {
        Debug.Log("initialized");
        // STR stuffs
        strMinusButton.onClick.AddListener(SubtractStrengthAndUpdateUI);
        strPlusButton.onClick.AddListener(AddStrengthAndUpdateUI);
        
        // ATT stuffs
        attMinusButton.onClick.AddListener(SubtractAttackAndUpdateUI);
        attPlusButton.onClick.AddListener(AddAttackAndUpdateUI);
        
        // DEF stuffs
        defMinusButton.onClick.AddListener(SubtractDefenseAndUpdateUI);
        defPlusButton.onClick.AddListener(AddDefenseAndUpdateUI);
        
        strValue.text = PlayerStatistics.Strength.ToString();
        attValue.text = PlayerStatistics.Attack.ToString();
        defValue.text = PlayerStatistics.Defense.ToString();
    }
    
    void SubtractStrengthAndUpdateUI()
    {
        strValue.text = PlayerStatistics.SubtractStrength().ToString();
    }
    void AddStrengthAndUpdateUI()
    {
        strValue.text = PlayerStatistics.AddStrength().ToString();
    }
    void SubtractAttackAndUpdateUI()
    {
        attValue.text = PlayerStatistics.SubtractAttack().ToString();
    }
    void AddAttackAndUpdateUI()
    {
        attValue.text = PlayerStatistics.AddAttack().ToString();
    }
    void SubtractDefenseAndUpdateUI()
    {
        defValue.text = PlayerStatistics.SubtractDefense().ToString();
    }
    void AddDefenseAndUpdateUI()
    {
        defValue.text = PlayerStatistics.AddDefense().ToString();
    }
}
