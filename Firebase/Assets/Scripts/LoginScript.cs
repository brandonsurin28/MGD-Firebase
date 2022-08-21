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

public class LoginScript : MonoBehaviour
{
    FirebaseApp app;
    FirebaseAuth auth;
    DatabaseReference databaseRoot;
    DatabaseReference userDatabase;

    [Header("Buttons")]
    [SerializeField] Button createAccountBtn;
    [SerializeField] Button LoginBtn;
    [SerializeField] Button googleLoginBtn;
    [SerializeField] Button readDataBtn;
    [SerializeField] Button writeDataBtn;

    [Header("Login Input Fields")]
    [SerializeField] TMP_InputField loginEmailInput;
    [SerializeField] TMP_InputField loginPasswordInput;

    [Header("Register Input Fields")]
    [SerializeField] TMP_InputField emailInput;
    [SerializeField] TMP_InputField passwordInput;

    [Header("User Input Fields")]
    [SerializeField] string email;
    [SerializeField] string password;

    [Header("Writing Data testing")]
    [SerializeField] string playerID;
    [SerializeField] PlayerScore scoreToSave;
    [SerializeField] PlayerScore loadedScore;
    [NonReorderable]
    [SerializeField] List<PlayerScore> loadedScoresFromDatabase;

    [Header("Misc")]
    [SerializeField] int newScore;
    [SerializeField] TextMeshProUGUI scoreText;



    private void Awake()
    {
        loginEmailInput.onEndEdit.AddListener(GetEmail);
        loginPasswordInput.onEndEdit.AddListener(GetPassword);

        emailInput.onEndEdit.AddListener(GetEmail);
        passwordInput.onEndEdit.AddListener(GetPassword);

        createAccountBtn.onClick.AddListener(CreateEmailAccount);
        LoginBtn.onClick.AddListener(LoginWithEmail);
        googleLoginBtn.onClick.AddListener(SignInWithGoogle);

        writeDataBtn.onClick.AddListener(WriteRawJsonData);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => 
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                databaseRoot = FirebaseDatabase.DefaultInstance.RootReference;
                userDatabase = databaseRoot.Child("users");
                Debug.Log($"Firebase Inisialise");

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

    // create method to accces error parse messages 
    //public static string GetErrorMessage(Exception exception)
    //{

    //}

    //private static string GetErrorMessage(AuthError errorCode)
    //{

    //}

    #region Authentication Methods (Create Email Account, Login, Login with Google)
    void GetEmail(string e)
    {
        email = e;
    }

    void GetPassword(string p)
    {
        password = p;
    }

    void CreateEmailAccount()
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => 
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);

                return;
            }

            // Firebase user has been created.
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });
    }

    void LoginWithEmail()
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);

                FirebaseException exception = (FirebaseException)task.Exception.InnerException;

                AuthError authError = (AuthError)exception.ErrorCode;

                switch (authError)
                {
                    case AuthError.WrongPassword:
                        Debug.LogError("Wrong Password");
                        break;
                    case AuthError.UserNotFound:
                        Debug.LogError("Username Not Found");
                        break;
                    default:
                        Debug.LogError("Unknown Error!");
                        break;
                }

                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);

            SceneManager.LoadScene("GameScene");
        });

    }

    void SignInWithGoogle()
    {
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            // Copy this value from the google-service.json file.
            // oauth_client with type == 3
            WebClientId = "756073559094-bq7p9im2bmnhqp1va5mdcrh37ash4cvb.apps.googleusercontent.com"
        };

        Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

        TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
        signIn.ContinueWith(task => {
            if (task.IsCanceled)
            {
                signInCompleted.SetCanceled();
            }
            else if (task.IsFaulted)
            {
                signInCompleted.SetException(task.Exception);
            }
            else
            {

                Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>)task).Result.IdToken, null);
                auth.SignInWithCredentialAsync(credential).ContinueWith(authTask => {
                    if (authTask.IsCanceled)
                    {
                        signInCompleted.SetCanceled();
                    }
                    else if (authTask.IsFaulted)
                    {
                        signInCompleted.SetException(authTask.Exception);
                    }
                    else
                    {
                        signInCompleted.SetResult(((Task<FirebaseUser>)authTask).Result);
                        SceneManager.LoadScene("GameScene");
                    }
                });
            }
        });
    }

    #endregion

    #region Writing Data
    public void WriteData()
    {
        //Serialized newScore to Json
        string serialisedNewScore = JsonUtility.ToJson(scoreToSave);
        Debug.Log(serialisedNewScore);

        //databaseRoot.Child("users").Child(auth.CurrentUser.UserId).Child("username").SetValueAsync(name);
        databaseRoot.Child($"users/){auth.CurrentUser.UserId}").SetValueAsync(name).ContinueWith(task =>
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

    public void WriteRawJsonData()
    {
        //Serialized newScore to Json
        string serialisedNewScore = JsonUtility.ToJson(scoreToSave);
        Debug.Log(serialisedNewScore);


        //databaseRoot.Child("users").Child(auth.CurrentUser.UserId).Child("username").SetValueAsync(name);

        userDatabase.Child($"{playerID}").SetRawJsonValueAsync(serialisedNewScore).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("WriteRawJsonData was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("WriteRawJsonData encountered an error: " + task.Exception);

                return;
            }
            Debug.Log($"Wrote Raw Json data sucessfully");
        });

    }

    public void UpdateData()
    {
        Dictionary<string, object> dataToUpdate = new Dictionary<string, object>();

        string uniqueKey = databaseRoot.Push().Key;

        //dataToUpdate.Add($"users/{playerID}/score", newScore);
        dataToUpdate[$"users/{playerID}/score"] = newScore;
        //dataToUpdate[$"users/{uniqueKey}/score"] = 100;


        //databaseRoot.Child("users").Child(auth.CurrentUser.UserId).Child("username").SetValueAsync(name);
        databaseRoot.UpdateChildrenAsync(dataToUpdate).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("UpdateData was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("UpdateData encountered an error: " + task.Exception);

                return;
            }
            Debug.Log($"UpdateData data sucessfully");
        });
    }

    public void UpdateToMakeNewData()
    {
        Dictionary<string, object> dataToUpdate = new Dictionary<string, object>();

        string uniqueKey = databaseRoot.Push().Key;

        //dataToUpdate.Add($"users/{playerID}/score", newScore);
        dataToUpdate[$"Monsters"] = 100;
        //dataToUpdate[$"users/{uniqueKey}/score"] = 100;


        //databaseRoot.Child("users").Child(auth.CurrentUser.UserId).Child("username").SetValueAsync(name);
        databaseRoot.UpdateChildrenAsync(dataToUpdate).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("UpdateToMakeNewData was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("UpdateToMakeNewData encountered an error: " + task.Exception);

                return;
            }
            Debug.Log($"UpdateToMakeNew data sucessfully");
        });
    }

    public void PushDataToDatabase()
    {
        databaseRoot.Child($"users/UserWithManyScores").Push().SetValueAsync(100);
    }

    #endregion

    #region Reading Data
    public void loadSingleDataOnMainThread()
    {
        userDatabase.Child($"{playerID}/score").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Load Data was canceled.");

            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot != null && snapshot.Exists)
                {
                    // Do something with snapshot...
                    Debug.Log(snapshot.Value);
                    scoreText.text = snapshot.Value.ToString();
                }
                else
                {
                    Debug.LogError("No Such data found in Firebase Database.");
                }
            }

        });

    }

    public void LoadRawJsonData()
    {
        userDatabase.Child($"{playerID}").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Load Data was canceled.");

            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot != null && snapshot.Exists)
                {
                    // Do something with snapshot...
                    string loadedJson = snapshot.GetRawJsonValue();
                    Debug.Log(loadedJson);

                    PlayerScore loadedPlayerScore = JsonUtility.FromJson<PlayerScore>(loadedJson);
                    loadedScore = loadedPlayerScore;

                }
                else
                {
                    Debug.LogError("No Such data found in Firebase Database.");
                }
            }

        });
    }

    public void LoadMultipleDataEntries()
    {
        userDatabase.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Load Data was canceled.");

            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot != null && snapshot.Exists && snapshot.ChildrenCount > 0 )
                {
                    List<PlayerScore> allPlayerScores = new List<PlayerScore>();

                    foreach (var childSnapshot in snapshot.Children)
                    {
                        string childJson = childSnapshot.GetRawJsonValue();
                        Debug.Log(childJson);

                        PlayerScore playerScore = JsonUtility.FromJson<PlayerScore>(childJson);
                        loadedScoresFromDatabase.Add(playerScore);

                    }

                }
                else
                {
                    Debug.LogError("No Such data found in Firebase Database.");
                }
            }

        });
    }




    #endregion

    [Serializable]
    public class PlayerScore
    {
        //public string playerID;
        public string playerName;
        public int score;

        public PlayerScore(string playerID, string playerName, int score)
        {
            //this.playerID = playerID;
            this.playerName = playerName;
            this.score = score;
        }
    }

}
