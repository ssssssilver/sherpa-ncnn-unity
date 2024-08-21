using System;
using System.Runtime.InteropServices;
using System.Text;
namespace AndroidNcnn
{
    public class OnlineRecognizer : IDisposable
    {

        private HandleRef _handle;

        private const string dllName = "sherpa-ncnn-jni";

        public OnlineRecognizer(OnlineRecognizerConfig config)
        {
            IntPtr handle = CreateOnlineRecognizer(ref config);
            _handle = new HandleRef(this, handle);
        }

        public OnlineStream CreateStream()
        {
            return new OnlineStream(CreateOnlineStream(_handle.Handle));
        }

        public bool IsReady(OnlineStream stream)
        {
            return IsReady(_handle.Handle, stream.Handle) != 0;
        }

        public bool IsEndpoint(OnlineStream stream)
        {
            return IsEndpoint(_handle.Handle, stream.Handle) != 0;
        }

        public void Reset(OnlineStream stream)
        {
            Reset(_handle.Handle, stream.Handle);
        }

        public void Decode(OnlineStream stream)
        {
            Decode(_handle.Handle, stream.Handle);
        }

        public OnlineRecognizerResult GetResult(OnlineStream stream)
        {
            IntPtr result = GetResult(_handle.Handle, stream.Handle);
            OnlineRecognizerResult result2 = new OnlineRecognizerResult(result);
            DestroyResult(result);
            return result2;
        }

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        ~OnlineRecognizer()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            DestroyOnlineRecognizer(_handle.Handle);
            _handle = new HandleRef(this, IntPtr.Zero);
        }

        [DllImport(dllName, EntryPoint = "newFromFile")]
        private static extern IntPtr CreateOnlineRecognizer(ref OnlineRecognizerConfig config);

        [DllImport(dllName, EntryPoint = "DestroyRecognizer")]
        private static extern void DestroyOnlineRecognizer(IntPtr handle);

        [DllImport(dllName, EntryPoint = "CreateStream")]
        private static extern IntPtr CreateOnlineStream(IntPtr handle);

        [DllImport(dllName)]
        private static extern int IsReady(IntPtr handle, IntPtr stream);

        [DllImport(dllName)]
        private static extern int IsEndpoint(IntPtr handle, IntPtr stream);

        [DllImport(dllName)]
        private static extern void Reset(IntPtr handle, IntPtr stream);

        [DllImport(dllName)]
        private static extern void Decode(IntPtr handle, IntPtr stream);

        [DllImport(dllName)]
        private static extern IntPtr GetResult(IntPtr handle, IntPtr stream);

        [DllImport(dllName)]
        private static extern void DestroyResult(IntPtr result);
    }

    public class OnlineStream : IDisposable
    {
        private HandleRef _handle;

        private const string dllName = "sherpa-ncnn-jni";

        public IntPtr Handle => _handle.Handle;

        public OnlineStream(IntPtr p)
        {
            _handle = new HandleRef(this, p);
        }

        public void AcceptWaveform(float sampleRate, float[] samples)
        {
            AcceptWaveform(Handle, sampleRate, samples, samples.Length);
        }

        public void InputFinished()
        {
            InputFinished(Handle);
        }

        ~OnlineStream()
        {
            Cleanup();
        }

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        private void Cleanup()
        {
            DestroyOnlineStream(Handle);
            _handle = new HandleRef(this, IntPtr.Zero);
        }

        [DllImport(dllName, EntryPoint = "DestroyStream")]
        private static extern void DestroyOnlineStream(IntPtr handle);

        [DllImport(dllName)]
        private static extern void AcceptWaveform(IntPtr handle, float sampleRate, float[] samples, int n);

        [DllImport(dllName)]
        private static extern void InputFinished(IntPtr handle);
    }
    [System.Serializable]
    public class OnlineRecognizerConfig
    {
        public FeatureExtractorConfig FeatConfig;

        public ModelConfig ModelConfig;

        public DecoderConfig DecoderConfig;

        public int EnableEndpoint;

        public float Rule1MinTrailingSilence;

        public float Rule2MinTrailingSilence;

        public float Rule3MinUtteranceLength;

        [MarshalAs(UnmanagedType.LPStr)]
        public string HotwordsFile;

        public float HotwordsScore;
    }
    [System.Serializable]
    public class FeatureExtractorConfig
    {
        public float sampleRate;

        public int featureDim;
    }
    [System.Serializable]
    public class ModelConfig
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string encoderParam;

        [MarshalAs(UnmanagedType.LPStr)]
        public string encoderBin;

        [MarshalAs(UnmanagedType.LPStr)]
        public string decoderParam;

        [MarshalAs(UnmanagedType.LPStr)]
        public string decoderBin;

        [MarshalAs(UnmanagedType.LPStr)]
        public string joinerParam;

        [MarshalAs(UnmanagedType.LPStr)]
        public string joinerBin;

        [MarshalAs(UnmanagedType.LPStr)]
        public string tokens;

        public int useVulkanCompute;

        public int numThreads;
        public bool useGPU;
    }
    [System.Serializable]
    public class DecoderConfig
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string method;
        public int numActivePaths;
    }

    public class OnlineRecognizerResult
    {
        private struct Impl
        {
            public IntPtr Text;

            public IntPtr Tokens;

            public IntPtr Timestamps;

            private int Count;
        }

        private string _text;

        public string Text => _text;

        public unsafe OnlineRecognizerResult(IntPtr handle)
        {
            Impl impl = (Impl)Marshal.PtrToStructure(handle, typeof(Impl));
            int num = 0;
            byte* ptr;
            for (ptr = (byte*)(void*)impl.Text; *ptr != 0; ptr++)
            {
            }

            num = (int)(ptr - (byte*)(void*)impl.Text);
            byte[] array = new byte[num];
            Marshal.Copy(impl.Text, array, 0, num);
            _text = Encoding.UTF8.GetString(array);
        }
    }
}