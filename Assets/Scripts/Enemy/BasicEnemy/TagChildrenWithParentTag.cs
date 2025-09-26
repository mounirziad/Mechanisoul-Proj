using UnityEngine;

public class TagChildrenWithParentTag : MonoBehaviour
{
    void Start()
    {
        ApplyParentTagToChildren();
    }

    [ContextMenu("Apply Tag to Children")]
    public void ApplyParentTagToChildren()
    {
        string parentTag = gameObject.tag;
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child.gameObject != gameObject) // Don't change the parent's tag
            {
                child.gameObject.tag = parentTag;
            }
        }

        Debug.Log($"Applied '{parentTag}' tag to {allChildren.Length - 1} children");
    }
}