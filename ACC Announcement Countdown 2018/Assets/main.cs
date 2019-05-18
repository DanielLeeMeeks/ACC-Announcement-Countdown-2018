using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; using UnityEngine.UI;
using System.IO;
using System;

public class main : MonoBehaviour {

	private bool bottomToggle;
	public Canvas menu;
	public Image endlessClockBackground;

	private updateFromWeb updater;
	private alert printAlert;
	public RawImage screen;
	public AudioSource audioPlayer;
	public calender _calender;

	public Sprite defaultBackground;
    public MovieTexture defaultSlide;
    public MovieTexture defaultBackgroundVideo;

	public TextMeshProUGUI buildingFundTX, cmfTX, cwfTX, countdownAg, countdownNum, welcomeTX, calTitleTX;
	public Slider buildingFundSlider;
	public TextMeshProUGUI [] autoSlideTX;
	public string countdownAdjective;

	private List<upcomingEvent> calender = new List<upcomingEvent>();
	//private MovieTexture endVideo;
	private FileInfo [] videos, slide, countdownBackgrounds, endVideo;
    private FileInfo clockBackground, buildingBackground, cmfBackground;

	public string videoURL = "";
    public string streamingPath;

	private DateTime startTime, videoStartTime;
	public string startTimeString;//2017-11-17 22:30:00
	private bool [] counddownUsed  = new bool[11];
	private int endVideoOffsetInSecounds=15;
	private bool ignoreEndTimer = false;

	public bool showBuildingFund, showCWFCMF, showCal, showCalOld;
	public string buildingFundTotal, cwfDate, cmfDate;

	public string endActionString;
	public enum steps{slides, smartSlides, autoSlides, videos, end};
	public steps currentStep;
	public int step;

	public Animator textOverLayTrigger;
	public GameObject eventObj, calenderParent;

	//Clock
	private float clockTimer = 0;
	private float showClockEveryXSecounds=180;

	// Use this for initialization
	void Start () {
        streamingPath = Application.streamingAssetsPath;
        updater = GameObject.Find("updateFromWeb").GetComponent<updateFromWeb>();
        printAlert = updater.GetComponent<alert>();

            videoURL = PlayerPrefs.GetString("videoPath");
            showClockEveryXSecounds = PlayerPrefs.GetInt("showClock");
            endVideoOffsetInSecounds = PlayerPrefs.GetInt("videoOffset");

        if (PlayerPrefs.GetInt("showCWF") == 1){showCWFCMF = true;}else{showCWFCMF = false;}
		if (PlayerPrefs.GetInt("showBuildingFund") == 1){showBuildingFund = true;}else{showBuildingFund = false;}
		if (PlayerPrefs.GetInt("showCalender") == 1){showCal = true;}else{showCal = false;}
		if (PlayerPrefs.GetInt("ignoreEnd")==1){ignoreEndTimer = true;}else{ignoreEndTimer = false;}
		endActionString = PlayerPrefs.GetString("endAction");
		countdownAdjective = PlayerPrefs.GetString("countdownNoun");

        welcomeTX.text = PlayerPrefs.GetString("nameFull");
		calTitleTX.text = "This Week @ " + PlayerPrefs.GetString("nameShort");

		Screen.SetResolution(1920, 1080, true);
		audioPlayer = this.GetComponent<AudioSource>();
		startTimeString = PlayerPrefs.GetString("startTime");
		SetNewStartTime(startTimeString);
		currentStep = steps.slides;

        LoadEvents(false);

        LoadOtherOnlineData(false);
        LoadVideoInfo();


        StartCoroutine(loadBackground()); 

StartCoroutine(waitF());

		if (startTime < DateTime.Now){printAlert.Error("Start time has already pasted.  End action starting...");}

	}

	public IEnumerator waitF(){
	yield return new WaitForSeconds(1);
	logic();
	}

	// Update is called once per frame
	void Update () {
		clockTimer -= Time.deltaTime;

		if (Input.GetKeyDown(KeyCode.BackQuote)){menu.enabled = !menu.enabled;}

	}

	public void SetIgnoreEndTimer(bool b){ignoreEndTimer = b;}

	void logic(){
        Debug.Log("logic");
		int countdown = CountdownCalculator();
        Debug.Log(countdown);
		//if (!ignoreEndTimer && videoStartTime < DateTime.Now){currentStep = steps.end;step=0;}

		if(videoStartTime < DateTime.Now && !ignoreEndTimer){
			ignoreEndTimer = true;
			StartEnd();
		}else if (clockTimer < 0){
			PlayVideo(clockBackground, "clock", false);
            clockTimer = showClockEveryXSecounds;
		}else if (countdown > 0){
			countdownNum.text = countdown.ToString();
			PlayVideo(countdownBackgrounds[UnityEngine.Random.Range(0, countdownBackgrounds.Length)], "countdown", false);
		}else if (currentStep == steps.slides && step < slide.Length){
			PlayVideo(slide[step], "hide", false);
			step++;
		}else if (currentStep ==  steps.slides && step >= slide.Length){
			currentStep = steps.autoSlides;
			step = 0;
			logic();
		}else if (currentStep == steps.autoSlides && step < calender.Count){
			if (calender[step].IsAutoSlide() && calender[step].background != null){
				textOverLayTrigger.SetTrigger("autoSlide");
				autoSlideTX[0].text = calender[step].GetTitle();
				autoSlideTX[1].text = calender[step].GetDate().ToString("M") + "\n@ " + calender[step].GetTime();
				autoSlideTX[2].text = calender[step].GetDescription();
				PlayVideo(calender[step].background, "autoSlide", false);
				textOverLayTrigger.SetTrigger("hide");
				step++;
			}else{
				step++;
				logic();
			}
		}else if (currentStep == steps.autoSlides && step >= calender.Count){
			currentStep = steps.smartSlides;
			step = 0;
			logic();
		}else if (currentStep == steps.smartSlides){
			if (step == 0){
				if (showBuildingFund){
					PlayVideo(buildingBackground, "buildingFund", false);
					step++;
				}else{
					step++;
					logic();
				}
			}else if (step == 1){
				if (showCWFCMF){
                    PlayVideo(cmfBackground, "cwf", false);
					step++;
				}else{
					step++;
					logic();
				}
			}else if (step == 2){
				if (showCal){
					StartCalender();
					step++;
				}else{
					step ++;
					logic();
				}
			}else if (step == 3){
				if(showCalOld){
					//Play Old Calender
					step++;
				}else{
					step++;
					logic();
				}
			}else{
				step = 0;
				currentStep = steps.slides;
				logic();
			}
		}else{
			printAlert.Error("currentStep was undefinded.  Resetting to currentStep.slides @0");
			step = 0;
			currentStep = steps.slides;
			logic();
		}

	}

	private void StartCalender(){StartCoroutine(Start_Calender());}
	private IEnumerator Start_Calender(){
		textOverLayTrigger.SetTrigger("calender");
		foreach(upcomingEvent e in calender){
			_calender.displayEvent(e);
			yield return new WaitForSeconds(5);
		}
		textOverLayTrigger.SetTrigger("hide");
		logic();
	}

	private void StartEnd(){StartCoroutine(Start_End());}
	private IEnumerator Start_End(){
		textOverLayTrigger.SetTrigger("hide");
		foreach(FileInfo f in videos){
			WWW newMovie = new WWW("file://"+ System.IO.Path.Combine(f.DirectoryName, f.Name));
			yield return newMovie;
			MovieTexture mt = newMovie.GetMovieTexture();

			screen.texture = mt;
			mt.Play();
			audioPlayer.clip=mt.audioClip;audioPlayer.Play();
			while (mt.isPlaying){yield return null;}
		}

		FileInfo f2 = endVideo[UnityEngine.Random.Range(0, endVideo.Length)];
		WWW newMovie2 = new WWW("file://"+ System.IO.Path.Combine(f2.DirectoryName, f2.Name));
		yield return newMovie2;
		MovieTexture mt2 = newMovie2.GetMovieTexture();

		screen.texture = mt2;
		mt2.Play();
		audioPlayer.clip=mt2.audioClip;audioPlayer.Play();
		while (mt2.isPlaying){yield return null;}

		endAction();
	}

	public void endAction(){
		if (endActionString == "endlessClock"){
			StopAllCoroutines();
			//screen.texture = BackgroundImage;
			textOverLayTrigger.SetTrigger("endlessClock");
		}
		else if(endActionString == "autoClose"){Application.Quit();}
		else { //Endless Slides
			clockTimer = 0;
			logic();
		}
	}

	public void LoadVideoInfo(){
		slide = new DirectoryInfo (System.IO.Path.Combine(videoURL, "Slides")).GetFiles("*.ogv*");
        if (slide.Length == 0)
        {
            printAlert.Warring("No slide videos could be found at " + System.IO.Path.Combine(videoURL, "Slide"));
            countdownBackgrounds = new DirectoryInfo(Application.streamingAssetsPath).GetFiles("defaultSlide.ogv");
        }
        videos = new DirectoryInfo (System.IO.Path.Combine(videoURL, "Videos")).GetFiles("*.ogv*");
		countdownBackgrounds = new DirectoryInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL, "Backgrounds"), "countDown")).GetFiles("*.ogv*");
		endVideo = new DirectoryInfo (System.IO.Path.Combine(videoURL, "VideosEnd")).GetFiles("*.ogv*");
		cmfBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "cmf_cwf.ogv"));
        if (!cmfBackground.Exists) {
            cmfBackground = new FileInfo(System.IO.Path.Combine(streamingPath, "defaultVideoBackground.ogv") );
            printAlert.Warring("No CWF/CMF background found at " + System.IO.Path.Combine(System.IO.Path.Combine(videoURL, "Backgrounds"), "cmf_cwf.ogv") + ".");
        }
		//buildingBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoPath,"Backgrounds"), "buildingFund_70.ogv"));
		clockBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "clockBackground.ogv"));
        if (!clockBackground.Exists)
        {
            clockBackground = new FileInfo(System.IO.Path.Combine(streamingPath, "defaultVideoBackground.ogv"));
            printAlert.Warring("No Clock Background background found at " + System.IO.Path.Combine(System.IO.Path.Combine(videoURL, "Backgrounds"), "clockBackground.ogv") + ".");
        }

        if (countdownBackgrounds.Length == 0)
        {
            printAlert.Warring("No background videos could be found at " + System.IO.Path.Combine(System.IO.Path.Combine(videoURL, "Backgrounds"), "countDown"));
            countdownBackgrounds = new DirectoryInfo(Application.streamingAssetsPath).GetFiles("defaultVideoBackground.ogv");
        }
        if (slide.Length == 0) { printAlert.Error("No slide videos could be found at "+ System.IO.Path.Combine(videoURL, "Slides"));
            slide = new DirectoryInfo(Application.streamingAssetsPath).GetFiles("defaultSlide.ogv");
        }
        if (videos.Length == 0) { printAlert.Warring("No videos could be found at " + System.IO.Path.Combine(videoURL, "Videos")); }
        if(endVideo.Length == 0) { printAlert.Error("No ending videos could be found at " + System.IO.Path.Combine(videoURL, "endVideos"));
            endVideo = new DirectoryInfo(Application.streamingAssetsPath).GetFiles("defaultSlide.ogv");
        }
        /*if (clockBackground.Length == 0)
        {
            printAlert.Warring("No clock background could be found at " + System.IO.Path.Combine(videoURL, "clockBackground.ogv"));
            clockBackground = new FileInfo(System.IO.Path.Combine(Application.streamingAssetsPath, "defaultSlide.ogv"));
        }*/


    }

	public void LoadOtherOnlineData(bool updateFromWeb){
		if (updateFromWeb){
			updater.DownloadOtherData();
		}

		FileInfo sourceFile = new FileInfo(System.IO.Path.Combine(Application.streamingAssetsPath, "otherOnlineData.txt"));
		StreamReader reader = sourceFile.OpenText();
		cwfDate = reader.ReadLine() + "\n";
		cwfDate = cwfDate + reader.ReadLine() + "\n";
		cwfDate = cwfDate + reader.ReadLine();
		cmfDate = reader.ReadLine() + "\n";
		cmfDate = cmfDate + reader.ReadLine() + "\n";
		cmfDate = cmfDate + reader.ReadLine();
		buildingFundTotal = reader.ReadLine();
		Debug.Log(reader.ReadLine());
		Debug.Log(reader.ReadLine());
		reader.Close();

		if (buildingFundTotal.Equals(null) || buildingFundTotal.Equals("0") || buildingFundTotal.Equals("")){
			showBuildingFund = false;
			buildingBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "buildingFund_00.ogv"));
			printAlert.Warring("Building fund total was not set.  Building fund slide will be skipped.");
		}
		else{
			int buildInt = int.Parse(buildingFundTotal);
			if (buildInt >= 100000){buildingBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "buildingFund_100.ogv"));}
			else if (buildInt >= 90000){buildingBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "buildingFund_90.ogv"));}
			else if (buildInt >= 80000){buildingBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "buildingFund_80.ogv"));}
			else if (buildInt >= 70000){buildingBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "buildingFund_70.ogv"));}
			else if (buildInt >= 60000){buildingBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "buildingFund_60.ogv"));}
			else if (buildInt >= 50000){buildingBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "buildingFund_50.ogv"));}
			else if (buildInt >= 40000){buildingBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "buildingFund_40.ogv"));}
			else if (buildInt >= 30000){buildingBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "buildingFund_30.ogv"));}
			else if (buildInt >= 20000){buildingBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "buildingFund_20.ogv"));}
			else if (buildInt >= 10000){buildingBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "buildingFund_10.ogv"));}
			else {buildingBackground = new FileInfo (System.IO.Path.Combine(System.IO.Path.Combine(videoURL,"Backgrounds"), "buildingFund_00.ogv"));}
			buildingFundTX.text = "$"+buildingFundTotal;
			if (buildInt < 100000){buildingFundSlider.value = buildInt/100000f;}
			else{buildingFundSlider.value = 1;}
		}

		if ( (cwfDate.Equals(null) || cwfDate.Equals("") || cwfDate.Equals("\n\n") ) && (cmfDate.Equals(null) || cmfDate.Equals("") || cmfDate.Equals("\n\n"))){
			showCWFCMF = false;
			printAlert.Warring("Both CMF and CWF dates are blank.  CMF/CWF slide will be skipped.");
			}
		else{cwfTX.text = cwfDate;cmfTX.text=cmfDate;}

		countdownAg.text = countdownAdjective + " begins in";

	}

	public void LoadEvents(){LoadEvents(false);}
	public void LoadEvents(bool updateFromWeb){

	if (updateFromWeb){
		updater.DownloadEvents();
	}

	FileInfo sourceFile = new FileInfo(System.IO.Path.Combine(Application.streamingAssetsPath, "onlineEvents.txt"));
	StreamReader reader = sourceFile.OpenText();
	int i = 0;
	string line;
	DateTime eventDate = DateTime.Now;
	string id = "";
	string eventTitle = "";
	string description = "";
		while((line = reader.ReadLine()) != null){
			string backgroundURL = System.IO.Path.Combine(videoURL, "Backgrounds");
			string backgroundURL_base = System.IO.Path.Combine(videoURL, "Backgrounds");
			i++;
			if (i == 1){id = line;}
			else if (i == 2){eventTitle = line;}
			else if (i == 3){description = line;}
			else if (i == 4){eventDate = DateTime.ParseExact(line, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);}
			else if (i == 5){
				backgroundURL = System.IO.Path.Combine(backgroundURL,line);
				if (backgroundURL.Equals(backgroundURL_base)){backgroundURL = "";}
				calender.Add(new upcomingEvent(id, eventTitle, eventDate, description, backgroundURL));
				i = 0;
			}else{printAlert.Error("Event Creater out of bounds.");}
		}
		if (i != 0){printAlert.Warring("onlineEvents.txt was not the right number of lines.  Last event may have been lost.");}

		reader.Close();
		upcomingEvent.weed(calender);

		float offset = -85;

		_calender.ResetCalender(calender.ToArray());
		printAlert.Log("Event calender loaded.");
	}

	public void SetNewStartTime(string time){
		startTime = DateTime.Parse(time);
		videoStartTime = startTime.AddSeconds(-1*endVideoOffsetInSecounds);
		ResetCountdown();
		//printAlert();
	}

	private void ResetCountdown(){
		int i = 0;
		while (i < counddownUsed.Length){counddownUsed[i]=false; i++;}
	}


	private int CountdownCalculator(){
		TimeSpan timeTill = startTime - DateTime.Now;

		if (timeTill.Hours == 0 && timeTill.Minutes <= 5 && counddownUsed[0] == false){
			counddownUsed[0]=true;
			counddownUsed[1]=true;
			counddownUsed[2]=true;
			counddownUsed[3]=true;
			counddownUsed[4]=true;
			counddownUsed[5]=true;
			counddownUsed[6]=true;
			counddownUsed[7]=true;
			counddownUsed[8]=true;
			counddownUsed[9]=true;
			counddownUsed[10]=true;
			return timeTill.Minutes;
		}else if (timeTill.Hours == 0 && timeTill.Minutes <= 10 && counddownUsed[1] == false){
			counddownUsed[1]=true;
			counddownUsed[2]=true;
			counddownUsed[3]=true;
			counddownUsed[4]=true;
			counddownUsed[5]=true;
			counddownUsed[6]=true;
			counddownUsed[7]=true;
			counddownUsed[8]=true;
			counddownUsed[9]=true;
			counddownUsed[10]=true;
			return timeTill.Minutes;
		}else if (timeTill.Hours == 0 && timeTill.Minutes <= 15 && counddownUsed[2] == false){
			counddownUsed[2]=true;
			counddownUsed[3]=true;
			counddownUsed[4]=true;
			counddownUsed[5]=true;
			counddownUsed[6]=true;
			counddownUsed[7]=true;
			counddownUsed[8]=true;
			counddownUsed[9]=true;
			counddownUsed[10]=true;
			return timeTill.Minutes;
		}else if (timeTill.Hours == 0 && timeTill.Minutes <= 20 && counddownUsed[3] == false){
			counddownUsed[3]=true;
			counddownUsed[4]=true;
			counddownUsed[5]=true;
			counddownUsed[6]=true;
			counddownUsed[7]=true;
			counddownUsed[8]=true;
			counddownUsed[9]=true;
			counddownUsed[10]=true;
			return timeTill.Minutes;
		}else if (timeTill.Hours == 0 && timeTill.Minutes <= 25 && counddownUsed[4] == false){
			counddownUsed[4]=true;
			counddownUsed[5]=true;
			counddownUsed[6]=true;
			counddownUsed[7]=true;
			counddownUsed[8]=true;
			counddownUsed[9]=true;
			counddownUsed[10]=true;
			return timeTill.Minutes;
		}else if (timeTill.Hours == 0 && timeTill.Minutes <= 30 && counddownUsed[5] == false){
			counddownUsed[5]=true;
			counddownUsed[6]=true;
			counddownUsed[7]=true;
			counddownUsed[8]=true;
			counddownUsed[9]=true;
			counddownUsed[10]=true;
			return timeTill.Minutes;
		}else if (timeTill.Hours == 0 && timeTill.Minutes <= 35 && counddownUsed[6] == false){
			counddownUsed[6]=true;
			counddownUsed[7]=true;
			counddownUsed[8]=true;
			counddownUsed[9]=true;
			counddownUsed[10]=true;
			return timeTill.Minutes;
		}else if (timeTill.Hours == 0 && timeTill.Minutes <= 40 && counddownUsed[7] == false){
			counddownUsed[7]=true;
			counddownUsed[8]=true;
			counddownUsed[9]=true;
			counddownUsed[10]=true;
			return timeTill.Minutes;
		}else if (timeTill.Hours == 0 && timeTill.Minutes <= 45 && counddownUsed[8] == false){
			counddownUsed[8]=true;
			counddownUsed[9]=true;
			counddownUsed[10]=true;
			return timeTill.Minutes;
		}else if (timeTill.Hours == 0 && timeTill.Minutes <= 50 && counddownUsed[9] == false){
			counddownUsed[9]=true;
			counddownUsed[10]=true;
			return timeTill.Minutes;
		}else if (timeTill.Hours == 0 && timeTill.Minutes <= 55 && counddownUsed[10] == false){
			counddownUsed[10]=true;
			return timeTill.Minutes;
		}

		return -1;

	}
	private void PlayVideo(MovieTexture f, string textOverlay, bool playSound){StartCoroutine(Play_Video(f,textOverlay, playSound));}
	private void PlayVideo(FileInfo f, bool playSound){StartCoroutine(Play_Video(f,"hide", playSound));}
	private void PlayVideo(FileInfo f){StartCoroutine(Play_Video(f, "hide", false));}
	private void PlayVideo(FileInfo f, string textOverlay){StartCoroutine(Play_Video(f,textOverlay, false));}
	private void PlayVideo(FileInfo f, string textOverlay, bool playSound){StartCoroutine(Play_Video(f,textOverlay, playSound));}
	private IEnumerator Play_Video(MovieTexture mt, string textOverlay, bool playSound){

		//if (mt != null){
			screen.texture = mt;
			mt.Play();
			if (playSound){audioPlayer.clip=mt.audioClip;audioPlayer.Play();}

			while (mt.isPlaying){yield return null;}
		//a.SetTrigger("hide");
		//}

		logic();
	}
	private IEnumerator Play_Video(FileInfo f, string textOverlay, bool playSound){
		
		textOverLayTrigger.SetTrigger("hide");
		WWW newMovie = new WWW("file://"+ System.IO.Path.Combine(f.DirectoryName, f.Name));
		yield return newMovie;
		MovieTexture mt = newMovie.GetMovieTexture();

		textOverLayTrigger.SetTrigger(textOverlay);
		screen.texture = mt;
		mt.Play();
		if (playSound){audioPlayer.clip=mt.audioClip;audioPlayer.Play();}
		while (mt.isPlaying){yield return null;}
		textOverLayTrigger.SetTrigger("hide");
		logic();

	}

	public void fullscreen(){Screen.fullScreen = !Screen.fullScreen;}
	public void Close(){Application.Quit();}
	public void GoToMenu(){Application.LoadLevel(0);}
	public void Reset(){Application.LoadLevel(Application.loadedLevel);}
	public void toggleBottom(){
		menu.enabled = false;
	}
	public void openRemote(){
		Application.OpenURL("http://dlm-movies.com/test/acc/announcementRemote.php");
	}
	public void ForceStart(){
		SetNewStartTime(DateTime.Now.ToString("yy-MM-dd HH:mm:ss"));
		SetIgnoreEndTimer(false);
	}

	IEnumerator loadBackground(){
		string path=System.IO.Path.Combine( System.IO.Path.Combine( PlayerPrefs.GetString("videoPath"), "Backgrounds"), "background.png");
		if (System.IO.File.Exists(path)){
			WWW www = new WWW(path);
			yield return www;
			endlessClockBackground.sprite = Sprite.Create(www.texture, new Rect(0,0,www.texture.width, www.texture.height), new Vector2 (0,0));
		}else{endlessClockBackground.sprite = defaultBackground;printAlert.Error("Background could not be found.  Go to \"Setting\" and check video path");}

	}

}
