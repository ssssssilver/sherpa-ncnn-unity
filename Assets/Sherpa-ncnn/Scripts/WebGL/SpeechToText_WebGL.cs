using System.Collections;
using System.Collections.Generic;
#if UNITY_WEBGL
using System.IO;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Done
{
    public class SpeechToText_WebGL : MonoBehaviour
    {
        // 声明配置和识别器变量
        private SherpaNcnn.OnlineRecognizer recognizer;
        private SherpaNcnn.OnlineStream onlineStream;
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

        /// <summary>
        /// 语音输入的按钮
        /// </summary>
        [SerializeField] private Button m_VoiceInputBotton;

        public VoiceInputs voiceInputs;
#if UNITY_WEBGL
        /// <summary>
        /// 注册按钮事件
        /// </summary>
        private void RegistButtonEvent()
        {
            if (m_VoiceInputBotton == null || m_VoiceInputBotton.GetComponent<EventTrigger>())
                return;

            EventTrigger _trigger = m_VoiceInputBotton.gameObject.AddComponent<EventTrigger>();

            //添加按钮按下的事件
            EventTrigger.Entry _pointDown_entry = new EventTrigger.Entry();
            _pointDown_entry.eventID = EventTriggerType.PointerDown;
            _pointDown_entry.callback = new EventTrigger.TriggerEvent();

            //添加按钮松开事件
            EventTrigger.Entry _pointUp_entry = new EventTrigger.Entry();
            _pointUp_entry.eventID = EventTriggerType.PointerUp;
            _pointUp_entry.callback = new EventTrigger.TriggerEvent();

            //添加委托事件
            _pointDown_entry.callback.AddListener(delegate { StartRecord(); });
            _pointUp_entry.callback.AddListener(delegate { StopRecord(); });

            _trigger.triggers.Add(_pointDown_entry);
            _trigger.triggers.Add(_pointUp_entry);
        }
        public void StartRecord()
        {
            voiceInputs.StartRecordAudio();
            buttonTxt.text = "录制中";
        }
        public void StopRecord()
        {
            voiceInputs.StopRecordAudio((clip) =>
            {
                TranslateAudio(clip);
                buttonTxt.text = "录制";
            });
        }

        private void TranslateAudio(AudioClip clip)
        {
            onlineStream = recognizer.CreateStream();
            int currentPosition = clip.samples;
            Debug.Log("currentPosition:" + currentPosition);
            if (currentPosition > 0)
            {
                int sampleCount = clip.samples * clip.channels;
                float[] samples = new float[sampleCount];
                clip.GetData(samples, 0);
                onlineStream.AcceptWaveform(clip.frequency, samples);


            }
        }
        void Start()
        {
            // 初始化配置
            SherpaNcnn.OnlineRecognizerConfig config = new SherpaNcnn.OnlineRecognizerConfig
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
            recognizer = new SherpaNcnn.OnlineRecognizer(config);
            RegistButtonEvent();
        }
        public AudioClip clip;
        int timer = 0;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                TranslateAudio(clip);
            }

            // 每帧更新识别器状态
            if (onlineStream != null)
            {
                if (recognizer.IsReady(onlineStream))
                {
                    recognizer.Decode(onlineStream);
                }
                string text = recognizer.GetResult(onlineStream).Text;
                if (text != lastText)
                {
                    lastText = text;
                    timer = 0;
                }
                else
                {
                    timer++;
                }
                //bool isEndpoint = recognizer.IsEndpoint(onlineStream);

                if (timer > 10)
                {
                    Debug.Log(text);
                    recognizer.Reset(onlineStream);
                    onlineStream = null;
                }
                Text.text = text;
            }
        }
#endif

    }
}