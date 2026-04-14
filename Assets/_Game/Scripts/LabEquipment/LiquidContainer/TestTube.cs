using Oculus.Interaction;
using UnityEngine;

public class TestTube : MonoBehaviour
{
    private TubeSupportRack currentRack;
    private int currentSlotIndex = -1;

    public float snapDistance = 0.1f;

    public bool isGrabbed = false;

    [SerializeField] private Grabbable grabbable;

    private void Awake()
    {
        grabbable.WhenPointerEventRaised += OnPointerEvent;
    }

    private void OnDestroy()
    {
        grabbable.WhenPointerEventRaised -= OnPointerEvent;
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        switch (evt.Type)
        {
            case PointerEventType.Select:
                HandleGrab();
                break;

            case PointerEventType.Unselect:
                HandleRelease();
                break;
        }
    }
    
    private void Update()
    {
        if (!isGrabbed) return;
        if (currentRack == null) return;
        int nearest = currentRack.GetNearestAvailableSlot(transform.position, snapDistance);
        currentRack.HighlightSlot(nearest);
    }

    public void HandleGrab()
    {
        isGrabbed = true;

        // nếu đang nằm trong rack thì remove
        if (currentRack != null && currentSlotIndex != -1)
        {
            currentRack.RemoveTube(currentSlotIndex);
            // currentRack = null;
            currentSlotIndex = -1;
        }
    }

    public void HandleRelease()
    {
        isGrabbed = false;

        if (currentRack != null)
        {
            currentRack.ClearHighlight();
            int slotIndex = currentRack.GetNearestAvailableSlot(transform.position, snapDistance);
            if (slotIndex != -1)
            {
                SnapToSlot(currentRack, slotIndex);
                return;
            }
        }
        GetComponent<Rigidbody>().isKinematic = false; // nếu không snap được thì cho rơi tự do
    }

    public void SnapToSlot(TubeSupportRack rack, int index)
    {
        Transform slot = rack.slots[index];

        transform.position = slot.position;
        transform.rotation = slot.rotation;

        currentRack = rack;
        currentSlotIndex = index;
        GetComponent<Rigidbody>().isKinematic = true; // tránh bị lệch khi snap
        rack.PlaceTube(this, index);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out TubeSupportRack rack))
        {
            currentRack = rack;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out TubeSupportRack rack))
        {
            if (currentRack == rack)
                currentRack = null;
        }
    }
}