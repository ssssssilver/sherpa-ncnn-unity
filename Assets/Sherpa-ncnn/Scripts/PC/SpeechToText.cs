using System.Collections;
using System.Collections.Generic;
#if !UNITY_WEBGL && !UNITY_ANDROID
using System.IO;
#endif
using SherpaNcnn;

using UnityEditor;
using UnityEngine;

using UnityEngine.UI;
namespace Done
{
    public class SpeechToText : MonoBehaviour
    {
        //#if !UNITY_WEBGL&& !UNITY_EDITOR
        // 声明配置和识别器变量
        private OnlineRecognizer recognizer;
        private OnlineStream onlineStream;
        private int segmentIndex = 0;
        private string lastText = "";
        [SerializeField]
        private Text Text;
        [SerializeField]
        private Text buttonTxt;

        // 可以在Unity编辑器中设置这些参数
        public string tokensPath;
        public string encoderParamPath;
        public string encoderBinPath;
        public string decoderParamPath;
        public string decoderBinPath;
        public string joinerParamPath;
        public string joinerBinPath;
        public int numThreads = 1;
        public string decodingMethod = "greedy_search";
#if !UNITY_WEBGL && !UNITY_ANDROID
        void Start()
        {
            // 初始化配置
            OnlineRecognizerConfig config = new OnlineRecognizerConfig
            {
                FeatConfig = { SampleRate = 16000, FeatureDim = 80 },
                ModelConfig = {
                Tokens = Path.Combine(Application.streamingAssetsPath,tokensPath),
                EncoderParam =  Path.Combine(Application.streamingAssetsPath,encoderParamPath),
                EncoderBin =Path.Combine(Application.streamingAssetsPath, encoderBinPath),
                DecoderParam =Path.Combine(Application.streamingAssetsPath, decoderParamPath),
                DecoderBin = Path.Combine(Application.streamingAssetsPath, decoderBinPath),
                JoinerParam = Path.Combine(Application.streamingAssetsPath,joinerParamPath),
                JoinerBin =Path.Combine(Application.streamingAssetsPath,joinerBinPath),
                UseVulkanCompute = 0,
                NumThreads = numThreads
            },
                DecoderConfig = {
                DecodingMethod = decodingMethod,
                NumActivePaths = 4
            },
                EnableEndpoint = 1,
                Rule1MinTrailingSilence = 2.4F,
                Rule2MinTrailingSilence = 1.2F,
                Rule3MinUtteranceLength = 20.0F
            };

            // 创建识别器和在线流
            recognizer = new OnlineRecognizer(config);


            // 启动麦克风捕获
            // StartMicrophoneCapture();
        }
        bool isStartRecord = false;

        void Update()
        {
            if (!isStartRecord) return;
            int currentPosition = Microphone.GetPosition(null);
            int sampleCount = currentPosition - lastSamplePosition;
            if (sampleCount < 0)
            {
                sampleCount += micClip.samples * micClip.channels;
            }

            if (sampleCount > 0)
            {
                float[] samples = new float[sampleCount];
                micClip.GetData(samples, lastSamplePosition);

                // 将采集到的音频数据传递给识别器
                onlineStream.AcceptWaveform(micClip.frequency, samples);

                // 更新lastSamplePosition
                lastSamplePosition = currentPosition;
            }
            // 每帧更新识别器状态
            if (recognizer.IsReady(onlineStream))
            {
                recognizer.Decode(onlineStream);
            }

            var text = recognizer.GetResult(onlineStream).Text;
            bool isEndpoint = recognizer.IsEndpoint(onlineStream);
            if (!string.IsNullOrWhiteSpace(text) && lastText != text)
            {
                lastText = text;
                //Debug.Log($"{segmentIndex}: {lastText}");
                Text.text = lastText;
            }

            if (isEndpoint)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    ++segmentIndex;
                    StopMicrophoneCapture();
                    //Debug.Log(text);
                    buttonTxt.text = "录制";
                }
                recognizer.Reset(onlineStream);
            }
        }
        private AudioClip micClip;
        private int lastSamplePosition = 0;
        public void StartMicrophoneCapture()
        {
            onlineStream = recognizer.CreateStream();
            StartCoroutine(CheckMicoPhoneInit());
        }
        IEnumerator CheckMicoPhoneInit()
        {
            string device = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
            micClip = Microphone.Start(device, true, 10, 16000);
            while (!(Microphone.GetPosition(device) > 0)) { yield return null; }
            lastSamplePosition = Microphone.GetPosition(device);
            isStartRecord = true;
            buttonTxt.text = "录制中...";
        }



        public void StopMicrophoneCapture()
        {
            isStartRecord = false;
            // 停止麦克风捕获
            Microphone.End(null);
        }

        private void OnDestroy()
        {
            recognizer.Dispose();
            if (Microphone.IsRecording(null))
                Microphone.End(null);
        }
#endif
    }
}