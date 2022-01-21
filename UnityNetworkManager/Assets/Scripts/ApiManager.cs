using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Authentication;
using UnityEngine.Networking;

/// <summary>
/// This static class handles sending api requests to moodle.
/// </summary>
public static class ApiManager
{
    //Moodle site URL including formatting and WebServiceToken
    private const string MoodleWebServiceURL = "https://surrogates.simetri.us/webservice/rest/server.php?moodlewsrestformat=json&wstoken=c5fd506c6bc73a6939799540aa34a9bb&wsfunction=";

    /// <summary>
    /// Creates a new user in the Moodle DB, 
    /// a confirmation email will be sent to the user.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="firstname"></param>
    /// <param name="lastname"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public static NewUserResponse RegisterNewUserRequest(string email, string firstname, string lastname, string username, string password)
    {
        NewUserResponse result = new NewUserResponse();
        result.success = false;

        try
        {
            var www = UnityWebRequest.Get(MoodleWebServiceURL + "auth_email_signup_user&" + "username=" + username + "&password=" + password + "&email=" + email + "&firstname=" + firstname + "&lastname=" + lastname);
            www.SendWebRequest();

            while (!www.isDone)
            {
                //Debug.Log("waiting for response");
            }

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);

            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }

            NewUserResponse data = JsonUtility.FromJson<NewUserResponse>(www.downloadHandler.text);
            result = data;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        return result;
    }

    //This function can be used to confirm a new user
    //requires the secret code sent to the user via email
    //TODO: call this on a "Confirm user" panel after sending RegisterNewUserRequest
    public static void ConfirmNewUserRequest(string username, string secret)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(MoodleWebServiceURL + "core_auth_confirm_user&" + "username=" + username + "&secret=" + secret);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string jsonResponse = reader.ReadToEnd();

            if (jsonResponse.Contains("moodle_exception"))
            {

            }
            else if (jsonResponse.Contains("success"))
            {
                Debug.Log("Confirmed User!");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

    }

    /// <summary>
    /// Attempts to authorize the user, if no token is returned, then the user is not logged in.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static string LoginRequest(string username, string password)
    {
        string token = "";
        Debug.Log("beginning task");
        try
        {
            //const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            //const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            //ServicePointManager.SecurityProtocol = Tls12;
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://surrogates.simetri.us/login/token.php?service=moodle_mobile_app&username=" + username + "&password=" + password);
            //HttpWebResponse response = (HttpWebResponse) await (request.GetResponseAsync());
            //StreamReader reader = new StreamReader(response.GetResponseStream());

            var www = UnityWebRequest.Get("https://surrogates.simetri.us/login/token.php?service=moodle_mobile_app&username=" + username + "&password=" + password);
            www.SendWebRequest();

            while (!www.isDone)
            {
                //Debug.Log("waiting for response");
            }

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);

            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }

            LoginData data = JsonUtility.FromJson<LoginData>(www.downloadHandler.text);  // reader.ReadToEnd();

            if (data.token != null && data.privatetoken != null)
            {
                token = data.token;
                Debug.Log("Successful login");
            }
            else
            {
                Debug.Log("Login Failed");
            }

        }
        catch (Exception e)
        {
            Debug.Log(e);
            //return e.Message;
        }

        return token;
    }

}

[Serializable]
public class LoginData
{
    public string token;
    public string privatetoken;
}

[Serializable]
public class NewUserResponse
{
    public bool success;
    public List<Warning> warnings;
}

[Serializable]
public class Warning
{
    public string item;
    public int itemid;
    public string warningcode;
    public string message;
}