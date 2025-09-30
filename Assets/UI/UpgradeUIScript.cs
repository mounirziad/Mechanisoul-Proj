using GLTFast.Schema;
using UnityEngine;
using UnityEngine.UIElements;

public class UpgradeUIScript : MonoBehaviour
{
    private VisualElement root;

    private Button angerMelee1;
    private Button joyRange1;
    private Button sadDash1;
    private Button angerDash1;
    private Button angerMelee2;
    private Button joyRange2;
    private Button sadDash2;
    private Button angerDash2;
    private Button upgradeTab;
    private Button comboTab;
    private VisualElement skillTree;
    private VisualElement combos;

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        angerMelee1 = root.Q<Button>("AngerMelee1");
        joyRange1 = root.Q<Button>("JoyRange1");
        sadDash1 = root.Q<Button>("SadDash1");
        angerDash1 = root.Q<Button>("AngerDash1");

        angerMelee2 = root.Q<Button>("AngerMelee2");
        joyRange2 = root.Q<Button>("JoyRange2");
        sadDash2 = root.Q<Button>("SadDash2");
        angerDash2 = root.Q<Button>("AngerDash2");

        upgradeTab = root.Q<Button>("UpgradeTab");
        comboTab = root.Q<Button>("ComboTab");

        skillTree = root.Q<VisualElement>("SkillTreeEL");
        combos = root.Q<VisualElement>("CombosEL");

        angerMelee1.clicked += OnAngerMelee1Clicked;
        joyRange1.clicked += OnJoyRange1Clicked;
        sadDash1.clicked += OnSadDash1Clicked;
        angerDash1.clicked += OnAngerDash1Clicked;

        angerMelee2.clicked += OnAngerMelee2Clicked;
        joyRange2.clicked += OnJoyRange2Clicked;
        sadDash2.clicked += OnSadDash2Clicked;
        angerDash2.clicked += OnAngerDash2Clicked;

        skillTree.style.display = DisplayStyle.Flex;
        combos.style.display = DisplayStyle.None;

        upgradeTab.clicked += () =>
        {
            skillTree.style.display = DisplayStyle.Flex;
            combos.style.display = DisplayStyle.None;
        };
        comboTab.clicked += () =>
        {
            skillTree.style.display = DisplayStyle.None;
            combos.style.display = DisplayStyle.Flex;
        };
    }

    void OnDisable()
    {
        angerMelee1.clicked -= OnAngerMelee1Clicked;
        joyRange1.clicked -= OnJoyRange1Clicked;
        sadDash1.clicked -= OnSadDash1Clicked;
        angerDash1.clicked -= OnAngerDash1Clicked;

        angerMelee2.clicked -= OnAngerMelee2Clicked;
        joyRange2.clicked -= OnJoyRange2Clicked;
        sadDash2.clicked -= OnSadDash2Clicked;
        angerDash2.clicked -= OnAngerDash2Clicked;
    }

    private void OnAngerMelee1Clicked()
    {
        Debug.Log("Anger Melee Upgrade 1 Clicked!");
    }

    private void OnJoyRange1Clicked()
    {
        Debug.Log("Joy Range Upgrade 1 Clicked!");
    }

    private void OnSadDash1Clicked()
    {
        Debug.Log("Sad Dash Upgrade 1 Clicked!");
    }

    private void OnAngerDash1Clicked()
    {
        Debug.Log("Anger Dash Upgrade 1 Clicked!");
    }
    private void OnAngerMelee2Clicked()
    {
        Debug.Log("Anger Melee Upgrade 2 Clicked!");
    }

    private void OnJoyRange2Clicked()
    {
        Debug.Log("Joy Range Upgrade 2 Clicked!");
    }

    private void OnSadDash2Clicked()
    {
        Debug.Log("Sad Dash Upgrade 2 Clicked!");
    }

    private void OnAngerDash2Clicked()
    {
        Debug.Log("Anger Dash Upgrade 2 Clicked!");
    }
}