using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabManager : MonoBehaviour
{
    public static TabManager Instance { get; private set; }

    public List<GameObject> tabPanels;
    public List<Button> tabButtons;

    public float defaultHeight = 240f;
    public float activeHeight = 320f;

    private int currentTabIndex = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OpenTab(int index)
    {
        if (index == currentTabIndex)
            return;
        
        for (int i = 0; i < tabPanels.Count; i++)
            tabPanels[i].SetActive(i == index);

        for (int i = 0; i < tabButtons.Count; i++)
        {
            LayoutElement le = tabButtons[i].GetComponent<LayoutElement>();
            if (le != null)
                le.preferredHeight = (i == index) ? activeHeight : defaultHeight;
        }

        currentTabIndex = index;
    }
}
