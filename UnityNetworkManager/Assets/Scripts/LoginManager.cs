using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

/// <summary>
/// The login Manager class handles authentication and the creation
/// of new users using the API Manager. 
/// This class also handles all the UI for the login panel, and stores 
/// the users credentials.
/// </summary>
public class LoginManager : MonoBehaviour
{
    //Main Panel & UI 
    [SerializeField]
    GameObject LoginPanel;
    [SerializeField]
    GameObject NewUserPanel;
    [SerializeField]
    GameObject SelectTrainingPanel;
    [SerializeField]
    GameObject MenuButton;
    [SerializeField]
    GameObject PopUpPanel;
    [SerializeField]
    TextMeshProUGUI popUpText;

    /// NEW USER PANEL UI
    [SerializeField]
    Text email;
    [SerializeField]
    Text firstName;
    [SerializeField]
    Text lastName;
    [SerializeField]
    Text newUsername;
    [SerializeField]
    Text newPassword;

    //login panel UI
    [SerializeField]
    TMP_InputField username;
    [SerializeField]
    TMP_InputField password;

    private string curUserToken;
    public string curUsername;
    private string curPassword;

    /// <summary>
    /// Called when the login button is pushed.
    /// Currently allows direct entrance to the app if "test" is 
    /// used for both credentials.
    /// Fails if not connected to Moodle server
    /// TODO : Add gracefull exception handling
    /// </summary>
    public void login()
    {
        //if (username.text == "test" && password.text == "test")
        //{
            curUsername = username.text;
            curPassword = password.text;
            LoginPanel.SetActive(false);
            SelectTrainingPanel.SetActive(true);
            MenuButton.gameObject.SetActive(true);
        //}
        //else
        //{
        //    string result = ApiManager.LoginRequest(username.text, password.text);
        //    if (string.IsNullOrEmpty(result))
        //    {
        //        Debug.Log("Failed to login");
        //        //todo display wrong username/password popup
        //        popUpText.text = "Incorrect Username or Password.";
        //        PopUpPanel.SetActive(true);
        //    }
        //    else
        //    {
        //        curUserToken = result;
        //        curUsername = username.text;
        //        curPassword = password.text;
        //        LoginPanel.SetActive(false);
        //        SelectTrainingPanel.SetActive(true);
        //        MenuButton.gameObject.SetActive(true);
        //    }
        //}
    }

    /// <summary>
    /// This button is called when the ENTER button is pushed 
    /// on the New User panel. Uses the API manager to call the moodle API endpoint.
    /// </summary>
    public void CreateNewUser()
    {
        try
        {
            NewUserResponse result = ApiManager.RegisterNewUserRequest(email.text, firstName.text, lastName.text, newUsername.text, newPassword.text);
            // display popup based on result

            if (!result.success)
            {
                //todo change popup based on warnings
                //popUpText.text = "Failed to create new user!";
                popUpText.text = "";
                foreach (var item in result.warnings)
                {
                    if (item.message.Contains("Passwords"))
                    {
                        popUpText.text += " Passwords must be at least 8 characters long, have at least 1 digit, 1 upper case letter, and 1 non-alphanumeric character.";
                    }
                    else
                    {
                        popUpText.text += " " + item.message + ".";
                    }
                }
                PopUpPanel.SetActive(true);
            }
            else
            {
                popUpText.text = "Successfully created new user! Follow the emailed instructions to confirm your account.";
                PopUpPanel.SetActive(true);
                NewUserPanel.SetActive(false);
                LoginPanel.SetActive(true);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

}
