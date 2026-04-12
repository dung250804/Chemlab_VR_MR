using System.Collections.Generic;
using UnityEngine;

public class TubeSupportRack : MonoBehaviour
{
    public List<Transform> slots = new List<Transform>();
    public List<TestTube> occupiedSlots = new List<TestTube>();
    public TestTube testTube;
    private int currentHighlight = -1;
    private void Awake()
    {
        occupiedSlots = new List<TestTube>(new TestTube[slots.Count]);
        SpawnAll(testTube);
    }

    public int GetNearestAvailableSlot(Vector3 position, float maxDistance)
    {
        int bestIndex = -1;
        float bestDist = maxDistance;

        for (int i = 0; i < slots.Count; i++)
        {
            if (occupiedSlots[i] != null) continue;

            float dist = Vector3.Distance(position, slots[i].position);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public void PlaceTube(TestTube tube, int index)
    {
        occupiedSlots[index] = tube;
    }

    public void RemoveTube(int index)
    {
        occupiedSlots[index] = null;
    }

    public void HighlightSlot(int index)
    {
        // reset slot cũ
        if (currentHighlight != -1 && currentHighlight < slots.Count)
        {
            slots[currentHighlight].gameObject.SetActive(false);
        }

        currentHighlight = index;

        // set slot mới
        if (index != -1 && index < slots.Count)
        {
            slots[index].gameObject.SetActive(true);
        }
    }

    public void ClearHighlight()
    {
        HighlightSlot(-1);
    }

    public void SpawnAll(TestTube prefab)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (occupiedSlots[i] == null)
            {
                SpawnTubeAtSlot(prefab, i);
            }
            slots[i].gameObject.SetActive(false);
        }
    }

    public void SpawnTubeAtSlot(TestTube prefab, int index)
    {
        if (index < 0 || index >= slots.Count) return;
        if (occupiedSlots[index] != null) return;

        Transform slot = slots[index];

        TestTube tube = Instantiate(prefab, slot.position, slot.rotation);

        // Nếu có parent (rack) thì set cho gọn hierarchy
        tube.transform.SetParent(this.transform);
        tube.SnapToSlot(this, index);
        occupiedSlots[index] = tube;
    }
}