using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;

public class FirebaseLogout : MonoBehaviour
{
    [SerializeField] Button logoutBtn;


    private void Awake()
    {
        logoutBtn.onClick.AddListener(Logout);

    }

    void Logout()
    {
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
        SceneManager.LoadScene("LoginPage");
    }
}
