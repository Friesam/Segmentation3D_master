using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Intel.RealSense;
using Intel.RealSense.Segmentation;
using System;
// For each subsequent algorithm module "using Intel.RealSense.AlgorithmModule;"

public class RawStreamsController : MonoBehaviour
{

    [Header("Color Settings")]
    // These values are used for Texture Size 
    private int colorWidth = 1280;
    private int colorHeight = 720;
    private float colorFPS = 30f;
    private int depthWidth = 640;
    private int depthHeight = 480;
    public float test;
    public Material RGBMaterial;

    private SenseManager sm = null;
    private NativeTexturePlugin texPlugin = null;
    private SampleReader sampleReader = null;

    private SampleReader sampleReader2 = null;
    private System.IntPtr colorTex2DPtr = System.IntPtr.Zero;
    Seg3D seg = null;

    void OnFrameProcessed(System.Object sender, FrameProcessedEventArgs args)
    {
        Seg3D s = (Seg3D)sender;
        if (s == null)
        {
            Debug.Log("Null");
        }
        else
        {
            Image image = s.AcquireSegmentedImage();
            if (image != null)
            {
                texPlugin.UpdateTextureNative(image, colorTex2DPtr);
            }
            else
            {
                Debug.Log("Image is null.");
            }
        }
        s.Dispose();

    }


    // Use this for initialization
    void Start()
    {

        /* Create SenseManager Instance */
        sm = SenseManager.CreateInstance();
        sm.CaptureManager.Realtime = false;

        // Selecting a higher resolution profile
        StreamProfileSet profiles = new StreamProfileSet();
        profiles.color.imageInfo.width = 1280;
        profiles.color.imageInfo.height = 720;
        RangeF32 f_rate = new RangeF32(30, 30);
        profiles.color.frameRate = f_rate;
        profiles.depth.imageInfo.width = 640;
        profiles.depth.imageInfo.height = 480;
        RangeF32 f_drate = new RangeF32(30, 30);
        profiles.depth.frameRate = f_drate;

        // Setting the resolution profile
        sm.CaptureManager.FilterByStreamProfiles(profiles);
        sampleReader = SampleReader.Activate(sm);
        sampleReader2 = SampleReader.Activate(sm);
        sampleReader2.EnableStream(StreamType.STREAM_TYPE_DEPTH, 640, 480, 30);
        // Enable and Get a segmentation instance here for configuration
        seg = Seg3D.Activate(sm);

        // Initialize
        seg.FrameProcessed += OnFrameProcessed;
        seg.OnAlert += Seg_OnAlert;
        sampleReader2.SampleArrived += SampleArrived;

        /* Initialize pipeline */
        sm.Init();

      
        // Flip the image horizontally
        sm.CaptureManager.Device.IVCAMAccuracy = IVCAMAccuracy.IVCAM_ACCURACY_FINEST;
        sm.CaptureManager.Device.MirrorMode = MirrorMode.MIRROR_MODE_HORIZONTAL;

        /* Create NativeTexturePlugin to render Texture2D natively */
        texPlugin = NativeTexturePlugin.Activate();

        // Configuring the material and its texture
        RGBMaterial.mainTexture = new Texture2D(colorWidth, colorHeight, TextureFormat.BGRA32, false); // Update material's Texture2D with enabled image size.
        RGBMaterial.mainTextureScale = new Vector2(-1, -1); // Flip the image
        colorTex2DPtr = RGBMaterial.mainTexture.GetNativeTexturePtr();// Retrieve native Texture2D Pointer

        /* Start Streaming */
        sm.StreamFrames(false);

    }

    private void SampleArrived(object sender, SampleArrivedEventArgs args)
    {
        Image dimage = args.sample.Depth;

        Debug.Log("Sample Called");

        if (dimage != null)

        {
            Debug.Log("seen");
            ImageData outBuffer;
            Status acquireAccessStatus = dimage.AcquireAccess(ImageAccess.ACCESS_READ, PixelFormat.PIXEL_FORMAT_DEPTH_F32, out outBuffer);
            if (acquireAccessStatus != Status.STATUS_NO_ERROR)
            {
                Debug.Log(string.Format("Failed to acquire access to the image. Return code:{0}", acquireAccessStatus));
            }
            var dwidth = dimage.Info.width;
            var dheight = dimage.Info.height;
            var centerIndex = ((640 * 190) + 320);
            Debug.Log(" this is:" + dwidth + " " + dheight);
            Debug.Log("pitch is:" + outBuffer.pitches[0]); // why is pitch 2560 for single float?

            var dpixels = outBuffer.ToFloatArray(0, 640 * dheight);
            Debug.Log("Here is the buffer length: " + dpixels.Length);
            double result = dpixels[centerIndex];
            double tester = ((double)result - int.MinValue) / ((double)int.MaxValue - int.MinValue);
            test = (float)tester;
            Debug.Log("This is my depth value: " + test);

            dimage.ReleaseAccess(outBuffer);
        }
        else
        {
            Debug.Log("Null depthImage");
        }


    }

    private void Seg_OnAlert(object sender, Seg3D.AlertEventArgs args)
    {
        switch (args.data.label)
        {
            case AlertEvent.ALERT_USER_TOO_FAR:
                {
                    Debug.Log("You are outside the ideal operating range, please move closer.");
                    break;
                }
            case AlertEvent.ALERT_USER_TOO_CLOSE:
                {
                    Debug.Log("You are outside the ideal operating range, please move back.");
                    break;
                }
            case AlertEvent.ALERT_USER_IN_RANGE:
                {
                    Debug.Log("Ideal Segmentatation conditions!");
                    break;
                }
            case AlertEvent.ALERT_BRIGHTNESS_LOW:
                {
                    Debug.Log("Low lighting conditions.");
                    break;
                }
            case AlertEvent.ALERT_BRIGHTNESS_GOOD:
                {
                    Debug.Log("Good lighting conditions.");
                    break;
                }
        }
    }

    // Use this for clean up
    void OnDisable()
    {
        // Clean up
        seg.FrameProcessed -= OnFrameProcessed;

        if (sm != null) sm.Dispose();
    }

}