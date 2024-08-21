package com.k2fsa.sherpa.ncnn;

public class DecoderConfig {
    String method;
    int numActivePaths;

    public DecoderConfig(String method, int numActivePaths) {
        this.method = method;
        this.numActivePaths = numActivePaths;
    }
}