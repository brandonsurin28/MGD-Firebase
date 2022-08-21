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

public class ReadScript : MonoBehaviour
{
    FirebaseApp app;
    FirebaseAuth auth;
    DatabaseReference databaseRoot;
    DatabaseReference userDatabase;

    [SerializeField] TextMeshProUGUI playerScoreLabel;
    [SerializeField] Button readDataBtn;

    void Awake()
    {
        readDataBtn.onClick.AddListener(ShowUpdateScore);

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
                userDatabase = databaseRoot.Child("users");

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
    public void ShowUpdateScore()
    {
        databaseRoot.Child("users").Child($"{auth.CurrentUser.UserId}/score").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                // Handle the error...
                Debug.LogError(" reading error: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                // Do something with snapshot...

                if (snapshot != null && snapshot.Exists)
                {
                    Debug.Log(snapshot.Value.ToString());
                    playerScoreLabel.text = snapshot.Value.ToString();
                }
                else
                {
                    Debug.LogError("No such data at this location!");
                }
            }
        });
    }

}
