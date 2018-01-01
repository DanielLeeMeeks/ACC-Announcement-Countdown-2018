using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class clock : MonoBehaviour {

	public TextMeshProUGUI date;

	// Use this for initialization
	void Start () {
		date.text = DateTime.Now.ToString("MMMMM d, yyyy");
		InvokeRepeating("UpdateTime", 5f, 5f);
	}
	

	void UpdateTime () {
		this.GetComponent<TextMeshProUGUI>().text = DateTime.Now.ToString("hh:mm");
	}
}
