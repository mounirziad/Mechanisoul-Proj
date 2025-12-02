using UnityEngine;

public class GiveStartUpgradeChip : MonoBehaviour
{
    bool hasGivenUpgradeChip = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasGivenUpgradeChip)
        {
            XPManager.Instance.SetSkillPoints(1);
            hasGivenUpgradeChip = true;
        }
    }
}
