package com.k2fsa.sherpa.ncnn;

public class ModelConfig {
    String encoderParam;
    String encoderBin;
    String decoderParam;
    String decoderBin;
    String joinerParam;
    String joinerBin;
    String tokens;
    int numThreads;
    boolean useGPU;

    public ModelConfig(String encoderParam, String encoderBin, String decoderParam,
                       String decoderBin, String joinerParam, String joinerBin,
                       String tokens, int numThreads, boolean useGPU) {
        this.encoderParam = encoderParam;
        this.encoderBin = encoderBin;
        this.decoderParam = decoderParam;
        this.decoderBin = decoderBin;
        this.joinerParam = joinerParam;
        this.joinerBin = joinerBin;
        this.tokens = tokens;
        this.numThreads = numThreads;
        this.useGPU = useGPU;
    }
}