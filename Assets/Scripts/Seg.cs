using UnityEngine;
using System.Collections;
using Intel.RealSense;
using Intel.RealSense.Segmentation;
using System;

public class Seg : MonoBehaviour
{

    [Header("Seg3D Settings")]
    public int segWidth = 1280;
    public int segHeight = 720;
    public float segFPS = 30f;
    public Material SegMaterial;

    private SenseManager sm = null;
    private Seg3D seg = null;
    private NativeTexturePlugin texPlugin = null;

    private System.IntPtr segTex2DPtr = System.IntPtr.Zero;

    public float distance;
    public float MaxSpeed = 0.1f;
    private Transform ThisImage = null;

    void OnFrameProcessed(System.Object sender, FrameProcessedEventArgs args)
    {
        Seg3D s = (Seg3D)sender;
        Image image = null;
        if (s != null)
        {
            image  = s.AcquireSegmentedImage();
            if (image != null)
            {
                Debug.Log("Color = " + image.Info.width + "x" + image.Info.height);
                texPlugin.UpdateTextureNative(image, segTex2DPtr);
            }
            else
            {
                Debug.Log("Image is null.");
            }
        }
        s.Dispose();
    }

    // Use this for initi alization
    void Start()
    {

        /* Create SenseManager Instance */
        sm = SenseManager.CreateInstance();

        /* Selecting a higher resolution profile*/
        StreamProfileSet profiles = new StreamProfileSet();
        profiles.color.imageInfo.width = 1280;
        profiles.color.imageInfo.height = 720;
        RangeF32 f_rate = new RangeF32(30, 30);
        profiles.color.frameRate = f_rate;
        profiles.depth.imageInfo.width = 640;
        profiles.depth.imageInfo.height = 480;
        RangeF32 f_drate = new RangeF32(30, 30);
        profiles.depth.frameRate = f_drate;

        /* Setting the resolution profile */
        sm.CaptureManager.FilterByStreamProfiles(profiles);

        /* Enable and Get a segmentation instance here for configuration */
        seg = Seg3D.Activate(sm);

        /* Subscribe to seg arrived event */
        seg.FrameProcessed += OnFrameProcessed;
        seg.OnAlert += Seg_OnAlert;

        /* Initialize pipeline */
        sm.Init();

        /* Create NativeTexturePlugin to render Texture2D natively */
        texPlugin = NativeTexturePlugin.Activate();

        //Flip the image horizontally
        //sm.CaptureManager.Device.IVCAMAccuracy = IVCAMAccuracy.IVCAM_ACCURACY_FINEST;
        //sm.CaptureManager.Device.MirrorMode = MirrorMode.MIRROR_MODE_HORIZONTAL;
        //if (distance <= 30)
        //{
        //    LoadingOverlay overlay = GameObject.Find("LoadingOverlay").gameObject.GetComponent<LoadingOverlay>();
        //    //distance = Vector3.Distance(ThisImage.position, this.transform.position);
        //    overlay.FadeIn(); 
        //}
        // Configuring the material and its texture
        SegMaterial.mainTexture = new Texture2D(segWidth, segHeight, TextureFormat.BGRA32, false); // Update material's Texture2D with enabled image size.
        SegMaterial.mainTextureScale = new Vector2(-1, -1); // Flip the image
        segTex2DPtr = SegMaterial.mainTexture.GetNativeTexturePtr();// Retrieve native Texture2D Pointer

        
        /* Start Streaming */
        sm.StreamFrames(false);

    }


    //void Awake()
    //{
    //    ThisImage = GetComponent<Transform>();

    //}
    //void Update()
    //{
    //    //ThisImage.position += ThisImage.forward * MaxSpeed;
    //    distance = Vector3.Distance(ThisImage.position, this.transform.position);
    //    LoadingOverlay overlay = GameObject.Find("LoadingOverlay").gameObject.GetComponent<LoadingOverlay>();
    //    if (distance <= 20)
    //    {
    //        //ThisImage = GetComponent<Transform>();

    //        overlay.FadeIn();
    //    }

    //    else if (distance > 20)
    //    {
    //        overlay.FadeOut();
    //    }


    //}

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
                    //distance = Vector3.Distance(ThisImage.position, this.transform.position);
                    //LoadingOverlay overlay = GameObject.Find("LoadingOverlay").gameObject.GetComponent<LoadingOverlay>();

                    Debug.Log("You are outside the ideal operating range, please move back.");
                    //overlay.FadeOut();
                    break;
                }
            case AlertEvent.ALERT_USER_IN_RANGE:
                {
                    //overlay.FadeIn();
                    break;
                }
            case AlertEvent.ALERT_BRIGHTNESS_LOW:
                {
                    Debug.Log("Low lighting conditions.");
                    break;
                }
            case AlertEvent.ALERT_BRIGHTNESS_GOOD:
                {
                    //overlay.FadeOut();
                    Debug.Log("Good lighting conditions.");
                    break;
                }
        }
    }
    // Use this for clean up
    void OnDisable()
    {

        /* Clean Up */
        seg.FrameProcessed -= OnFrameProcessed;

        if (sm != null) sm.Dispose();
    }

    
    }