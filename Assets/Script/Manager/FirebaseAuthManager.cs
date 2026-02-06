using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Photon.Pun;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseAuthManager : MonoBehaviour
{
    public FirebaseAuth auth;
    public FirebaseUser user;

    [SerializeField] TMP_InputField emailField;
    [SerializeField] TMP_InputField registerEmailField;
    [SerializeField] TMP_InputField pwField;
    [SerializeField] TMP_InputField registerPwField;
    [SerializeField] TMP_InputField nickField;
    [SerializeField] Button startButton;
    [SerializeField] GameObject LoginPanel;
    [SerializeField] GameObject RegisterPanel;

    public TMP_Text warningText;
    public TMP_Text registerWarningText;
    public TMP_Text confirmText;
    public TMP_Text registerConfirmText;


    private void Awake()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            }
            else
            {
                Debug.LogError(System.String.Format("뭔가 잘못되었음" + dependencyStatus));
            }
        });
    }
    void Start()
    {
        startButton.interactable = false;
        warningText.text = "";
        registerWarningText.text = "";
        confirmText.text = "";
        registerConfirmText.text = "";
    }

    public void Login()
    {
        StartCoroutine(LoginCor(emailField.text, pwField.text));
    }

    IEnumerator LoginCor(string email, string password)
    {
        Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if(LoginTask.Exception != null)
        {
            Debug.Log("다음과 같은 이유로 로그인 실패:" + LoginTask.Exception);

            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "";
            switch(errorCode)
            {
                case AuthError.MissingEmail:
                    message = "이메일 누락";
                    break;
                case AuthError.MissingPassword:
                    message = "패스워드 누락";
                    break;
                case AuthError.WrongPassword:
                    message = "패스워드 틀림";
                    break;
                case AuthError.InvalidEmail:
                    message = "이메일 형식이 옳지 않음";
                    break;
                case AuthError.UserNotFound:
                    message = "이메일이 존재하지 않음";
                    break;
                default:
                    message = "관리자에게 문의 바랍니다";
                    break;
            }
            warningText.text = message;
        }
        else
        {
            user = LoginTask.Result.User;
            warningText.text = "";
            nickField.text = user.DisplayName;
            PhotonNetwork.NickName = user.DisplayName;
            confirmText.text = "로그인 완료, 반갑습니다" + user.DisplayName + "님";
            startButton.interactable = true;
        }

            //auth.SignInWithEmailAndPasswordAsync(emailField.text, pwField.text).ContinueWithOnMainThread(task =>
            //{
            //    if (task.IsFaulted)
            //    {
            //        Debug.Log("로그인 오류");
            //        return;
            //    }
            //    if (task.IsCanceled)
            //    {
            //        Debug.Log("로그인 취소");
            //        return;
            //    }
            //    user = task.Result.User;
            //});
    }

    public void Register()
    {
        StartCoroutine(RegisterCor(registerEmailField.text, registerPwField.text, nickField.text));
        //auth.CreateUserWithEmailAndPasswordAsync(registerEmailField.text, registerPwField.text).ContinueWithOnMainThread(task =>
        //{
        //    if (task.IsFaulted)
        //    {
        //        Debug.Log("회원가입 오류");
        //        return;
        //    }
        //    if (task.IsCanceled)
        //    {
        //        Debug.Log("회원가입 취소");
        //        return;
        //    }
        //    user = task.Result.User;
        //});
    }

    IEnumerator RegisterCor(string email, string password, string userName)
    {
        Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

        if (RegisterTask.Exception != null)
        {
            Debug.LogWarning(message: "실패 사유" + RegisterTask.Exception);
            FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "회원가입 실패";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "이메일 누락";
                    break;
                case AuthError.MissingPassword:
                    message = "패스워드 누락";
                    break;
                case AuthError.WeakPassword:
                    message = "패스워드 약함";
                    break;
                case AuthError.EmailAlreadyInUse:
                    message = "중복 이메일";
                    break;
                default:
                    message = "기타 사유. 관리자 문의 바람";
                    break;
            }
            warningText.text = message;
        }
        else
        {
            user = RegisterTask.Result.User;

            if(user != null)
            {
                UserProfile profile = new UserProfile { DisplayName = userName };

                Task profileTask = user.UpdateUserProfileAsync(profile);
                yield return new WaitUntil(predicate: () => profileTask.IsCompleted);

                if(profileTask.Exception != null)
                {
                    Debug.LogWarning("닉네임설정 실패" +  profileTask.Exception);
                    FirebaseException firebaseEx = profileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                    warningText.text = "닉네임 설정에 실패하였습니다";
                }
                else
                {
                    registerWarningText.text = "";
                    registerConfirmText.text = "생성 완료, 반갑습니다 " + user.DisplayName + "님";
                    PhotonNetwork.NickName = user.DisplayName;
                    startButton.interactable = true;
                }
            }
        }
    }

    public void RegisterButton()
    {
        LoginPanel.SetActive(false);
        RegisterPanel.SetActive(true);
    }
    public void RegisterExit()
    {
        LoginPanel.SetActive(true);
        RegisterPanel.SetActive(false);
    }
}
