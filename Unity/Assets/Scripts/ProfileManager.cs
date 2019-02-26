using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour {

    public Dictionary<string, GameObject> allTabs = new Dictionary<string, GameObject>();

	// Use this for initialization
	void Start ()
    {
        var TabsHolder = transform.Find("Tabs");
        foreach (Transform tab in TabsHolder)
        {
            allTabs.Add(tab.name, tab.gameObject);
        }
    }
	
	public void OpenTab(string tabname)
    {
        foreach (var item in allTabs)
            item.Value.SetActive(item.Key == tabname);
    }
}
