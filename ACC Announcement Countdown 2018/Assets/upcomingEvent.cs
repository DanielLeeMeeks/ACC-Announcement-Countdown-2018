using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class upcomingEvent{

	private DateTime eventDate;
	private string id;
	private string eventTitle;
	private string description;
	private string backgroundURL;
	public MovieTexture background;

	public string GetBackgroundURL(){return this.backgroundURL;}
	public bool IsAutoSlide(){return !(backgroundURL.Equals("") || backgroundURL.Equals(null));}
	public void SetTitle(string eventTitle){this.eventTitle = eventTitle;}
	public void SetDate(DateTime eventDate){this.eventDate = eventDate;}
	public void SetDate(string eventDate){SetDate(DateTime.ParseExact(eventDate, "yyyy-MM-dd HH:mm", null));}
	public void SetDescription(string description){this.description = description;}

	public string GetID(){return id;}
	public string GetDescription(){return description;}
	public string GetTitle(){return eventTitle;}
	public string GetDateFormated(){return eventDate.DayOfWeek.ToString() + ", " + eventDate.ToString("M");}
	public DateTime GetDate(){return eventDate;}
	public string GetTime(){return eventDate.ToString("t");}

	override public string ToString(){return eventTitle + " on " + this.GetDateFormated() + " @ " + this.GetTime();}

	public upcomingEvent (string id, string eventTitle, DateTime eventDate){
		new upcomingEvent(id, eventTitle, eventDate, "This event has no description", "");
	}

	public upcomingEvent (string id, string eventTitle, DateTime eventDate, string description){
		new upcomingEvent(id, eventTitle, eventDate, description, "");
	}

	public upcomingEvent (string id, string eventTitle, DateTime eventDate, string description, string backgroundURL){
		this.id = id;
		this.eventTitle = eventTitle.Replace("\\", "");
		this.description = description;
		this.eventDate = eventDate;
		this.backgroundURL = backgroundURL;

		if (eventDate < DateTime.Now.AddHours(1)){backgroundURL = "";}

		if (!(backgroundURL == null || backgroundURL == "")){createBackground();}
	}

	private void createBackground(){
		WWW myMovie = new WWW ("file://"+backgroundURL);
		//yield return myMovie;
		background = myMovie.GetMovieTexture();
	}

	public static void weed(List<upcomingEvent> listToWeed){
		List<upcomingEvent> weededList = new List<upcomingEvent>();
		foreach (upcomingEvent e in listToWeed){
			if (e.GetDate() < DateTime.Now.AddDays(12) || e.IsAutoSlide()){
				weededList.Add(e);
			}
		}
		listToWeed.Clear();
		foreach (upcomingEvent e in weededList){listToWeed.Add(e);}
	}
}
