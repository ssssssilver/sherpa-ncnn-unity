using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
namespace Done
{

    public class SpeechToText_Android : MonoBehaviour
    {
        [SerializeField]
        private Text Text;
        [SerializeField]
        private Text buttonTxt;
        private AndroidJavaObject currentActivity;

        void Start()
        {
            InitMicrophone();
        }

        public void InitMicrophone()
        {   //初始化
            if (Application.platform == RuntimePlatform.Android)
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    currentActivity.Call("initModel");
                }
            }
        }
        bool isStartRecord = false;

        void Update()
        {
            if (currentActivity != null)
            {
                //获取语音识别结果
                string text = currentActivity.Call<string>("GetSpeechText");
                Text.text = text;
            }
        }
        bool isOn;
        //启用或停用语音识别
        public void StartMicrophoneCaptureClick()
        {
            currentActivity.Call("onclick");
            isOn = !isOn;
            if (isOn)
            {
                buttonTxt.text = "停止";
            }
            else
            {
                buttonTxt.text = "录制";
            }
        }





        private void OnDestroy()
        {
            if (Microphone.IsRecording(null))
                Microphone.End(null);
        }
        //#endif
    }
}