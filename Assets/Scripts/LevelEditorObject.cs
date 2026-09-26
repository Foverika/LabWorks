using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class LevelEditorObject : MonoBehaviour
{
    public string objectType;
    public string customName;

    private Renderer rend;
    private Color originalColor;
    private int overlapCount = 0;

    public bool HasCollision => overlapCount > 0;
    private bool isSelected = false;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        if (rend != null && rend.material != null)
        {
            originalColor = rend.material.color;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateColor();
    }

    public void UpdateColor()
    {
        if (rend == null) return;

        if (HasCollision)
            rend.material.color = Color.red;
        else if (isSelected)
            rend.material.color = Color.yellow;
        else
            rend.material.color = originalColor;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<LevelEditorObject>() != null)
        {
            overlapCount++;
            UpdateColor();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<LevelEditorObject>() != null)
        {
            overlapCount = Mathf.Max(0, overlapCount - 1);
            UpdateColor();
        }
    }
}