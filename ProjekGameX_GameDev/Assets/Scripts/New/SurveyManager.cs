using UnityEngine;

public class SurveyManager : MonoBehaviour
{
    public void AbrirCuestionarioGoogle()
    {
        string linkForms = "https://docs.google.com/forms/d/e/1FAIpQLSdEOHPEWi7yWnL7uwmSoDVN-AtD1OnuIc_S60mMSQWtJJiLPQ/viewform?usp=publish-editor";
        Application.OpenURL(linkForms);
    }
}