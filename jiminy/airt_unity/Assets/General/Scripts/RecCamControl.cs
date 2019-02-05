using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecCamControl : MonoBehaviour {

    ClientUnity clientUnity;
    int i = 0;

    public Toggle recording, whiteBalance, upsideDownCamera, burstMode;
    public Slider brightness, tintWB, contrast, saturation;
    public Dropdown resolutionDropDown, megapixels, sharpness, AE, photoQuality, burstSpeed;
    public Button irisaperture, Iso;
    RCamMovieFormat movieResolution;
    public GameObject aboutPanel;
    float lastTime = 0.0f, actualTime = 0.0f;
    float clampTime = 0.2f;
    public Button startRecordingButton, changeToRecordMode, changeToPhotoMode;
    float pitchValue = 0.0f, rollValue = 0.0f, yawValue = 0.0f, speedValue = 1.0f;
    public Sprite recCircle, recSquare;
    bool recordingState = true;
    public GameObject videoParametersPanel, photoParametersPanel;
    public GameObject irisScreen, isoScreen;
    //This array contains every parameter to send to the camera to get the current configuration
    RCamConfigParameter[] param = { RCamConfigParameter.RCAM_CONFIG_MOVIE_FORMAT, RCamConfigParameter.RCAM_CONFIG_PHOTO_SIZE, RCamConfigParameter.RCAM_CONFIG_WB, RCamConfigParameter.RCAM_CONFIG_ISO, RCamConfigParameter.RCAM_CONFIG_SHARPNESS, RCamConfigParameter.RCAM_CONFIG_CONTRAST, RCamConfigParameter.RCAM_CONFIG_SATURATION, RCamConfigParameter.RCAM_CONFIG_BRIGHTNESS, RCamConfigParameter.RCAM_CONFIG_PHOTO_QUALITY, RCamConfigParameter.RCAM_CONFIG_ROTATION, RCamConfigParameter.RCAM_CONFIG_IRIS, RCamConfigParameter.RCAM_CONFIG_MANUAL_WB_TINT, RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST_SPEED, RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST };
    public Image imageStateRecording;
    public Sprite videoSprite;
    public Sprite photoSprite;
    //Parameters from RCAMModule
    public static bool modeReceived = false;
    public static bool photoStateFromModule;
    public GameObject overlay;

    public static int configsReceived = 0;
    public static Dictionary<byte, int> paramDict = new Dictionary<byte, int>(); 
    void Start()
    {

        clientUnity = GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        //first we get the mode and start a coroutine to get the mode
        clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_GET_MODE);
        StartCoroutine(lookForChangeInMode());

        for (int i = 0; i < param.Length; i++)
        {
            //here we send each configuration request to the drone
            clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_GET_CONFIG, (byte)param[i]);
            paramDict.Add((byte)param[i], 0);
        }
        //And start a coroutine to get them
        StartCoroutine(lookForChangeInConfig());


    }
    IEnumerator lookForChangeInConfig()
    {
        while (configsReceived < paramDict.Count )
        {
            yield return new WaitForEndOfFrame();
        }
        //Once we get each config, we put the parameters on the gameobjects
        foreach (byte key in paramDict.Keys)
        {
            RCamConfigParameter configParameter = (RCamConfigParameter)key;
            switch (configParameter)
            {
                case RCamConfigParameter.RCAM_CONFIG_MOVIE_FORMAT:
                    resolutionDropDown.onValueChanged.RemoveAllListeners();
                    onResolutionReceived( paramDict[key]);
                    resolutionDropDown.onValueChanged.AddListener(delegate { onResolutionChanged(); });
                    break;
                case RCamConfigParameter.RCAM_CONFIG_PHOTO_SIZE:
                    megapixels.onValueChanged.RemoveAllListeners();
                    megapixels.value = paramDict[key];
                    megapixels.onValueChanged.AddListener(delegate { SendMP(); });
                    break;
                case RCamConfigParameter.RCAM_CONFIG_WB:
                    whiteBalance.onValueChanged.RemoveAllListeners();
                    if (paramDict[key] == 254)
                    {
                        whiteBalance.isOn = false;
                        tintWB.gameObject.SetActive(true);
                    }
                    else
                    {
                        whiteBalance.isOn = true;
                        tintWB.gameObject.SetActive(false);

                    }
                    whiteBalance.onValueChanged.AddListener(delegate { SendToggleManualWB(); });
                    break;
                case RCamConfigParameter.RCAM_CONFIG_ISO:
                    Iso.transform.GetChild(0).GetComponent<Text>().text = paramDict[key].ToString();
                    break;
                case RCamConfigParameter.RCAM_CONFIG_SHARPNESS:
                    sharpness.onValueChanged.RemoveAllListeners();
                    sharpness.value = paramDict[key];
                    sharpness.onValueChanged.AddListener(delegate { SendSharpness(); });
                    break;
                case RCamConfigParameter.RCAM_CONFIG_CONTRAST:
                    contrast.onValueChanged.RemoveAllListeners();
                    contrast.value = paramDict[key];
                    contrast.onValueChanged.AddListener(delegate { SendContrast(); });
                    break;
                case RCamConfigParameter.RCAM_CONFIG_SATURATION:
                    saturation.onValueChanged.RemoveAllListeners();
                    saturation.value = paramDict[key];
                    saturation.onValueChanged.AddListener(delegate { SendSaturation(); });
                    break;
                case RCamConfigParameter.RCAM_CONFIG_BRIGHTNESS:
                    brightness.onValueChanged.RemoveAllListeners();
                    brightness.value = paramDict[key];
                    brightness.onValueChanged.AddListener(delegate { SendBrightness(); });
                    break;
                case RCamConfigParameter.RCAM_CONFIG_PHOTO_QUALITY:
                    photoQuality.onValueChanged.RemoveAllListeners();
                    photoQuality.value = paramDict[key];
                    photoQuality.onValueChanged.AddListener(delegate { SendPhotoQuality(); });
                    break;
                case RCamConfigParameter.RCAM_CONFIG_ROTATION:
                    upsideDownCamera.onValueChanged.RemoveAllListeners();
                    if (paramDict[key] == 3)
                    {
                        upsideDownCamera.isOn = true;
                    }
                    else
                    {
                        upsideDownCamera.isOn = false;

                    }
                    upsideDownCamera.onValueChanged.AddListener(delegate { SendRotationChanged(); });
                    break;
                case RCamConfigParameter.RCAM_CONFIG_IRIS:
                    irisaperture.transform.GetChild(0).GetComponent<Text>().text = paramDict[key].ToString();
                    break;
                case RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST:
                    burstMode.onValueChanged.RemoveAllListeners();
                    if (paramDict[key] == 1)
                    {
                        burstMode.isOn = true;
                        burstSpeed.gameObject.SetActive(true);
                    }
                    else
                    {
                        burstMode.isOn = false;
                        burstSpeed.gameObject.SetActive(true);

                    }
                    burstMode.onValueChanged.AddListener(delegate { sendPhotoBurstMode(); });
                    break;
               
                case RCamConfigParameter.RCAM_CONFIG_MANUAL_WB_TINT:
                    tintWB.onValueChanged.RemoveAllListeners();
                    tintWB.value = paramDict[key];
                    tintWB.onValueChanged.AddListener(delegate { SendTintWB(); });
                    break;
                case RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST_SPEED:
                    burstSpeed.onValueChanged.RemoveAllListeners();
                    burstSpeed.value = paramDict[key];
                    burstSpeed.onValueChanged.AddListener(delegate { SendBurstSpeed(); });
                    break;

                default:
                    break;
            }
        }
        paramDict.Clear();
        configsReceived = 0;
    }



    IEnumerator lookForChangeInMode()
    {
        //same as above for mode
        while (!modeReceived)
        {
            yield return new WaitForEndOfFrame();
        }
        if (photoStateFromModule)
        {
            recordingState = false;
            changeToPhotoMode.gameObject.SetActive(false);
            changeToRecordMode.gameObject.SetActive(true);
            videoParametersPanel.SetActive(false);
            photoParametersPanel.SetActive(true);
            imageStateRecording.sprite = photoSprite;
            changeToRecordMode.GetComponent<Image>().color = new Color(1, 1, 1, 1);

            changeToPhotoMode.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);


        }
        else
        {
            recordingState = true;
            changeToRecordMode.gameObject.SetActive(false);
            changeToPhotoMode.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            changeToPhotoMode.gameObject.SetActive(true);
            photoParametersPanel.SetActive(false);
            videoParametersPanel.SetActive(true);
            imageStateRecording.sprite = videoSprite;

            changeToRecordMode.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);

        }
        modeReceived = false;

    }
   
    void Update()
    {
        if (overlay != null && clientUnity != null && clientUnity.client != null && clientUnity.client.isConnected)
            overlay.SetActive(false);
        else if (overlay)
            overlay.SetActive(true);
    }
    //Sends recording state or photo state to the camera
    public void sendRecordingState()
    {
        if (recording.isOn)
        {
            clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SWITCH_TO_REC );

        }
        else
        {
            clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SWITCH_TO_STILL);

        }
    }
    //Sends recording state or photo state to the camera
    public void activateRecordingOrPhoto()
    {
        if (recordingState)
        {
            recordingState = false;
            changeToPhotoMode.gameObject.SetActive(false);
            changeToRecordMode.gameObject.SetActive(true);
            videoParametersPanel.SetActive(false);
            photoParametersPanel.SetActive(true);

            changeToRecordMode.GetComponent<Image>().color = new Color(1, 1, 1, 1);

            changeToPhotoMode.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
            imageStateRecording.sprite = photoSprite;

            clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SWITCH_TO_STILL);


        }
        else
        {
            recordingState = true;
            changeToRecordMode.gameObject.SetActive(false);
            changeToPhotoMode.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            changeToPhotoMode.gameObject.SetActive(true);
            photoParametersPanel.SetActive(false);
            videoParametersPanel.SetActive(true);

            changeToRecordMode.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
            imageStateRecording.sprite = videoSprite;

            clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SWITCH_TO_REC);

        }
    }
    //Changes the resolution to the one received. The enumerator values are not sequential, so a fix has to be made
    public void onResolutionReceived(int value)
    {


        RCamMovieFormat index = (RCamMovieFormat)value;
        switch (index)
        {
            case RCamMovieFormat.RCAM_MOVIE_FORMAT_4KP25:
                resolutionDropDown.value = 0;
                break;
            case RCamMovieFormat.RCAM_MOVIE_FORMAT_1080P50:
                resolutionDropDown.value = 1;

                break;
            case RCamMovieFormat.RCAM_MOVIE_FORMAT_1080P25:
                resolutionDropDown.value = 2;

                break;
            case RCamMovieFormat.RCAM_MOVIE_FORMAT_720P50:
                resolutionDropDown.value = 3;

                break;
            case RCamMovieFormat.RCAM_MOVIE_FORMAT_WVGAP25:
                resolutionDropDown.value = 4;

                break;
            case RCamMovieFormat.RCAM_MOVIE_FORMAT_2160P25:
                resolutionDropDown.value = 5;

                break;
            case RCamMovieFormat.RCAM_MOVIE_FORMAT_1440P25:
                resolutionDropDown.value = 6;

                break;
            case RCamMovieFormat.RCAM_MOVIE_FORMAT_S1920P25:
                resolutionDropDown.value = 7;

                break;
            default:
                break;
        }

       
    }
    //Changes the resolution and sends it
    public void onResolutionChanged()
    {
        

        ChangePointType.resolution index = (ChangePointType.resolution)resolutionDropDown.value;
        switch (index)
        {
            case ChangePointType.resolution.RCAM_MOVIE_FORMAT_4KP25:
                movieResolution = RCamMovieFormat.RCAM_MOVIE_FORMAT_4KP25;
                break;
            case ChangePointType.resolution.RCAM_MOVIE_FORMAT_1080P50:
                movieResolution = RCamMovieFormat.RCAM_MOVIE_FORMAT_1080P50;
                break;
            case ChangePointType.resolution.RCAM_MOVIE_FORMAT_1080P25:
                movieResolution = RCamMovieFormat.RCAM_MOVIE_FORMAT_1080P25;
                break;
            case ChangePointType.resolution.RCAM_MOVIE_FORMAT_720P50:
                movieResolution = RCamMovieFormat.RCAM_MOVIE_FORMAT_720P50;
                break;
            case ChangePointType.resolution.RCAM_MOVIE_FORMAT_WVGAP25:
                movieResolution = RCamMovieFormat.RCAM_MOVIE_FORMAT_WVGAP25;
                break;
            case ChangePointType.resolution.RCAM_MOVIE_FORMAT_2160P25:
                movieResolution = RCamMovieFormat.RCAM_MOVIE_FORMAT_2160P25;
                break;
            case ChangePointType.resolution.RCAM_MOVIE_FORMAT_1440P25:
                movieResolution = RCamMovieFormat.RCAM_MOVIE_FORMAT_1440P25;
                break;
            case ChangePointType.resolution.RCAM_MOVIE_FORMAT_S1920P25:
                movieResolution = RCamMovieFormat.RCAM_MOVIE_FORMAT_S1920P25;
                break;
            default:
                break;
        }
        sendResolution();
    }
    public void sendResolution()
    {
        clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_MOVIE_FORMAT, (byte)movieResolution);

    }
    //Sends the photo resolution when in modo photo
    public void SendMP()
    {
        clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_SIZE, (byte)megapixels.value);

    }
    //Sends brightness
    public void SendBrightness()
    {


        if (((Time.time - lastTime) > clampTime))
        {
            lastTime = Time.time;
            byte[] brightnessArray = BitConverter.GetBytes((int)brightness.value);
            clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_BRIGHTNESS, brightnessArray[0], brightnessArray[1], brightnessArray[2], brightnessArray[3]);
            //UnityEngine.Debug.Log("Changed! " + i++);
        }
       

    }
    //Sends if the white balance needs to be manual or automatic
    public void SendToggleManualWB()
    {
        if (!whiteBalance.isOn)
        {
            tintWB.gameObject.SetActive(true);
            clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_WB, (byte)254);
        }
        else
        {
            tintWB.gameObject.SetActive(false);

            clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_WB, (byte)0);

        }


    }
    //Sends the white balance
    public void SendTintWB()
    {

        if (((Time.time - lastTime) > clampTime))
        {
            lastTime = Time.time;
            clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_MANUAL_WB_TINT, (byte)tintWB.value);
            //UnityEngine.Debug.Log("Changed! " + i++);
        }



    }
    //Sends sharpness
    public void SendSharpness()
    {
        clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_SHARPNESS, (byte)sharpness.value);



    }
    //Sends contrast
    public void SendContrast()
    {
        if (((Time.time - lastTime) > clampTime))
        {
            lastTime = Time.time;
            byte[] contrastArray = BitConverter.GetBytes((int)contrast.value);
            clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_CONTRAST, contrastArray[0], contrastArray[1], contrastArray[2], contrastArray[3]);
            //UnityEngine.Debug.Log("Changed! " + i++);
        }


    }
    //Sends AE
    public void SendAE()
    {
        clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_AE_METER_MODE, (byte)AE.value);

    }
    public void SendSaturation()
    {
        if (((Time.time - lastTime) > clampTime))
        {
            lastTime = Time.time;
            byte[] saturationArray = BitConverter.GetBytes((int)saturation.value);
            clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_SATURATION, saturationArray[0], saturationArray[1], saturationArray[2], saturationArray[3]);
            //UnityEngine.Debug.Log("Changed! " + i++);
        }

    }
    public void SendPhotoQuality()
    {
        clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_QUALITY, (byte)photoQuality.value);

    }
    public void SendRotationChanged()
    {
        if (upsideDownCamera.isOn)
        {
            clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_ROTATION, (byte)3);
        }
        else
        {
            clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_ROTATION, (byte)0);

        }

    }

    //For burst mode always enable it first and put a speed before activating it 
    public void sendPhotoBurstMode()
    {
        if (burstMode.isOn)
        {
            clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST, (byte)1);
        }
        else
        {
            clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST, (byte)0);

        }
    }
    public void SendBurstSpeed()
    {
        clientUnity.client.sendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_SET_CONFIG, (byte)RCamConfigParameter.RCAM_CONFIG_PHOTO_BURST_SPEED, (byte)burstSpeed.value);

    }
    //starts recording or stops and changes the graphics or takes a photo or starts and stops burst mode
    public void startRecording()
    {
        if (recordingState)
        {
            if (startRecordingButton.transform.GetChild(0).GetComponent<Image>().sprite == recSquare)
            {
                startRecordingButton.transform.GetChild(0).GetComponent<Image>().sprite = recCircle;
                startRecordingButton.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);

                clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_STOP_REC);

            }
            else
            {
                startRecordingButton.transform.GetChild(0).GetComponent<Image>().sprite = recSquare;
                startRecordingButton.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
                clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_START_REC);

            }
        }
        else
        {
            if (burstMode.isOn)
            {
                if (startRecordingButton.transform.GetChild(0).GetComponent<Image>().sprite == recSquare)
                {
                    startRecordingButton.transform.GetChild(0).GetComponent<Image>().sprite = recCircle;
                    startRecordingButton.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);

                    clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_BURST_CAPTURE_CANCEL);

                }
                else
                {
                    startRecordingButton.transform.GetChild(0).GetComponent<Image>().sprite = recSquare;
                    startRecordingButton.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
                    clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_BURST_CAPTURE_START);

                }
            }
            else
            {
                clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_CAPTURE);

            }
        }


       


    }
    //Takes photo
    public void startCapturing()
    {
        clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_CAPTURE);

    }
    //Burst mode
    public void startBurstPhoto()
    {
        clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_BURST_CAPTURE_START);

    }
    //Stops recording
    public void stopRecording()
    {
        clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_STOP_REC);

    }
    public void stopBurstPhoto()
    {
        clientUnity.client.SendCommand((byte)Modules.RCAM_MODULE, (byte)RCamCommandType.RCAM_BURST_CAPTURE_CANCEL);

    }
    //Closes the reccam window
    public void Close()
    {
        this.gameObject.SetActive(false);
    }

    public void goToIrisScreen()
    {
        irisScreen.SetActive(true);

    }
    public void closeIrisScreen()
    {
        irisScreen.SetActive(false);
    }
    public void goToIsoScreen()
    {
        isoScreen.SetActive(true);

    }
    public void closeIsoScreen()
    {
        isoScreen.SetActive(false);
    }

}


