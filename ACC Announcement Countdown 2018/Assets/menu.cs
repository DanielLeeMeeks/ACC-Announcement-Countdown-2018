using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
//using UnityEngine.Windows;

public class menu : MonoBehaviour {

	public TextMeshProUGUI titleTX;

	private updateFromWeb _web;
	private alert printAlert;
	public Button startButton;
	public Image SetStartButton;
	public TMP_Dropdown startTime_month, startTime_hour;
	public TMP_InputField startTime_day, startTime_year, startTime_minute, nameShort, nameFull, countdownNoun;
	public Image background;public Sprite defaultBackground;

	public Canvas[] canvases;

	public TMP_InputField videoPathTX, apiPathTX, showClockTX, endOffsetTX;
	public Toggle showCWFT, showBuildingFundT, showCalenderT, ignoEnd;
	public TMP_Dropdown endActionDD;

	// Use this for initialization
	void Start () {
		Screen.SetResolution(1280, 720, false);
		printAlert = GameObject.Find("updateFromWeb").GetComponent<alert>();
		_web = GameObject.Find("updateFromWeb").GetComponent<updateFromWeb>();

		if (PlayerPrefs.GetString("startTime").Equals(null) ||PlayerPrefs.GetString("startTime").Equals("") || DateTime.Parse(PlayerPrefs.GetString("startTime"))< DateTime.Now){menuStartTime();}
		else{menuStartTime(DateTime.Parse(PlayerPrefs.GetString("startTime")));}

		if (PlayerPrefs.GetString("firstLoad")!= "false"){firstLoad();}else{loadSettings();}

		StartCoroutine( loadBackground() );
		titleTX.text = PlayerPrefs.GetString("nameShort") + " Announcement Countdown 2018";
		UpdateWeb();
		startButton.interactable = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator loadBackground(){

		string path=System.IO.Path.Combine( System.IO.Path.Combine( PlayerPrefs.GetString("videoPath"), "Backgrounds"), "background.png");
		if (System.IO.File.Exists(path)){
			WWW www = new WWW(path);
			yield return www;
			background.sprite = Sprite.Create(www.texture, new Rect(0,0,www.texture.width, www.texture.height), new Vector2 (0,0));
		}else{background.sprite = defaultBackground;printAlert.Error("Background could not be found.  Go to \"Setting\" and check video path");}

	}

	public void ChangeCanvas(Canvas c){
		foreach (Canvas k in canvases){
			k.enabled = false;
		}
		c.enabled = true;
	}

	private void firstLoad(){

		PlayerPrefs.SetString("videoPath", "C:\\Users\\USERNAME\\Google Drive\\AnnouncementVideos");
		PlayerPrefs.SetString("apiURL", "http://dlm-movies.com/test/acc/api.php");
		PlayerPrefs.SetInt("showClock", 180);
		PlayerPrefs.SetInt("videoOffset", 15);
		PlayerPrefs.SetInt("showCWF", 1);
		PlayerPrefs.SetInt("showBuildingFund", 1);
		PlayerPrefs.SetInt("showCalender", 1);
		PlayerPrefs.SetString("endAction", "endlessClock");
		PlayerPrefs.SetInt("ignoreEnd", 0);
		PlayerPrefs.SetString("countdownNoun", "Church");
		PlayerPrefs.SetString("nameShort", "ACC");
		PlayerPrefs.SetString("nameFull", "Arthur Christian Church");

		printAlert.Warring("First time loading.  Check settings.");

		loadSettings();

		PlayerPrefs.SetString("firstLoad", "false");

	}
	private void loadSettings(){

		videoPathTX.text = PlayerPrefs.GetString("videoPath");
		apiPathTX.text = PlayerPrefs.GetString("apiURL");
		showClockTX.text = PlayerPrefs.GetInt("showClock").ToString();
		endOffsetTX.text = PlayerPrefs.GetInt("videoOffset").ToString();
		countdownNoun.text = PlayerPrefs.GetString("countdownNoun");
		nameShort.text = PlayerPrefs.GetString("nameShort");
		nameFull.text = PlayerPrefs.GetString("nameFull");
		if (PlayerPrefs.GetInt("showCWF")==1){showCWFT.isOn = true;}else{showCWFT.isOn=false;}
		if (PlayerPrefs.GetInt("showBuildingFund")==1){showBuildingFundT.isOn = true;}else{showBuildingFundT.isOn=false;}
		if (PlayerPrefs.GetInt("showCalender")==1){showCalenderT.isOn = true;}else{showCalenderT.isOn=false;}
		if (PlayerPrefs.GetInt("ignoreEnd")==1){ignoEnd.isOn = true;}else{ignoEnd.isOn=false;}
		if (PlayerPrefs.GetString("endAction")=="endlessClock"){endActionDD.value=0;}else if (PlayerPrefs.GetString("endAction")=="endlessSlides"){endActionDD.value=1;}else{endActionDD.value=2;}

	}

	public void setVideoPath(){PlayerPrefs.SetString("videoPath", videoPathTX.text);}
	public void setAPI(){PlayerPrefs.SetString("apiURL", apiPathTX.text);_web.SetAPI();}
	public void setShowClock(){PlayerPrefs.SetInt("showClock", int.Parse(showClockTX.text));}
	public void setEndVideoOffset(){PlayerPrefs.SetInt("videoOffset", int.Parse(endOffsetTX.text));}
	public void setShowCWF(){ if (showCWFT.isOn){PlayerPrefs.SetInt("showCWF", 1);}else{PlayerPrefs.SetInt("showCWF", 0);} }
	public void setShowBuildingFund(){ if(showBuildingFundT.isOn){PlayerPrefs.SetInt("showBuildingFund", 1);}else{PlayerPrefs.SetInt("showBuildingFund", 0);} }
	public void setShowCalender(){ if(showCalenderT.isOn){PlayerPrefs.SetInt("showCalender", 1);}else{PlayerPrefs.SetInt("showCalender", 0);} }
	public void setEndAction(){if (endActionDD.value == 0){PlayerPrefs.SetString("endAction", "endlessClock");}else if(endActionDD.value == 1){PlayerPrefs.SetString("endAction", "endlessSlides");}else{PlayerPrefs.SetString("endAction", "autoClose");} }
	public void setIgnore(){ if(ignoEnd.isOn){PlayerPrefs.SetInt("ignoreEnd", 1);}else{PlayerPrefs.SetInt("ignoreEnd", 0);} }
	public void setNoun(){PlayerPrefs.SetString("countdownNoun", countdownNoun.text);}
	public void setNameFull(){PlayerPrefs.SetString("nameFull", nameFull.text);}
	public void setNameShort(){PlayerPrefs.SetString("nameShort", nameShort.text);}

	private void menuStartTime(){
		printAlert.Warring("Start time has already passed "+PlayerPrefs.GetString("startTime")+".  The program will try and guess the next start date.");
		int year, month, day, hour, minute;
		bool today=true;Debug.Log(DateTime.Now.Hour);
		if (DateTime.Now > new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 00, 00)){
			today = false;
			hour = 8;
			minute = 30;
		}else if (DateTime.Now > new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 00, 00)){
			hour = 19;
			minute = 0;
		}else if (DateTime.Now > new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 11, 00, 00)){
			hour = 18;
			minute = 0;
		}else if (DateTime.Now > new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 10, 30, 00)){
			hour = 11;
			minute = 0;
		}
        else if (DateTime.Now > new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 30, 00))
        {
            hour = 10;
            minute = 30;
        }
        else if(DateTime.Now > new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 30, 00)){
			hour = 9;
			minute = 30;
		}else {
			hour = 8;
			minute = 30;
		}

		if (today){year = DateTime.Now.Year;day=DateTime.Now.Day;month=DateTime.Now.Month;}
		else{year = DateTime.Now.AddDays(1).Year;day=DateTime.Now.AddDays(1).Day;month=DateTime.Now.AddDays(1).Month;}

		menuStartTime(new DateTime(year, month, day, hour,minute, 0));
	}

	public void changeStart(bool b){
		if (b){SetStartButton.color = Color.green;}
		else{SetStartButton.color =Color.red;}
	}

	private void menuStartTime(DateTime d){
		Debug.Log(d.ToString("G"));
		startTime_year.text = d.Year.ToString();
		startTime_month.value = d.Month-1;
		startTime_day.text = d.Day.ToString();
		startTime_hour.value = d.Hour;
		startTime_minute.text = d.Minute.ToString();
		PlayerPrefs.SetString("startTime", d.ToString("yy-MM-dd HH:mm:ss"));
	}

	public void SetStartTime(){
		string s = startTime_year.text +"-"+ leadingFormat(startTime_month.value+1)+"-"+leadingFormat(int.Parse(startTime_day.text))+" "+leadingFormat(startTime_hour.value)+":"+leadingFormat(int.Parse(startTime_minute.text))+":00";
		PlayerPrefs.SetString("startTime", s);
		printAlert.Log("Start Time changed to " + s);
		changeStart(true);
	}

	private string leadingFormat(int i){
		if (i < 10){return "0"+i;}
		else{return i.ToString();}
	}

	public void StartSlideshow(){Application.LoadLevel(1);}

	public void quit(){Application.Quit();}

	public void UpdateWeb(){
		_web.DownloadEvents();
		_web.DownloadOtherData();
	}
}
