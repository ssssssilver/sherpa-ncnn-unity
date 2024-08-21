using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceInputs : MonoBehaviour
{

        /// <summary>
        /// 录制的音频长度
        /// </summary>
        public int m_RecordingLength = 5;

        public AudioClip recording;

        /// <summary>
        /// WebGL辅助类
        /// </summary>
        [SerializeField] private SignalManager signalManager;
        /// <summary>
        /// 开始录制声音
        /// </summary>
        public void StartRecordAudio()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        signalManager.onAudioClipDone = null;
        signalManager.StartRecordBinding();
#else
                recording = Microphone.Start(null, false, m_RecordingLength, 16000);

#endif
        }

        /// <summary>
        /// 结束录制，返回audioClip
        /// </summary>
        /// <param name="_callback"></param>
        public void StopRecordAudio(Action<AudioClip> _callback)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        signalManager.onAudioClipDone += _callback;
        signalManager.StopRecordBinding();
#else
                Microphone.End(null);
                _callback(recording);

#endif

        }

}
