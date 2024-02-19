# Hardware-AI (Nigel)

## Current progress

What's working
<ol>
    <li> 
        FCN/ MLP Classifier of 35 frames window, just Acc_XYZ, Gyr_XYZ totalling (25+10)*6 = 210 features that outputs all actions + logout
        Accuracy 100% on test data, see in runs folder
        Current architecture is 210-264-264-9, for {input} -> {hidden} -> {hidden} -> {output}. ReLU is used for each hidden layer except for output.
    </li>
    <li> Action-detection algorithm framework in `action_detection`. Current threshold uses the variation of accelereation across a 25-frame window, in contrast to the 50 frame window we use in the classifier. Seems to identify actions well, sliding window + buffer collection explained here</li>
</ol>

TODO
<ol>
    <li> Collect more data but keep existing MLP infrastructure </li>
    <li> Re-train with more data</li>
</ol>

## File-directory

<li> hwai_notes.ipynb </li>

> Documentation most of my work in training the model and collecting data, good starting point for overview

<li> integration </li>

> Containing the latest and tested code our group is using. Should be synced up with the ultra96 too

> the bitstreams will be found in `bitstreams` folder

<li> action_detection </li>

> Implementation of action detection, particularly classifying dynamic (moving) vs static (walking/ briskwalk) actions

<li> data </li>

> Raw data

> latest X_train, y_train can be found here as well

<li> feature_engineering </li>

> Engineering features to better describe the data

<li> hls </li>

> High-level synthesis files to generate IP for Vivado's block diagram, including inference c++ files, headers and weights 

<li> md-images </li>

> Image repo for the notebooks

<li> model_param </li>

> Trained weights, the files here can be easily generated with hwai_notes notebook file

<li> runs </li>

> jupyter notebook outputs of various model runs
