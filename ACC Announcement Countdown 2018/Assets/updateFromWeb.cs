using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class updateFromWeb : MonoBehaviour {

	public static updateFromWeb instance = null;
	public string apiURL;
	private alert printAlert;

	void Start(){

		SetAPI();
		printAlert = this.GetComponent<alert>();
	}

	/// <summary>
	/// Gets the API address as a string.
	/// </summary>
	/// <returns>The API Address.</returns>
	public string GetAPI(){return apiURL;}

	public void SetAPI(){
		apiURL = PlayerPrefs.GetString("apiURL");
	}

	public void DownloadEvents(){StartCoroutine(DwnEvents());}
	private IEnumerator DwnEvents(){
		bool conected = false;
		Ping pingMasterServer = new Ping(apiURL + "?eventBuilder");
		float startTime = Time.time;
		/*while (!pingMasterServer.isDone && Time.time < startTime + 5f){
			yield return new WaitForSeconds(0.1f);
		}
		if (pingMasterServer.isDone){*/
			WWW www = new WWW(apiURL + "?eventBuilder");
			yield return www;

		if (string.IsNullOrEmpty(www.error)){

			string s = www.text;
			Debug.Log(s);
			StreamWriter sw = new StreamWriter (System.IO.Path.Combine(Application.streamingAssetsPath, "onlineEvents.txt"));
			sw.WriteLine(s);
			sw.Close();
			printAlert.Log("Event list downloaded.");

			}else{
			printAlert.Error("Could not connect to online database. " + www.error);printAlert.Warring("Could not update onlineEvents.txt.  An older version will be used.");
			}

		/*}else{
			printAlert.Error("Could not connect to online database.  Check your internet conection.");
			printAlert.Warring("Could not update onlineEvents.txt.  An older version will be used.");
		}*/
	}

	public void DownloadOtherData(){StartCoroutine(dwnOtherData());}
	private IEnumerator dwnOtherData(){
		bool conected = false;
			Ping pingMasterServer = new Ping(apiURL+"?cwf");
			float startTime = Time.time;
			/*while (!pingMasterServer.isDone && Time.time < startTime + 5f){
				yield return new WaitForSeconds(0.1f);
			}
			if (pingMasterServer.isDone){*/

				StreamWriter sw = new StreamWriter (System.IO.Path.Combine(Application.streamingAssetsPath, "otherOnlineData.txt"));
				WWW www = new WWW(apiURL + "?cwf");
				yield return www;
					if (string.IsNullOrEmpty(www.error)){
				sw.WriteLine(www.text);
				www = new WWW(apiURL + "?cmf");
				yield return www;
					if (string.IsNullOrEmpty(www.error)){
				sw.WriteLine(www.text);
				www = new WWW(apiURL + "?buildingFund");
				yield return www;
					if (string.IsNullOrEmpty(www.error)){
				sw.WriteLine(www.text);

				sw.Close();

				printAlert.Log("Online data updated.");

				}else{sw.Close();printAlert.Error("Could not connect to online database (1_CWF). " + www.error);printAlert.Warring("Could not update otherOnlineData.txt.  An older version will be used.");}
			}else{sw.Close();printAlert.Error("Could not connect to online database (2_CMF). " + www.error);printAlert.Warring("Could not update otherOnlineData.txt.  An older version will be used.");}
		}else{sw.Close();printAlert.Error("Could not connect to online database (3_buildingFund). " + www.error);printAlert.Warring("Could not update otherOnlineData.txt.  An older version will be used.");}

			/*}else{
			printAlert.Error("Could not connect to online database.  Check your internet conection.");
			printAlert.Warring("Could not update otherOnlineData.txt.  An older version will be used.");
			}*/
	}
}
