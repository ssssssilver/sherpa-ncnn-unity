package com.unity3d.player;

import android.Manifest;
import android.app.Activity;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.res.Configuration;
import android.media.AudioFormat;
import android.media.AudioRecord;
import android.media.MediaRecorder;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.Window;

import com.k2fsa.sherpa.ncnn.DecoderConfig;
import com.k2fsa.sherpa.ncnn.FeatureExtractorConfig;
import com.k2fsa.sherpa.ncnn.ModelConfig;
import com.k2fsa.sherpa.ncnn.RecognizerConfig;
import com.k2fsa.sherpa.ncnn.SherpaNcnn;


public class UnityPlayerActivity extends Activity implements IUnityPlayerLifecycleEvents
{
    protected UnityPlayer mUnityPlayer; // don't change the name of this variable; referenced from native code

    // Override this in your custom UnityPlayerActivity to tweak the command line arguments passed to the Unity Android Player
    // The command line arguments are passed as a string, separated by spaces
    // UnityPlayerActivity calls this from 'onCreate'
    // Supported: -force-gles20, -force-gles30, -force-gles31, -force-gles31aep, -force-gles32, -force-gles, -force-vulkan
    // See https://docs.unity3d.com/Manual/CommandLineArguments.html
    // @param cmdLine the current command line arguments, may be null
    // @return the modified command line string or null
    protected String updateUnityCommandLineArguments(String cmdLine)
    {
        return cmdLine;
    }

    // Setup activity layout
    @Override protected void onCreate(Bundle savedInstanceState)
    {
        requestWindowFeature(Window.FEATURE_NO_TITLE);
        super.onCreate(savedInstanceState);

        String cmdLine = updateUnityCommandLineArguments(getIntent().getStringExtra("unity"));
        getIntent().putExtra("unity", cmdLine);

        mUnityPlayer = new UnityPlayer(this, this);
        setContentView(mUnityPlayer);
        mUnityPlayer.requestFocus();
    }

    // When Unity player unloaded move task to background
    @Override public void onUnityPlayerUnloaded() {
        moveTaskToBack(true);
    }

    // Callback before Unity player process is killed
    @Override public void onUnityPlayerQuitted() {
    }

    @Override protected void onNewIntent(Intent intent)
    {
        // To support deep linking, we need to make sure that the client can get access to
        // the last sent intent. The clients access this through a JNI api that allows them
        // to get the intent set on launch. To update that after launch we have to manually
        // replace the intent with the one caught here.
        setIntent(intent);
        mUnityPlayer.newIntent(intent);
    }

    // Quit Unity
    @Override protected void onDestroy ()
    {
        mUnityPlayer.destroy();
        super.onDestroy();
    }

    // If the activity is in multi window mode or resizing the activity is allowed we will use
    // onStart/onStop (the visibility callbacks) to determine when to pause/resume.
    // Otherwise it will be done in onPause/onResume as Unity has done historically to preserve
    // existing behavior.
    @Override protected void onStop()
    {
        super.onStop();

        if (!MultiWindowSupport.getAllowResizableWindow(this))
            return;

        mUnityPlayer.pause();
    }

    @Override protected void onStart()
    {
        super.onStart();

        if (!MultiWindowSupport.getAllowResizableWindow(this))
            return;

        mUnityPlayer.resume();
    }

    // Pause Unity
    @Override protected void onPause()
    {
        super.onPause();

        MultiWindowSupport.saveMultiWindowMode(this);

        if (MultiWindowSupport.getAllowResizableWindow(this))
            return;

        mUnityPlayer.pause();
    }

    // Resume Unity
    @Override protected void onResume()
    {
        super.onResume();

        if (MultiWindowSupport.getAllowResizableWindow(this) && !MultiWindowSupport.isMultiWindowModeChangedToTrue(this))
            return;

        mUnityPlayer.resume();
    }

    // Low Memory Unity
    @Override public void onLowMemory()
    {
        super.onLowMemory();
        mUnityPlayer.lowMemory();
    }

    // Trim Memory Unity
    @Override public void onTrimMemory(int level)
    {
        super.onTrimMemory(level);
        if (level == TRIM_MEMORY_RUNNING_CRITICAL)
        {
            mUnityPlayer.lowMemory();
        }
    }

    // This ensures the layout will be correct.
    @Override public void onConfigurationChanged(Configuration newConfig)
    {
        super.onConfigurationChanged(newConfig);
        mUnityPlayer.configurationChanged(newConfig);
    }

    // Notify Unity of the focus change.
    @Override public void onWindowFocusChanged(boolean hasFocus)
    {
        super.onWindowFocusChanged(hasFocus);
        mUnityPlayer.windowFocusChanged(hasFocus);
    }

    // For some reason the multiple keyevent type is not supported by the ndk.
    // Force event injection by overriding dispatchKeyEvent().
    @Override public boolean dispatchKeyEvent(KeyEvent event)
    {
        if (event.getAction() == KeyEvent.ACTION_MULTIPLE)
            return mUnityPlayer.injectEvent(event);
        return super.dispatchKeyEvent(event);
    }

    public void onclick() {
        if (!isRecording) {
            initMicrophone();

            Log.i(TAG, "state: " + (audioRecord != null ? audioRecord.getState() : "null"));
            audioRecord.startRecording();
            isRecording = true;
            lastText = "";
            idx = 0;

            recordingThread = new Thread(() -> {
                model.reset(true);
                processSamples();
            }, "Audio Recording Thread");
            recordingThread.start();
            Log.i(TAG, "Started recording");
        } else {
            isRecording = false;
            audioRecord.stop();
            audioRecord.release();
            audioRecord = null;
            Log.i(TAG, "Stopped recording");
        }
    }

    public void processSamples() {
        Log.i(TAG, "processing samples");

        double interval = 0.1; // i.e., 100 ms
        int bufferSize = (int) (interval * sampleRateInHz); // in samples
        short[] buffer = new short[bufferSize];

        while (isRecording) {
            int ret = audioRecord.read(buffer, 0, buffer.length);
            if (ret > 0) {
                float[] samples = new float[ret];
                for (int i = 0; i < ret; i++) {
                    samples[i] = buffer[i] / 32768.0f;
                }
                model.acceptSamples(samples);
                while (model.isReady()) {
                    model.decode();
                }
                boolean isEndpoint = model.isEndpoint();
                String text = model.getText();
                String textToDisplay = lastText;

                if (!text.isEmpty()) {
                    if (lastText.isEmpty()) {
                        textToDisplay =  text;
                    } else {
                        textToDisplay = lastText + "\n" + text;
                    }
                }

                if (isEndpoint) {
                    model.reset();
                    if (!text.isEmpty()) {
                        lastText = lastText + "\n" +  text;
                        textToDisplay = lastText;
                        idx += 1;
                    }
                }

                 finalTextToDisplay = textToDisplay;
                //runOnUiThread(() -> textView.setText(finalTextToDisplay));
            }
        }
    }
    String finalTextToDisplay="文字";
    private Thread recordingThread;
    public String GetSpeechText()
    {
    return finalTextToDisplay;
    }

    public void initMicrophone() {
        if (checkSelfPermission(Manifest.permission.RECORD_AUDIO) != PackageManager.PERMISSION_GRANTED) {
            requestPermissions(new String[]{Manifest.permission.RECORD_AUDIO}, REQUEST_RECORD_AUDIO_PERMISSION);
        }

        int numBytes = AudioRecord.getMinBufferSize(sampleRateInHz, channelConfig, audioFormat);
        Log.i(TAG, "buffer size in milliseconds: " + (numBytes * 1000.0f / sampleRateInHz));

        audioRecord = new AudioRecord(audioSource, sampleRateInHz, channelConfig, audioFormat, numBytes * 2);

    }
    private SherpaNcnn model;
    private AudioRecord audioRecord;
    private static final int REQUEST_RECORD_AUDIO_PERMISSION = 200;
    private final boolean useGPU = true;
    private final String[] permissions = new String[]{Manifest.permission.RECORD_AUDIO};
    private static final String TAG = "sherpa-ncnn";
    private final int audioSource = MediaRecorder.AudioSource.MIC;
    private final int sampleRateInHz = 16000;
    private final int channelConfig = AudioFormat.CHANNEL_IN_MONO;
    private final int audioFormat = AudioFormat.ENCODING_PCM_16BIT;
    private int idx = 0;
    private String lastText = "";
    private volatile boolean isRecording = false;
    public void initModel() {
        FeatureExtractorConfig featConfig = new FeatureExtractorConfig(16000.0f, 80);
        // Please change the argument "type" if you use a different model
        ModelConfig modelConfig = SherpaNcnn.getModelConfig(2, useGPU);
        DecoderConfig decoderConfig = new DecoderConfig("greedy_search", 4);

        RecognizerConfig config = new RecognizerConfig(featConfig, modelConfig, decoderConfig, true, 2.0f, 0.8f, 20.0f, "", 1.5f);

        model = new SherpaNcnn(config, getApplicationContext().getAssets());
    }

    // Pass any events not handled by (unfocused) views straight to UnityPlayer
    @Override public boolean onKeyUp(int keyCode, KeyEvent event)     { return mUnityPlayer.injectEvent(event); }
    @Override public boolean onKeyDown(int keyCode, KeyEvent event)   { return mUnityPlayer.injectEvent(event); }
    @Override public boolean onTouchEvent(MotionEvent event)          { return mUnityPlayer.injectEvent(event); }
    /*API12*/ public boolean onGenericMotionEvent(MotionEvent event)  { return mUnityPlayer.injectEvent(event); }
}
