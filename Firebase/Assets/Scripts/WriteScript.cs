using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
using Google;
using Firebase.Database;
using System;

public class WriteScript : MonoBehaviour
{
    FirebaseApp app;
    FirebaseAuth auth;
    DatabaseReference databaseRoot;
    DatabaseReference userDatabase;

    string playerName;
    string playerScore;

    [SerializeField] TMP_InputField playerNameInput;
    [SerializeField] TMP_InputField playerScoreInput;
    [SerializeField] Button writeDataBtn;
    [SerializeField] PlayerScore saveScore;

    void Awake()
    {
        playerNameInput.onEndEdit.AddListener(GetPlayerName);
        playerScoreInput.onEndEdit.AddListener(GetPlayerScore);
        writeDataBtn.onClick.AddListener(WriteData);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = FirebaseApp.DefaultInstance;
                auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

                databaseRoot = FirebaseDatabase.DefaultInstance.RootReference;
                //userDatabase = databaseRoot.Child("users");

                Debug.Log($"Firebase initialized!");
                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    public void WriteData()
    {
        //Serialized newScore to Json
        string serialisedNewScore = JsonUtility.ToJson(saveScore);
        Debug.Log(serialisedNewScore);

        //databaseRoot.Child("users").Child(auth.CurrentUser.UserId).Child("username").SetValueAsync(name);
        // databaseRoot.Child($"users/){auth.CurrentUser.UserId}").SetValueAsync(name)
        databaseRoot.Child("users").Child(auth.CurrentUser.UserId).SetRawJsonValueAsync(serialisedNewScore).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("WriteData was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("WriteData encountered an error: " + task.Exception);

                return;
            }
            Debug.Log($"Wrote data sucessfully");
        });
    }

    void GetPlayerName(string n)
    {
        saveScore.playerName = n;
    }
    void GetPlayerScore(string s)
    {
        saveScore.score = s;
    }

    [Serializable]
    public class PlayerScore
    {
        //public string playerID;
        public string playerName;
        public string score;

       // public PlayerScore(string playerID, string playerName, string score)
        public PlayerScore( string playerName, string score)
        {
            //this.playerID = playerID;
            this.playerName = playerName;
            this.score = score;
        }
    }
}

