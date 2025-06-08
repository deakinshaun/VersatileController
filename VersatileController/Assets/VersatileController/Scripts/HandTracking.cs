using System;
using System.Globalization;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UI;

// Based on resources provided at: https://huggingface.co/unity/sentis-blaze-hand
public static class BlazeUtils
{
    // matrix utility
    public static float2x3 mul(float2x3 a, float2x3 b)
    {
        return new float2x3(
            a[0][0] * b[0][0] + a[1][0] * b[0][1],
            a[0][0] * b[1][0] + a[1][0] * b[1][1],
            a[0][0] * b[2][0] + a[1][0] * b[2][1] + a[2][0],
            a[0][1] * b[0][0] + a[1][1] * b[0][1],
            a[0][1] * b[1][0] + a[1][1] * b[1][1],
            a[0][1] * b[2][0] + a[1][1] * b[2][1] + a[2][1]
        );
    }

    public static float2 mul(float2x3 a, float2 b)
    {
        return new float2(
            a[0][0] * b.x + a[1][0] * b.y + a[2][0],
            a[0][1] * b.x + a[1][1] * b.y + a[2][1]
        );
    }

    public static float2x3 RotationMatrix(float theta)
    {
        var sinTheta = math.sin(theta);
        var cosTheta = math.cos(theta);
        return new float2x3(
            cosTheta, -sinTheta, 0,
            sinTheta, cosTheta, 0
        );
    }

    public static float2x3 TranslationMatrix(float2 delta)
    {
        return new float2x3(
            1, 0, delta.x,
            0, 1, delta.y
        );
    }

    public static float2x3 ScaleMatrix(float2 scale)
    {
        return new float2x3(
            scale.x, 0, 0,
            0, scale.y, 0
        );
    }

    // model filtering utility
    static FunctionalTensor ScoreFiltering(FunctionalTensor rawScores, float scoreThreshold)
    {
        return Functional.Sigmoid(Functional.Clamp(rawScores, -scoreThreshold, scoreThreshold));
    }

    public static (FunctionalTensor, FunctionalTensor, FunctionalTensor) ArgMaxFiltering(FunctionalTensor rawBoxes, FunctionalTensor rawScores)
    {
        var detectionScores = ScoreFiltering(rawScores, 100f); // (1, 2016, 1)
        var bestScoreIndex = Functional.ArgMax(rawScores, 1).Squeeze();

        var selectedBoxes = Functional.IndexSelect(rawBoxes, 1, bestScoreIndex).Unsqueeze(0); // (1, 1, 16)
        var selectedScores = Functional.IndexSelect(detectionScores, 1, bestScoreIndex).Unsqueeze(0); // (1, 1, 1)

        return (bestScoreIndex, selectedScores, selectedBoxes);
    }

    // image transform utility
    static ComputeShader s_ImageTransformShader = Resources.Load<ComputeShader>("ImageTransform");
    static int s_ImageSample = s_ImageTransformShader.FindKernel("ImageSample");
    static int s_Optr = Shader.PropertyToID("Optr");
    static int s_X_tex2D = Shader.PropertyToID("X_tex2D");
    static int s_O_height = Shader.PropertyToID("O_height");
    static int s_O_width = Shader.PropertyToID("O_width");
    static int s_O_channels = Shader.PropertyToID("O_channels");
    static int s_X_height = Shader.PropertyToID("X_height");
    static int s_X_width = Shader.PropertyToID("X_width");
    static int s_affineMatrix = Shader.PropertyToID("affineMatrix");

    static int IDivC(int v, int div)
    {
        return (v + div - 1) / div;
    }

    public static void SampleImageAffine(Texture srcTexture, Tensor<float> dstTensor, float2x3 M)
    {
        var tensorData = ComputeTensorData.Pin(dstTensor, false);
        
        s_ImageTransformShader.SetTexture(s_ImageSample, s_X_tex2D, srcTexture);
        s_ImageTransformShader.SetBuffer(s_ImageSample, s_Optr, tensorData.buffer);
        
        s_ImageTransformShader.SetInt(s_O_height, dstTensor.shape[1]);
        s_ImageTransformShader.SetInt(s_O_width, dstTensor.shape[2]);
        s_ImageTransformShader.SetInt(s_O_channels, dstTensor.shape[3]);
        s_ImageTransformShader.SetInt(s_X_height, srcTexture.height);
        s_ImageTransformShader.SetInt(s_X_width, srcTexture.width);
        
        s_ImageTransformShader.SetMatrix(s_affineMatrix, new Matrix4x4(new Vector4(M[0][0], M[0][1]), new Vector4(M[1][0], M[1][1]), new Vector4(M[2][0], M[2][1]), Vector4.zero));
        
        s_ImageTransformShader.Dispatch(s_ImageSample, IDivC(dstTensor.shape[1], 8), IDivC(dstTensor.shape[1], 8), 1);
    }

    public static float[,] LoadAnchors(string csv, int numAnchors)
    {
        var anchors = new float[numAnchors, 4];
        var anchorLines = csv.Split('\n');

        for (var i = 0; i < numAnchors; i++)
        {
            var anchorValues = anchorLines[i].Split(',');
            for (var j = 0; j < 4; j++)
            {
                anchors[i, j] = float.Parse(anchorValues[j], CultureInfo.InvariantCulture);
            }
        }

        return anchors;
    }
}

public enum HandKeypoints { Wrist = 0, 
                            ThumbCMC, ThumbMCP, ThumbIP, ThumbTIP, // from palm, each joint outwards.
                            IndexMCP, IndexPIP, IndexDIP, IndexTIP, // from palm, each joint outwards.
                            MiddleMCP, MiddlePIP, MiddleDIP, MiddleTIP, // from palm, each joint outwards.
                            RingMCP, RingPIP, RingDIP, RingTIP, // from palm, each joint outwards.
                            PinkyMCP, PinkyPIP, PinkyDIP, PinkyTIP, // from palm, each joint outwards.    
};

public enum Fingers { Thumb, Index, Middle, Ring, Pinky };

public class HandTracking : MonoBehaviour
{
    // public HandPreview handPreview;
    // public ImagePreview imagePreview;
    // public Texture2D imageTexture;
    public ModelAsset handDetector;
    public ModelAsset handLandmarker;
    public TextAsset anchorsCSV;

    public float scoreThreshold = 0.5f;
    
    public RawImage textureDisplay;

    const int k_NumAnchors = 2016;
    float[,] m_Anchors;

    const int k_NumKeypoints = 21;
    const int detectorInputSize = 192;
    const int landmarkerInputSize = 224;

    Worker m_HandDetectorWorker;
    Worker m_HandLandmarkerWorker;
    Tensor<float> m_DetectorInput;
    Tensor<float> m_LandmarkerInput;
    Awaitable m_DetectAwaitable;

    float m_TextureWidth;
    float m_TextureHeight;

    private WebCamTexture sourceTexture;
    
    private Dictionary<Fingers, HandKeypoints []> fingerIndexes = new Dictionary<Fingers, HandKeypoints []> {
        { Fingers.Thumb, new HandKeypoints [] { HandKeypoints.ThumbCMC, HandKeypoints.ThumbMCP, HandKeypoints.ThumbIP, HandKeypoints.ThumbTIP } },
        { Fingers.Index, new HandKeypoints [] { HandKeypoints.IndexMCP, HandKeypoints.IndexPIP, HandKeypoints.IndexDIP, HandKeypoints.IndexTIP } },
        { Fingers.Middle, new HandKeypoints [] { HandKeypoints.MiddleMCP, HandKeypoints.MiddlePIP, HandKeypoints.MiddleDIP, HandKeypoints.MiddleTIP } },
        { Fingers.Ring, new HandKeypoints [] { HandKeypoints.RingMCP, HandKeypoints.RingPIP, HandKeypoints.RingDIP, HandKeypoints.RingTIP } },
        { Fingers.Pinky, new HandKeypoints [] { HandKeypoints.PinkyMCP, HandKeypoints.PinkyPIP, HandKeypoints.PinkyDIP, HandKeypoints.PinkyTIP } },
    };
    
    public async void Start()
    {
        m_Anchors = BlazeUtils.LoadAnchors(anchorsCSV.text, k_NumAnchors);

        var handDetectorModel = ModelLoader.Load(handDetector);

        // post process the model to filter scores + argmax select the best hand
        var graph = new FunctionalGraph();
        var input = graph.AddInput(handDetectorModel, 0);
        var outputs = Functional.Forward(handDetectorModel, input);
        var boxes = outputs[0]; // (1, 2016, 18)
        var scores = outputs[1]; // (1, 2016, 1)
        var idx_scores_boxes = BlazeUtils.ArgMaxFiltering(boxes, scores);
        handDetectorModel = graph.Compile(idx_scores_boxes.Item1, idx_scores_boxes.Item2, idx_scores_boxes.Item3);

        m_HandDetectorWorker = new Worker(handDetectorModel, BackendType.GPUCompute);

        var handLandmarkerModel = ModelLoader.Load(handLandmarker);
        m_HandLandmarkerWorker = new Worker(handLandmarkerModel, BackendType.GPUCompute);

        m_DetectorInput = new Tensor<float>(new TensorShape(1, detectorInputSize, detectorInputSize, 3));
        m_LandmarkerInput = new Tensor<float>(new TensorShape(1, landmarkerInputSize, landmarkerInputSize, 3));

        sourceTexture = new WebCamTexture ();
        // sourceTexture.deviceName = WebCamTexture.devices[1].name;
        sourceTexture.Play ();
        textureDisplay.texture = sourceTexture;
        
        while (true)
        {
            try
            {
                m_DetectAwaitable = Detect(sourceTexture);
                await m_DetectAwaitable;
                Debug.Log ("Completed");
            }
            catch (OperationCanceledException)
            {
                Debug.Log ("Cancelled");
                break;
            }
        }

        Debug.Log ("Disposing");
        m_HandDetectorWorker.Dispose();
        m_HandLandmarkerWorker.Dispose();
        m_DetectorInput.Dispose();
        m_LandmarkerInput.Dispose();
    }

    // int count = 500;
    // int currdev = 0;
    // void Update ()
    // {
    //     if (count < 0)
    //     {
    //         // if (!sourceTexture.isPlaying)
    //         {
    //             currdev += 1;
    //     sourceTexture.deviceName = WebCamTexture.devices[currdev % WebCamTexture.devices.Length].name;
    //     sourceTexture.Play ();
    //             Debug.Log ("Restarting camera: " + sourceTexture.deviceName + " " + currdev);
    //         }
    //         count = 500;
    //     }
    //     count--;
    // }
    
    Vector3 ImageToWorld(Vector2 position)
    {
        return (position - 0.5f * new Vector2(m_TextureWidth, m_TextureHeight)) / m_TextureHeight;
    }

    async Awaitable Detect(Texture texture)
    {
        m_TextureWidth = texture.width;
        m_TextureHeight = texture.height;

        var size = Mathf.Max(texture.width, texture.height);

        // The affine transformation matrix to go from tensor coordinates to image coordinates
        var scale = size / (float)detectorInputSize;
        var M = BlazeUtils.mul(BlazeUtils.TranslationMatrix(0.5f * (new Vector2(texture.width, texture.height) + new Vector2(-size, size))), BlazeUtils.ScaleMatrix(new Vector2(scale, -scale)));
        BlazeUtils.SampleImageAffine(texture, m_DetectorInput, M);

        m_HandDetectorWorker.Schedule(m_DetectorInput);

        var outputIdxAwaitable = (m_HandDetectorWorker.PeekOutput(0) as Tensor<int>).ReadbackAndCloneAsync();
        var outputScoreAwaitable = (m_HandDetectorWorker.PeekOutput(1) as Tensor<float>).ReadbackAndCloneAsync();
        var outputBoxAwaitable = (m_HandDetectorWorker.PeekOutput(2) as Tensor<float>).ReadbackAndCloneAsync();

        using var outputIdx = await outputIdxAwaitable;
        using var outputScore = await outputScoreAwaitable;
        using var outputBox = await outputBoxAwaitable;

        var scorePassesThreshold = outputScore[0] >= scoreThreshold;

        if (!scorePassesThreshold)
            return;

        var idx = outputIdx[0];

        var anchorPosition = detectorInputSize * new float2(m_Anchors[idx, 0], m_Anchors[idx, 1]);

        var boxCentre_TensorSpace = anchorPosition + new float2(outputBox[0, 0, 0], outputBox[0, 0, 1]);
        var boxSize_TensorSpace = math.max(outputBox[0, 0, 2], outputBox[0, 0, 3]);

        var kp0_TensorSpace = anchorPosition + new float2(outputBox[0, 0, 4 + 2 * 0 + 0], outputBox[0, 0, 4 + 2 * 0 + 1]);
        var kp2_TensorSpace = anchorPosition + new float2(outputBox[0, 0, 4 + 2 * 2 + 0], outputBox[0, 0, 4 + 2 * 2 + 1]);
        var delta_TensorSpace = kp2_TensorSpace - kp0_TensorSpace;
        var up_TensorSpace = delta_TensorSpace / math.length(delta_TensorSpace);
        var theta = math.atan2(delta_TensorSpace.y, delta_TensorSpace.x);
        var rotation = 0.5f * Mathf.PI - theta;
        boxCentre_TensorSpace += 0.5f * boxSize_TensorSpace * up_TensorSpace;
        boxSize_TensorSpace *= 2.6f;

        var origin2 = new float2(0.5f * landmarkerInputSize, 0.5f * landmarkerInputSize);
        var scale2 = boxSize_TensorSpace / landmarkerInputSize;
        var M2 = BlazeUtils.mul(M, BlazeUtils.mul(BlazeUtils.mul(BlazeUtils.mul(BlazeUtils.TranslationMatrix(boxCentre_TensorSpace), BlazeUtils.ScaleMatrix(new float2(scale2, -scale2))), BlazeUtils.RotationMatrix(rotation)), BlazeUtils.TranslationMatrix(-origin2)));
        BlazeUtils.SampleImageAffine(texture, m_LandmarkerInput, M2);

        m_HandLandmarkerWorker.Schedule(m_LandmarkerInput);

        var landmarksAwaitable = (m_HandLandmarkerWorker.PeekOutput("Identity") as Tensor<float>).ReadbackAndCloneAsync();
        using var landmarks = await landmarksAwaitable;

        Vector3 [] jointPositions = new Vector3 [k_NumKeypoints];
        for (var i = 0; i < k_NumKeypoints; i++)
        {
            var position_ImageSpace = BlazeUtils.mul(M2, new float2(landmarks[3 * i + 0], landmarks[3 * i + 1]));

            Vector3 position_WorldSpace = ImageToWorld(position_ImageSpace) + new Vector3(0, 0, landmarks[3 * i + 2] / m_TextureHeight);
            jointPositions[i] = position_WorldSpace;
        }
        
        Dictionary<Fingers, float> angles = new Dictionary<Fingers, float> ();
        // a typical minimum value for fingers in completely bent position.
        Dictionary<Fingers, float> reductionFactor = new Dictionary<Fingers, float> { { Fingers.Thumb, 0.6f }, { Fingers.Index, 0.33f }, { Fingers.Middle, 0.33f }, { Fingers.Ring, 0.35f }, { Fingers.Pinky, 0.45f } };
        foreach (Fingers finger in Enum.GetValues (typeof (Fingers)))
        {
            HandKeypoints basej = fingerIndexes[finger][0];
            HandKeypoints j1 = fingerIndexes[finger][1];
            HandKeypoints j2 = fingerIndexes[finger][2];
            HandKeypoints tip = fingerIndexes[finger][3];
            float fingerLength = (jointPositions[(int) j1] - jointPositions[(int) basej]).magnitude + 
                                 (jointPositions[(int) j2] - jointPositions[(int) j1]).magnitude +
                                 (jointPositions[(int) tip] - jointPositions[(int) j2]).magnitude;
            float baseToTip = (jointPositions[(int) tip] - jointPositions[(int) basej]).magnitude;
            angles[finger] = 1.0f - Mathf.Max (((baseToTip / fingerLength) - reductionFactor[finger]) / (1.0f - reductionFactor[finger]), 0.0f);
        }
        
        if (gameObject != null)
        {
            HandButtons hb = GetComponent <HandButtons> ();
            if (hb != null)
            {
                hb.thumb.value= angles[Fingers.Thumb];
                hb.indexFinger.value= angles[Fingers.Index];
                hb.middleFinger.value= angles[Fingers.Middle];
                hb.ringFinger.value= angles[Fingers.Ring];
                hb.littleFinger.value= angles[Fingers.Pinky];
            }
        }
        
        Debug.Log ("Detect complete");
    }

    void OnDestroy()
    {
        Debug.Log ("On destroy");
        m_DetectAwaitable?.Cancel();
        if (sourceTexture != null)
        {
            sourceTexture.Stop ();
            Destroy (sourceTexture);
        }
    }
}
