using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SignalManager : MonoBehaviour
{
 
    #region Public Method
    /// <summary>
    /// 语音合成成功，回调
    /// </summary>
    public Action<AudioClip> onAudioClipDone;
    /// <summary>
    /// 开始录制
    /// </summary>
    public void StartRecordBinding() {
        StartRecorderFunc();
    }
    /// <summary>
    /// 结束录制
    /// </summary>
    public void StopRecordBinding()
    {
        EndRecorderFunc();
    }

    #endregion
    #region UnityToJs
    [DllImport("__Internal")]
    private static extern void StartRecord();
    [DllImport("__Internal")]
    private static extern void StopRecord();
    void StartRecorderFunc()
    {
        StartRecord();
    }
    void EndRecorderFunc()
    {
        StopRecord();
    }
    #endregion

    #region JsToUnity
    #region Data
    /// <summary>
    ///需获取数据的数目
    /// </summary>
    private int m_valuePartCount = 0;
    /// <summary>
    /// 获取的数据数目
    /// </summary>
    private int m_getDataLength = 0;
    /// <summary>
    /// 获取的数据长度
    /// </summary>
    private int m_audioLength = 0;
    /// <summary>
    /// 获取的数据
    /// </summary>
    private string[] m_audioData = null;

    /// <summary>
    /// 当前音频
    /// </summary>
    public static AudioClip m_audioClip = null;

    /// <summary>
    /// 音频片段存放列表
    /// </summary>
    private List<byte[]> m_audioClipDataList;

    /// <summary>
    /// 片段结束标记
    /// </summary>
    private string m_currentRecorderSign;
    /// <summary>
    /// 音频频率
    /// </summary>
    private int m_audioFrequency;

    /// <summary>
    /// 单次最大录制时间
    /// </summary>
    private const int maxRecordTime = 30;
    #endregion

    public void GetAudioData(string _audioDataString)
    {
        if (_audioDataString.Contains("Head"))
        {
            string[] _headValue = _audioDataString.Split('|');
            m_valuePartCount = int.Parse(_headValue[1]);
            m_audioLength = int.Parse(_headValue[2]);
            m_currentRecorderSign = _headValue[3];
            m_audioData = new string[m_valuePartCount];
            m_getDataLength = 0;
            Debug.Log("接收数据头：" + m_valuePartCount + "   " + m_audioLength);
        }
        else if (_audioDataString.Contains("Part"))
        {
            string[] _headValue = _audioDataString.Split('|');
            int _dataIndex = int.Parse(_headValue[1]);
            m_audioData[_dataIndex] = _headValue[2];
            m_getDataLength++;
            if (m_getDataLength == m_valuePartCount)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < m_audioData.Length; i++)
                {
                    stringBuilder.Append(m_audioData[i]);
                }
                string _audioDataValue = stringBuilder.ToString();
                Debug.Log("接收长度:" + _audioDataValue.Length + " 需接收长度:" + m_audioLength);
                int _index = _audioDataValue.LastIndexOf(',');
                string _value = _audioDataValue.Substring(_index + 1, _audioDataValue.Length - _index - 1);
                byte[] data = Convert.FromBase64String(_value);
                Debug.Log("已接收长度 :" + data.Length);

                if (m_currentRecorderSign == "end")
                {
                    int _audioLength = data.Length;
                    for (int i = 0; i < m_audioClipDataList.Count; i++)
                    {
                        _audioLength += m_audioClipDataList[i].Length;
                    }
                    byte[] _audioData = new byte[_audioLength];
                    Debug.Log("总长度 :" + _audioLength);
                    int _audioIndex = 0;
                    data.CopyTo(_audioData, _audioIndex);
                    _audioIndex += data.Length;
                    Debug.Log("已赋值0:" + _audioIndex);
                    for (int i = 0; i < m_audioClipDataList.Count; i++)
                    {
                        m_audioClipDataList[i].CopyTo(_audioData, _audioIndex);
                        _audioIndex += m_audioClipDataList[i].Length;
                        Debug.Log("已赋值 :" + _audioIndex);
                    }

                    WAV wav = new WAV(_audioData);
                    AudioClip _audioClip = AudioClip.Create("TestWAV", wav.SampleCount, 1, wav.Frequency, false);
                    _audioClip.SetData(wav.LeftChannel, 0);

                    /*测试音频代码
                    AudioSource _test = this.gameObject.AddComponent < AudioSource>();
                    _test.clip = _audioClip;
                    _test.Play();
                    */

                    m_audioClip = _audioClip;
                    Debug.Log("音频设置成功,已设置到unity。" + m_audioClip.length + "  " + m_audioClip.name);
                     if (onAudioClipDone != null){onAudioClipDone(m_audioClip); }

                    m_audioClipDataList.Clear();
                }
                else
                    m_audioClipDataList.Add(data);

                m_audioData = null;
            }
        }
    }
    #endregion
}
