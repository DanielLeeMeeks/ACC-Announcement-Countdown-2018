using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using System;

public class calender : MonoBehaviour {

	private DateTime sunday = DateTime.Now;
	public TextMeshProUGUI [] dates, boxs;

	public TextMeshProUGUI titleTX, descriptionTX, monthTX, dayTX, timeTX;

	// Use this for initialization
	void Start () {
		//ResetCalender();
	}

	public void ResetCalender(upcomingEvent [] events){

		while(sunday.DayOfWeek != DayOfWeek.Sunday) {
			sunday = sunday.Add(new TimeSpan(-1, 0, 0, 0));
		}

		int i = 0;
		while (i < 7){
			dates[i].text = sunday.AddDays(i).Day.ToString();

			string cal = "";
			foreach (upcomingEvent e in events){
				if (e.GetDate().Month == sunday.AddDays(i).Month && e.GetDate().Day == sunday.AddDays(i).Day){
					cal += "<b>"+ e.GetTime() + "</b> " + e.GetTitle() + "\n";
				}
			}
			boxs[i].text = cal;

			i++;
		}

	}

	public void displayEvent(upcomingEvent e){
		titleTX.text = e.GetTitle();
		descriptionTX.text = e.GetDescription();
		monthTX.text = e.GetDate().ToString("MMMMM");
		dayTX.text = e.GetDate().ToString("dd");
		timeTX.text = "@" + e.GetTime();
	}

}
