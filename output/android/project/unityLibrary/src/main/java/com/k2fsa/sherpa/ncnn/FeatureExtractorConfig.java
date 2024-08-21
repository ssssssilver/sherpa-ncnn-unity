package com.k2fsa.sherpa.ncnn;

public class FeatureExtractorConfig {
    float sampleRate;
    int featureDim;

    public FeatureExtractorConfig(float sampleRate, int featureDim) {
        this.sampleRate = sampleRate;
        this.featureDim = featureDim;
    }
}
