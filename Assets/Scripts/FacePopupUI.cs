    using UnityEngine;
    using System.Collections.Generic;

    public class FacePopupUI : MonoBehaviour
    {
        public Transform slotParent;         // 슬롯 붙을 부모 (그리드 레이아웃)
        public GameObject faceSlotPrefab;    // FaceSlotUI 프리팹

        private List<FaceSlotUI> slotList = new List<FaceSlotUI>();

        private void OnEnable()
        {
            RefreshSlots();
        }

        public void RefreshSlots()
        {
            ClearSlots();

            var faceItems = ItemManager.Instance.GetItemsByType(ItemData.ItemType.Face);
            foreach (var item in faceItems)
            {
                GameObject go = Instantiate(faceSlotPrefab, slotParent);
                FaceSlotUI slot = go.GetComponent<FaceSlotUI>();
                slot.Initialize(item);
                slotList.Add(slot);
            }
        }

        void ClearSlots()
        {
            foreach (var slot in slotList)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            slotList.Clear();
        }

        public void ClosePopup()
        {
            gameObject.SetActive(false);
        }
    }
