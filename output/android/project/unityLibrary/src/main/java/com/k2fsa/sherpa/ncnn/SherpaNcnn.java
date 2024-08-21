package com.k2fsa.sherpa.ncnn;

import android.content.res.AssetManager;

public class SherpaNcnn {
    private long ptr;
    private  RecognizerConfig recognizerConfig;
    static {
        System.loadLibrary("sherpa-ncnn-jni");
    }

    public SherpaNcnn(RecognizerConfig config, AssetManager assetManager) {
        recognizerConfig=config;
        if (assetManager != null) {
            ptr = newFromAsset(assetManager, config);
        } else {
            ptr = newFromFile(config);
        }
    }

    @Override
    protected void finalize() throws Throwable {
        delete(ptr);
        super.finalize();
    }

    public void acceptSamples(float[] samples) {
        acceptWaveform(ptr, samples, recognizerConfig.featConfig.sampleRate);
    }

    public boolean isReady() {
        return isReady(ptr);
    }

    public void decode() {
        decode(ptr);
    }

    public void inputFinished() {
        inputFinished(ptr);
    }

    public boolean isEndpoint() {
        return isEndpoint(ptr);
    }

    public void reset(boolean recreate) {
        reset(ptr, recreate);
    }
    public void reset() {
        reset(ptr, false);
    }

    public String getText() {
        return getText(ptr);
    }

    private native long newFromAsset(AssetManager assetManager, RecognizerConfig config);

    private native long newFromFile(RecognizerConfig config);

    private native void delete(long ptr);

    private native void acceptWaveform(long ptr, float[] samples, float sampleRate);

    private native void inputFinished(long ptr);

    private native boolean isReady(long ptr);

    private native void decode(long ptr);

    private native boolean isEndpoint(long ptr);

    private native void reset(long ptr, boolean recreate);

    private native String getText(long ptr);











    public static ModelConfig getModelConfig(int type, boolean useGPU) {
        String modelDir;
        switch (type) {
            case 0:
                modelDir = "sherpa-ncnn-2022-09-30";
                break;
            case 1:
                modelDir = "sherpa-ncnn-conv-emformer-transducer-2022-12-06";
                break;
            case 2:
                modelDir = "sherpa-ncnn-streaming-zipformer-bilingual-zh-en-2023-02-13";
                break;
            case 3:
                modelDir = "sherpa-ncnn-streaming-zipformer-en-2023-02-13";
                break;
            case 4:
                modelDir = "sherpa-ncnn-streaming-zipformer-fr-2023-04-14";
                break;
            case 5:
                modelDir = "sherpa-ncnn-streaming-zipformer-zh-14M-2023-02-23";
                break;
            case 6:
                modelDir = "sherpa-ncnn-streaming-zipformer-small-bilingual-zh-en-2023-02-16/96";
                break;
            default:
                return null; // or throw an exception if the type is not supported
        }

        return new ModelConfig(
                modelDir + "/encoder_jit_trace-pnnx.ncnn.param",
                modelDir + "/encoder_jit_trace-pnnx.ncnn.bin",
                modelDir + "/decoder_jit_trace-pnnx.ncnn.param",
                modelDir + "/decoder_jit_trace-pnnx.ncnn.bin",
                modelDir + "/joiner_jit_trace-pnnx.ncnn.param",
                modelDir + "/joiner_jit_trace-pnnx.ncnn.bin",
                modelDir + "/tokens.txt",
                1,  // Number of threads, you may want to adjust this based on the model or device capabilities
                useGPU
        );
    }
}