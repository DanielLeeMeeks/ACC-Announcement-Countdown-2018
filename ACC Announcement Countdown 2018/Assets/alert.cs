using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class alert : MonoBehaviour {

	List<string> alerts = new List<string>();
	int removeRate = 10;
	public TextMeshProUGUI alertTX;

	void Start(){
		InvokeRepeating("updateAlert", 1, 1);
	}

	public void Error(string message){updateAlert("<sprite=0> "+message);}
	public void Warring(string message){updateAlert("<sprite=1> "+message);}
	public void Remote(string message){updateAlert("<sprite=2> "+message);}
	public void Log(string message){updateAlert("<sprite=3> "+message);}

	public void alertClear(){alerts.Clear();alertTX.text = "";}

	private void updateAlert(string message){
		alerts.Add(message);
		removeRate = 10;
		string ns = "";
		foreach (string s in alerts){
			ns = ns + s + "\n";
		}
		alertTX.text = ns;
	}
	private void updateAlert(){
		if (removeRate < 0){
			if (alerts.Count != 0){
				alerts.RemoveAt(0);
				string ns = "";
				foreach (string s in alerts){
					ns = ns + s + "\n";
				}
				alertTX.text = ns;
			}
		}
		removeRate-=1;
	}

}
