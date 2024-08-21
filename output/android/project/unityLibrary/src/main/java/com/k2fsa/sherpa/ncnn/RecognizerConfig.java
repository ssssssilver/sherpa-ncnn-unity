package com.k2fsa.sherpa.ncnn;

public class RecognizerConfig {
    FeatureExtractorConfig featConfig;
    ModelConfig modelConfig;
    DecoderConfig decoderConfig;
    boolean enableEndpoint;
    float rule1MinTrailingSilence;
    float rule2MinTrailingSilence;
    float rule3MinUtteranceLength;
    String hotwordsFile;
    float hotwordsScore;

    public RecognizerConfig(FeatureExtractorConfig featConfig, ModelConfig modelConfig,
                            DecoderConfig decoderConfig, boolean enableEndpoint,
                            float rule1MinTrailingSilence, float rule2MinTrailingSilence,
                            float rule3MinUtteranceLength, String hotwordsFile, float hotwordsScore) {
        this.featConfig = featConfig;
        this.modelConfig = modelConfig;
        this.decoderConfig = decoderConfig;
        this.enableEndpoint = enableEndpoint;
        this.rule1MinTrailingSilence = rule1MinTrailingSilence;
        this.rule2MinTrailingSilence = rule2MinTrailingSilence;
        this.rule3MinUtteranceLength = rule3MinUtteranceLength;
        this.hotwordsFile = hotwordsFile;
        this.hotwordsScore = hotwordsScore;
    }
}