using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class remote : MonoBehaviour {

	private updateFromWeb _web;
	private int connectFail;
	private DateTime lastUpdate;
	private alert printLog;

	// Use this for initialization
	void Start () {
			_web = this.GetComponent<updateFromWeb>();
			printLog = this.GetComponent<alert>();
			lastUpdate = DateTime.Now;
			StartCoroutine(check());
	}

	IEnumerator check(){
		bool conected = false;
		Ping pingMasterServer = new Ping(_web.GetAPI() + "?remote="+ lastUpdate.AddSeconds(-10).ToString("yy-MM-dd_HH:mm:ss") );
		Debug.Log(pingMasterServer.ip);
		float startTime = Time.time;
	//	while (!pingMasterServer.isDone && Time.time < startTime + 10f){
	//		yield return new WaitForSeconds(0.1f);
	//	}
//	yield return pi
	//	if (pingMasterServer.isDone){
			WWW www = new WWW(_web.GetAPI() + "?remote="+ lastUpdate.AddSeconds(-10).ToString("yy-MM-dd_HH:mm:ss"));
			yield return www;
		if (string.IsNullOrEmpty(www.error)){
	//		while(!www.isDone){yield return new WaitForSeconds(0.25f);}
			printLog.Remote(www.url);/////////////////////////
			string[] line = www.text.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);

			foreach (string s in line){
			printLog.Remote(s);////////////////////////
				if (!(s == null || s == "")){

					if (s.Contains(",")){
						string [] split = s.Split(',');
						if (split[0].Contains("ForceReset")){printLog.Remote(split[1].Replace("\n", ""));Application.LoadLevel(Application.loadedLevel);}
						else if (split[0].Contains("LoadEvent")){
							printLog.Remote("Updating event list from web...");
							_web.DownloadEvents();
							main m = GameObject.Find("Main Camera").GetComponent<main>();
							if (m != null){
								m.LoadEvents();
							}else{printLog.Warring("Could not find event calender.");}
						}
						else if (split[0].Contains("SetNewStartTime")){printLog.Remote("SetNewStartTime is not supported yet.");}
						else if (split[0].Contains("LoadOtherOnlineData")){
							printLog.Remote(split[1].Replace("\n", ""));
							_web.DownloadOtherData();
						}
						else if (split[0].Contains("LoadVideoInfo")){
							printLog.Remote(split[1].Replace("\n", ""));
							main m = GameObject.Find("Main Camera").GetComponent<main>();
							if (m != null){
								m.LoadVideoInfo();
							}else{printLog.Warring("Could not find event calender.");}
						}
						else if (split[0].Contains("Close")){Application.Quit();}
						else if (split[0].Contains("ForceStart")){
							printLog.Remote(split[1].Replace("\n", ""));
							main m = GameObject.Find("Main Camera").GetComponent<main>();
							if (m != null){
								m.SetNewStartTime(DateTime.Now.ToString("yy-MM-dd HH:mm:ss"));
								m.SetIgnoreEndTimer(false);
							}else{printLog.Warring("Could not find event calender.");}

						}
						else if (split[0].Contains("ForceClock")){
							printLog.Remote(split[1].Replace("\n", ""));
							main m = GameObject.Find("Main Camera").GetComponent<main>();
							if (m != null){
								m.endActionString = "endlessClock";
								m.endAction();
							}else{printLog.Warring("Could not find event calender.");}
						}
					}

				}
			}
			lastUpdate = DateTime.Now;
			connectFail = 0;
		yield return new WaitForSeconds(5f);
		printLog.alertClear();////////////////////////////////
		StartCoroutine(check());
		}else{
			printLog.Warring("There was an error connecting to remote. " + www.error.ToString());
			connectFail++;
			if (connectFail > 5){
				printLog.Error("Failed to connect to remote 5 or more time. Last Error: " + www.error);
				printLog.Warring("Now in offline mode.");
			}else{
				yield return new WaitForSeconds(7f);
				printLog.alertClear();////////////////////////////////
				StartCoroutine(check());
			}
		}
	/*	}else{
		//printLog.Warring("Failed to connect to remote ("+connectFail+").");///////////////////////////////
			connectFail++;
			if (connectFail > 5){
				printLog.Error("Failed to connect to remote 5 or more time.");
				printLog.Warring("Now in offline mode.");
			}else{
				yield return new WaitForSeconds(7f);
			//	printLog.alertClear();////////////////////////////////
				StartCoroutine(check());
			}
		}*/
	}

}
