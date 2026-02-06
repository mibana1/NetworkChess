using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase;
using Firebase.Extensions;
using TMPro;

public class FirebaseAuthManager : MonoBehaviour
{
    public FirebaseAuth auth;
    public FirebaseUser user;

    [SerializeField] TMP_InputField emailField;
    [SerializeField] TMP_InputField pwField;
    [SerializeField] TMP_InputField nickField;
    [SerializeField] Button strtButton;

    public Text warningText;
    public Text confirmText;


    private void Awake()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                strtButton.interactable = true;
            }
            else
            {
                Debug.LogError(System.String.Format("뭔가 잘못되었음" + dependencyStatus));
            }
        });
    }
    void Start()
    {

    }

    public void Login()
    {
        auth.SignInWithEmailAndPasswordAsync(emailField.text, pwField.text).ContinueWithOnMainThread(task =>
        {
            if(task.IsFaulted)
            {
                Debug.Log("로그인 오류");
                return;
            }
            if(task.IsCanceled)
            {
                Debug.Log("로그인 취소");
                return;
            }
            user = task.Result.User;
        });
    }

    public void Register()
    {
        auth.CreateUserWithEmailAndPasswordAsync(emailField.text, pwField.text).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("회원가입 오류");
                return;
            }
            if (task.IsCanceled)
            {
                Debug.Log("회원가입 취소");
                return;
            }
            user = task.Result.User;
        });
    }

    void Update()
    {
        
    }
}
