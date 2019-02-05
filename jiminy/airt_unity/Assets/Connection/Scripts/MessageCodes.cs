


// This enums define the version of atreyu. This constant should be used in Jiminy, in Atreyu and in CPack
// Update them before each release
public enum AIRTVersion : byte
{
    MAJOR = 0,
    MINOR = 2,
    PATCH = 5
};

/**
 * @brief The Modules enum specifies the type of message being sent. For example, STD_COMMANDS_MODULE refers
 * to standard commands (START, STOP, QUIT, POWEROFF) that the server can receive. STD_NOTIFICATIONS_MODULE refers
 * to information notification sent by the server to the clients, etc.
 *
 * For adding new modules (like messages regarding the managing of the ZR300 camera or Pozyx tags, etc)
 * you have to define a new Modules number.
 *
 * Using STD_COMMANDS_MODULE from the client application side means you want to address a command to all active
 * internal sources. The server will receive the STD command and will re-direct it to all active internal sources.
 *
 * STD_NOTIFICATIONS_MODULE starts with value of 128 for using the MostSignificativeBit as flag to differenciate
 * between modules and notifications. There are some functions in StdMessages.h that uses the MSB.
 *
 * AIRT Standard message format: msg{signature, module, action}
 *   signature: 1 byte ('A')
 *   module: 1 byte
 *   action/notification/data: 1 byte
 **/
public enum Modules
{
    STD_COMMANDS_MODULE = 1,  // to all active internal sources
    POINTCLOUD_MODULE,  // to this specific module
    POSITIONING_MODULE,
    RCAM_MODULE,            
    FCS_MULTIPLEXER_MODULE,  // 5
    OS_MODULE,
    ATREYU_MODULE,
    PLAN_LIBRARIAN_MODULE,
    PLAN_EXECUTOR_MODULE,   
    FPV_MODULE,                 // 10
    GIMBAL_MULTIPLEXER_MODULE,
    DRONE_MODULE,
    MAPPER_MODULE,


    STD_NOTIFICATIONS_MODULE = 128,  // This is a notification from the server
    POINTCLOUD_NOTIFICATIONS_MODULE,
    POSITIONING_NOTIFICATIONS_MODULE,   // 130
    RCAM_NOTIFICATIONS_MODULE,
    FCS_MULTIPLEXER_NOTIFICATIONS_MODULE,
    OS_NOTIFICATIONS_MODULE,
    ATREYU_NOTIFICATIONS_MODULE,        
    PLAN_LIBRARIAN_NOTIFICATIONS_MODULE,  // 135
    PLAN_EXECUTOR_NOTIFICATIONS_MODULE,
    FPV_NOTIFICATIONS_MODULE,
    GIMBAL_MULTIPLEXER_NOTIFICATIONS_MODULE,  
    DRONE_NOTIFICATIONS_MODULE,         
    MAPPER_NOTIFICATIONS_MODULE,         // 140

    MESSAGE_BUS_MODULE = 255    // Notifications addressed to this module are how modules communicate with each other.
                                // For example: {'A', MESSAGE_BUS_MODULE, PLAN_EXECUTOR_MODULE} {'A', FCS_MODULE, FCS_PING}
                                // is a message for FCS_MODULE generated as a notification from PLAN_EXECutOR_MODULE.
};

/**
 * @brief The CommandType enum are the Standandard commands the server attends.
 * You can address one of these commands to an specific module using its value in the message header.
 *   Example: msg{A, POINTCLOUD_MODULE, START}
 * You can also address a command to all:
 *   Example: msg{A, STD_COMMANDS_MODULE, START}
 */
public enum CommandType
{
    // Commands
    START = 1,  // sb asks you to start all the sources
    STOP,       // sb asks you to stop all the sources
    QUIT,       // sb asks you to quit the system
    POWEROFF,   // sb asks you to shutdown the system

    LAST_STD_COMMAND        // this is not a command. Just indicates where the specific commands start. Keep it always the last
};

public enum PointcloudCommandType
{
    PCL_START_CAPTURING = CommandType.LAST_STD_COMMAND,
    PCL_STOP_CAPTURING,

    PCL_LAST_COMMAND    // Keep it at the end
};

public enum PositioningCommandType
{
    IPS_SYSTEM = CommandType.LAST_STD_COMMAND,     // check physical connection to the local-TAG
    IPS_DISCOVER,   // find nearby tags/anchors
    IPS_AUTOCALIBRATE,  // request a calibration (get the estimated anchors locations from the response)
    IPS_UPDATE_SETTINGS,  // post an update of positioning settings (anchors locatio, tags channels, etc.)
    IPS_START_POSITIONING,
    IPS_STOP_POSITIONING,
    IPS_ANCHOR_MANUAL_CONFIG,
    IPS_ANCHOR_TOBE_AUTOCALIBRATED,
    IPS_DISCOVER_DRONETAGS,
    IPS_DRONETAGS_MANUAL_CONFIG,
    IPS_CLEAR_POZYXSETTINGS,
    IPS_GET_DRONEFILTER,
    IPS_SET_DRONEFILTER,
    IPS_GET_ANCHORS_FROM_FILE,

    IPS_LAST_COMMAND    // Keep it at the end
};

public enum RCamCommandType : byte
{
    RCAM_EMULATE_KEY = CommandType.LAST_STD_COMMAND, // emulate a key press
    RCAM_SWITCH_TO_REC ,    // change to record mode (video)
    RCAM_SWITCH_TO_STILL,   // change to photo mode
    RCAM_SWITCH_TO_PB,      // change to playback mode
    RCAM_START_REC,         // start recording
    RCAM_STOP_REC,          // stop recording
    RCAM_CAPTURE,           // capture a photo?
    RCAM_CAPTURE_AF,        // capture a photo with autofocus?
    RCAM_START_PB,          // start playback
    RCAM_STOP_PB,           // stop playback
    RCAM_PAUSE_PB,          // pause playback
    RCAM_RESUME_PB,         // resume playback
    RCAM_SET_CONFIG,        // set configuration
    RCAM_GET_CONFIG,        // | cmd | key |
    RCAM_SET_WIFI,
    RCAM_GET_WIFI,
    RCAM_GET_BATTERY,       // request battery charge
    RCAM_GET_CARD_STATUS,   // request card status
    RCAM_GET_MODE,          // request mode
    RCAM_GET_STATUS,        // request status
    RCAM_GET_REC_REMAINING, // request remaining recording time
    RCAM_GET_STILL_REMAINING, // request number of pictures left
    RCAM_FORMAT_CARD,         // format card
    RCAM_SWITCH_TO_MULTIPLE_MODE_CAPTURE, //
    RCAM_SET_X_CONFIG, //
    RCAM_GET_X_CONFIG, //
    RCAM_QUERY_IS_RECORDING, //
    RCAM_BURST_CAPTURE_START,   // start burst capture (first you have to set the RCAM_CONFIG_PHOTO_BURST to 1)
    RCAM_BURST_CAPTURE_CANCEL,  // cancel burst capture
    RCAM_CLEAR_SETTING,         // clear settings
    RCAM_INIT_PBMEDIA_FILE,     // init playback media file
    RCAM_PBMEDIA_NEXT_FILE,     // next playback file
    RCAM_PBMEDIA_PREV_FILE,     // previous playback file
    RCAM_PBMEDIA_DELETE_FILE,   // delete file
    RCAM_SHUTDOWN,              // shutdown

    RCAM_LAST_COMMAND   // Keep it at the end
};

public enum RCamKeys : byte {
    RCAM_KEY_START_BURST = 0x1,   // In photo mode, starts and stops burst capture
    RCAM_KEY_UP =  0x2, 
    RCAM_KEY_DOWN = 0x3, 
    RCAM_KEY_MENU = 0x4, 
    RCAM_KEY_REC = 0x6
};

public enum RCamConfigParameter : byte {
    RCAM_CONFIG_MOVIE_FORMAT = 0x0, // see RCamMovieFormat
    RCAM_CONFIG_PHOTO_SIZE,         // see RCamConfigPhotoSize
    RCAM_CONFIG_WB,                 // 0: auto 254: manual. For manual, use the RCAM_CONFIG_MANUAL_WB_TINT
    RCAM_CONFIG_ISO,                // see RCamConfigISO
    RCAM_CONFIG_SHARPNESS,          // 0: weak 1: normal 2: strong 
    RCAM_CONFIG_CONTRAST = 5,       // range:  0..256/1
    RCAM_CONFIG_AE_METER_MODE,      // 0: center 1: average 2: spot 
    RCAM_CONFIG_SCENE,              
    RCAM_CONFIG_DIGITAL_EFFECT,
    RCAM_CONFIG_FLICKER_REDUCTION,  // 1: 60Hz 2: 50Hz
    RCAM_CONFIG_VIDEO_SYSTEM = 10,  
    RCAM_CONFIG_WIFI_ONOFF,         // 0: 1:
    RCAM_CONFIG_EV_BIAS,            // range: -96..96/1
    RCAM_CONFIG_BATTERY,            // range: 0..100/1
    RCAM_CONFIG_SATURATION,         // range: 0..256/1
    RCAM_CONFIG_BRIGHTNESS = 15,    // range -256..256/1
    RCAM_CONFIG_NOISE_REDUCTION,    // 0: 1: 2: 
    RCAM_CONFIG_PHOTO_QUALITY,      // see RCamConfigPhotoQuality
    RCAM_CONFIG_LCD_ONOFF,          // 0: 1:
    RCAM_CONFIG_ROTATION,           // 0: normal  3: upside down
    RCAM_CONFIG_VERSION = 20,
    RCAM_CONFIG_IRIS,               // see RCamConfigIris
    RCAM_CONFIG_FOCUS_METHOD,       // 0: MF 1: AF
    RCAM_CONFIG_AF_AREA,            // range: 0..256
    RCAM_CONFIG_MAGNIFY_POSITION,   
    RCAM_CONFIG_NEW_FW = 25,
    RCAM_CONFIG_HW_VERSION,
    RCAM_CONFIG_DO_AF,
    RCAM_CONFIG_CAF_ONOFF,
    RCAM_CONFIG_LENS_ATTACHED,
    RCAM_CONFIG_LED_ENABLE = 30,    // 0: 1: 
    RCAM_CONFIG_BEEPER_ENABLE,      // 0: 1: 
    RCAM_CONFIG_AF_MODE,            // 0: 1: 
    RCAM_CONFIG_MF_DRIVE,
    RCAM_CONFIG_MODEL_NAME,
    RCAM_CONFIG_LCD_BACKLIGHT_LEVEL = 35,   // 10..100/10
    RCAM_CONFIG_PHOTO_BURST,            // 0: off 1: on 
    RCAM_CONFIG_RTC_TIME,
    RCAM_CONFIG_BT_MAC,
    RCAM_CONFIG_MAX_SHUTTER_TIME,       // 0: 1: 2: 3: 4: 5: .... 20:
    RCAM_CONFIG_PC_CONNECTED = 40,
    RCAM_CONFIG_USB_CABLE_STATUS,
    RCAM_CONFIG_OLED_ONOFF_ENABLE,
    RCAM_CONFIG_SHUTTER_ANGLE,
    RCAM_CONFIG_DCF_REACH_MAX_NUMBER,
    RCAM_CONFIG_MANUAL_WB = 45,
    RCAM_CONFIG_HDMI_OSD_ONOFF,
    RCAM_CONFIG_STILL_SHUTTER_SPEED,
    RCAM_CONFIG_LENS_ZOOM,
    RCAM_CONFIG_DCF_FILE_NUMBERING,
    RCAM_CONFIG_CVBS_VIDEO_SYSTEM = 50,
    RCAM_CONFIG_CVBS_OUTPUT_ENBLE,
    RCAM_CONFIG_LENS_FOCUS_POSITION,
    RCAM_CONFIG_LENS_FOCUS_SPEED,
    RCAM_CONFIG_MANUAL_WB_TINT,     // range: -100..100/1
    RCAM_CONFIG_CAF_RANGE = 55,
    RCAM_CONFIG_CAF_SENSITIVITY,
    RCAM_CONFIG_ENC_ROTATION,
    RCAM_CONFIG_VIDEO_QUALITY,
    RCAM_CONFIG_DUAL_STREAM_ENABLE,
    RCAM_CONFIG_PHOTO_AEB = 60,
    RCAM_CONFIG_CAPTURE_TIMESTAMP,
    RCAM_CONFIG_RECORD_TIMESTAMP,
    RCAM_CONFIG_IMU_ROTATION,
    RCAM_CONFIG_PHOTO_BURST_SPEED, // 0: L 1: L1 2: M 3: H
    RCAM_CONFIG_LUT_TYPE = 65,
    RCAM_CONFIG_DCF_LAST_FILE_NAME,
    RCAM_CONFIG_UART_COMMAND_SUPPORTED,
    RCAM_CONFIG_LCD_RUNTIME_ONOFF,
    RCAM_CONFIG_MOV_CONTAINER_ROTATION,
    RCAM_CONFIG_UI_TIMELAPSE_STATUS = 70,
    RCAM_CONFIG_USB_CHARGE_DETECTION,
    RCAM_CONFIG_USB_DEVICE_ROLE,
    RCAM_CONFIG_IC_TEMPERATURE,
    RCAM_CONFIG_DCF_NAME_MODE, // 1、ABCD0001.JPG 2、ABCD_201501011633_0001.JPG 3、ABCD_0001_201501011633.JPG
    RCAM_CONFIG_RCAM_IS_MULTIPLE = 75,
    RCAM_CONFIG_DEWARP_ONOFF,
    RCAM_CONFIG_MAX_RECORD_TEMPERATURE_LIMIT,
    RCAM_CONFIG_VIGNETTE_ONOFF,
    RCAM_CONFIG_SECONDRY_STREAM_RESOLUTION,
    RCAM_CONFIG_SECONDRY_STREAM_BITRATE = 80,
    RCAM_CONFIG_RECORD_INT_CAP,
    RCAM_CONFIG_HDMI_PREFER_FORMAT,
    RCAM_CONFIG_MULTIPLE_CONTROL_ID,
    RCAM_CONFIG_MULTIPLE_CAPTURE_DELAY,
    RCAM_CONFIG_RCAM_DEFLECTION_ANGLE = 85,
    RCAM_CONFIG_VOLUME_CONTROL,
    RCAM_CONFIG_AE_EXPOSURE_MODE,
    RCAM_CONFIG_OIS_MODE,
    RCAM_CONFIG_MOVIE_RECORD_SPLIT_DURATION,
    RCAM_CONFIG_MULTIPLE_TIMEOUT_ENABLE = 90,
    RCAM_CONFIG_MULTIPLE_CONTROL_ENABLE,
    RCAM_CONFIG_AEB_NUMCAPTURE,
    RCAM_CONFIG_LIVEVIEW_WITH_AUDIO,
    RCAM_CONFIG_SECONDRY_STREAM_GOP,
    RCAM_CONFIG_SEND_TO_LNX_STREAM  = 95,
    RCAM_CONFIG_PRIMARY_STREAM_BITRATE,
    RCAM_CONFIG_SECONDARY_AUDIO_TYPE,
    RCAM_CONFIG_SECONDARY_B_FRAME,
    RCAM_CONFIG_AELOCK,
    RCAM_CONFIG_SECONDARY_STREAM_BITRARE_TYPE = 100, // value is CBR, VBR
    RCAM_CONFIG_GROUP_INDEX,
    RCAM_CONFIG_MAX_ISO_LIMIT,
    RCAM_CONFIG_ETHERNET_IP
};

public enum RCamConfigPhotoSize {
    RCAM_PHOTO_SIZE_16M = 0x0,
    RCAM_PHOTO_SIZE_12M = 0x1,
    RCAM_PHOTO_SIZE_8M = 0x2,
    RCAM_PHOTO_SIZE_5M = 0x3,
    RCAM_PHOTO_SIZE_3M = 0x4
};

public enum RCamMovieFormat : byte {
    RCAM_MOVIE_FORMAT_4KP25 = 22,
    RCAM_MOVIE_FORMAT_1080P50 = 23,
    RCAM_MOVIE_FORMAT_1080P25 = 24,
    RCAM_MOVIE_FORMAT_720P50 = 25,
    RCAM_MOVIE_FORMAT_WVGAP25 = 26,
    RCAM_MOVIE_FORMAT_2160P25 = 35,
    RCAM_MOVIE_FORMAT_1440P25 = 41,
    RCAM_MOVIE_FORMAT_S1920P25 = 47,
};

public enum RCamConfigISO {
    RCAM_ISO_AUTO = 0,
    RCAM_ISO_100,
    RCAM_ISO_125,
    RCAM_ISO_160,
    RCAM_ISO_200,
    RCAM_ISO_250,
    RCAM_ISO_320,
    RCAM_ISO_400,
    RCAM_ISO_500,
    RCAM_ISO_640,
    RCAM_ISO_800,
    RCAM_ISO_1000,
    RCAM_ISO_1250,
    RCAM_ISO_1600,
    RCAM_ISO_2000,
    RCAM_ISO_2500,
    RCAM_ISO_3200,
    RCAM_ISO_4000,
    RCAM_ISO_5000,
    RCAM_ISO_6400,
    RCAM_ISO_8000,
    RCAM_ISO_10000,
    RCAM_ISO_12800,
    RCAM_ISO_16000,
    RCAM_ISO_20000,
    RCAM_ISO_25600,
};

public enum RCamConfigPhotoQuality {
    RCAM_PHOTO_QUALITY_BASIC = 0,
    RCAM_PHOTO_QUALITY_FINE,
    RCAM_PHOTO_QUALITY_SUPER_FINE,
    RCAM_PHOTO_QUALITY_JPG_PLUS_DNC
};

public enum RCamConfigIris {
    RCAM_IRIS_F0_7 = 0,
    RCAM_IRIS_F0_8,
    RCAM_IRIS_F0_9,
    RCAM_IRIS_F1,
    RCAM_IRIS_F1_1,
    RCAM_IRIS_F1_2,
    RCAM_IRIS_F1_4,
    RCAM_IRIS_F1_6,
    RCAM_IRIS_F1_8,
    RCAM_IRIS_F2,
    RCAM_IRIS_F2_2,
    RCAM_IRIS_F2_5,
    RCAM_IRIS_F2_8,
    RCAM_IRIS_F3_2,
    RCAM_IRIS_F3_5,
    RCAM_IRIS_F4,
    RCAM_IRIS_F4_5 = 0x10,
    RCAM_IRIS_F5,
    RCAM_IRIS_F5_6,
    RCAM_IRIS_F6_3,
    RCAM_IRIS_F7_1,
    RCAM_IRIS_F8,
    RCAM_IRIS_F9,
    RCAM_IRIS_F10,
    RCAM_IRIS_F11,
    RCAM_IRIS_F13,
    RCAM_IRIS_F14,
    RCAM_IRIS_F16,
    RCAM_IRIS_F18,
    RCAM_IRIS_F20,
    RCAM_IRIS_F22,
    RCAM_IRIS_F25,
    RCAM_IRIS_F29 = 0x20,
    RCAM_IRIS_F32,
    RCAM_IRIS_F36,
    RCAM_IRIS_F40,
    RCAM_IRIS_F45,
    RCAM_IRIS_F51,
    RCAM_IRIS_F57,
    RCAM_IRIS_F64,
    RCAM_IRIS_F72,
    RCAM_IRIS_F80,
    RCAM_IRIS_F90
};


public enum RCamConfigFocusMethod {
    RCAM_FOCUS_MF = 0,
    RCAM_FOCUS_AF
};

public enum FCSMultiplexerCommandType : byte
{
    FCS_PING = CommandType.LAST_STD_COMMAND,    // Ask for a pong from the multiplexer
    FCS_VERSION,    // Asks for the multiplexer version
    FCS_CLEARALL,   // Clear all waypoints in the flight plan
    FCS_SETMODE,    // Sets the flight mode
    FCS_GOTO,       // On guided mode, set go to the provided position
    FCS_WP,         // Add a waypoint to the flight plan
    FCS_ARM,        // Arm the drone (the motors will start to spin)
    FCS_DISARM,     // Disarm the drom (the motors will stop)
    FCS_TAKEOFF,    // Take off
    FCS_LAND,       // Land
    FCS_POSITIONON, // Start broadcasting the FCS position
    FCS_POSITIONOFF,// Stop broadcasting the FCS position
    FCS_SETSPEED,   // Set speed
    FCS_CREATEMISSION,  // Creates a mission

    FCS_LAST_COMMAND    // Keep it at the end
};

public enum FCSFlightModes : byte {
    MODE_STABILIZE = 0,
    MODE_ACRO,
    MODE_ALTHOLD,
    MODE_AUTO,
    MODE_GUIDED,
    MODE_LOITER,
    MODE_RTL,
    MODE_CIRCLE,
    MODE_CIRCLE2,
    MODE_LAND,
    MODE_LAND2,
    MODE_DRIFT,
    MODE_DRIFT2,
    MODE_SPORT
};

public enum OSCommandType
{
    OS_QUERY_AVAILABLE_DISKSPACE = CommandType.LAST_STD_COMMAND,  // how much space is available in disk in the OCS?
    OS_QUERY_SYSTEM_INFO,   // Queries information about memory usage, load, etc.
    OS_CREATE_FILE,         // Creates a new file with a given name. If it already exists, delete its contents
    OS_APPEND_DATA_FILE,    // Appends data to a file (if it does not exists, it creates it)
    OS_DELETE_FILE,         // Deletes a file
    OS_REQUEST_FILE_CONTENT,    // Requests a chunk of the contents of a file in the server
    OS_REQUEST_FILE_SIZE,   // Request the size of the given file in the server
    OS_QUERY_NETWORK_STATS, // Request stats about the network interface statistics (read, sent bytes)
    OS_ACCEPT_BLOB,         // Accept a blob of data (used for bandwidth measures)
    OS_REQUEST_BLOB,         // Send me a blob of data (used for bandwidth measures)

    OS_LAST_COMMAND    // Keep it at the end
};


public enum AtreyuCommandType
{
    ATREYU_QUERY_SERVER_VERSION = CommandType.LAST_STD_COMMAND,  // what is the server version running in the OCS?
    ATREYU_QUERY_UPDATE_PACKAGE_PATHNAME,   // what pathname (dir + filename) should have the server update deb file?
    ATREYU_QUERY_SYSTEM_STATE,           // current status of the system (see AtreyuSystemState)
    ATREYU_ENTER_RECORDING_STATE,       // configure the system to start recording. If the current state is other than IDLE_STATE, it returns an error
    ATREYU_ENTER_MAPPING_STATE,     // configure the system to start mapping. If the current state is other than IDLE_STATE, it returns an error

    /// The following command is mostly for testing!!!
    ATREYU_RELAY_NOTIFICATION,  // Given a message with n parts, it will broadcast the parts 1..<n-1> as it they were a notification from a module

    ATREYU_LAST_COMMAND    // Keep it at the end
};


public enum AtreyuSystemState : byte {
    IDLE_STATE,
    MAPPING_STATE,
    RECORDING_STATE,
};

public enum PlanLibrarianCommandType
{
    PLAN_LIB_REQUEST_NUMBER_PLANS_IN_DDBB = CommandType.LAST_STD_COMMAND,  // Returns the number of plans stored in the server
    PLAN_LIB_REQUEST_BASE_NAME,     // Given a plan index (0..numberOfPlans-1), return the mission guid
    PLAN_LIB_REQUEST_THUMBNAIL,    // Given a mission guid, it returns its thumbnail's filename
    PLAN_LIB_REQUEST_MISSION,          // Given a mission guid, it returns the mission' filename (the json file)
    PLAN_LIB_REQUEST_MAP,           // Given a mission guid, it returns its map's filename (either a 3d file or the point cloud file)
    PLAN_LIB_REQUEST_METADATA,      // Given a mission guid, it returns its metada filename
    PLAN_LIB_REQUEST_LIBRARY_PATH,       // returns the path in the server for the library
    PLAN_LIB_DELETE_MISSION,        // Given a mission guid, it deletes it from the server

    PLAN_LIB_LAST_COMMAND    // Keep it at the end
};

public enum PlanExecutorCommandType
{
    PLAN_EXEC_LOAD_PLAN = CommandType.LAST_STD_COMMAND,  // Loads the given plan for execution
    PLAN_EXEC_START,    // Start the mission. It performs preflight checks and issues a ready to fly or preflight tests failed notification
    PLAN_EXEC_TAKEOFF,  // Arm the drone, wait a specified period and start the flight plan
    PLAN_EXEC_STOP,   // Aborts the mission. It is necessary to start from the beginning. If flying, it lands immediately!
    PLAN_EXEC_PAUSE,    // If flying, it sets the drone to loiter mode
    PLAN_EXEC_RESUME,
    PLAN_EXEC_SET_REGISTRATION_MATRIX,  // Sets the matrix that transform
    PLAN_EXEC_EXIT, // Returns atreyu to IDLE_STATE
    PLAN_EXEC_REQUEST_CURRENT_FLIGHT_PLAN,  // returns the current mission and the plan index

    PLAN_EXEC_LAST_COMMAND    // Keep it at the end
};

public enum FPVCommandType
{
    FPV_GET_RESOLUTION = CommandType.LAST_STD_COMMAND,
    FPV_START_STREAMING,
    FPV_STOP_STREAMING,
    FPV_LAST_COMMAND
};

public enum GimbalMultiplexerCommandType : byte
{
    GIMBAL_GOTO_ANGLE = CommandType.LAST_STD_COMMAND, // Goto to the given angle (pitch, roll, yaw)
    GIMBAL_SET_SPEED_DEPRECATED,    // Set the speed in degrees per second
    GIMBAL_MOVE_PITCH,
    GIMBAL_MOVE_ROLL,
    GIMBAL_MOVE_YAW,
    GIMBAL_STOP_PITCH,
    GIMBAL_STOP_ROLL,
    GIMBAL_STOP_YAW,
    GIMBAL_STOP_ALL,
    GIMBAL_RC_MODE,
    GIMBAL_RC_FIX_MODE,
    GIMBAL_RC_NOROLL_MODE,
    GIMBAL_GOTO_ZERO,        // Return to the zero position
    GIMBAL_SET_YAW_OFFSET,

    GIMBAL_LAST_COMMAND     // Keep it at the end
};


public enum DroneCommandType : byte
{
    DRONE_SET_ZCAMERA_OFFSET = CommandType.LAST_STD_COMMAND, // Sets the position of the depth camera wrt the drone
    DRONE_GET_ZCAMERA_OFFSET,

    DRONE_LAST_COMMAND
};


public enum MapperCommandType : byte 
{
    MAPPER_CREATE_MAP = CommandType.LAST_STD_COMMAND,  // Creates a new map for mapping
    MAPPER_LOAD_MAP,    // Loads an existing map

    MAPPER_START_MAPPING,     // Start recording (adding new clouds into the map)
    MAPPER_PAUSE_MAPPING,       // Pause mapping (stop adding new clouds into the map)
    MAPPER_RESUME_MAPPING,    // Resume mapping
    MAPPER_DONE_MAPPING,      // Stop mapping, save the point clouds and return to idle state
    MAPPER_GET_NUMBER_OF_POINTCLOUDS,   // Requests the number of point clouds in the current map
    MAPPER_GET_POINTCLOUDS_IDS,     // Requests the list of ids in the database
    MAPPER_GET_POINTCLOUD,      // Requests the given point cloud
    MAPPER_GET_ALL_POINTCLOUDS, // Requests all the point clouds in the system (use with caution. Probably PAUSE the recording before)
    MAPPER_DELETE_POINT_CLOUD,  // Deletes the given point cloud  
    MAPPER_DELETE_ALL_POINT_CLOUDS,     // Discard all the captured clouds in the current map
    MAPPER_REQUEST_CURRENT_GUID,        // Request the current guid
    MAPPER_REQUEST_CURRENT_STATE,        // Request the current status (it will return a 
                                        // MAPPER_IDLE_NOTIFICATION, MAPPER_MAP_READY_NOTIFICATION, MAPPER_STARTED_MAPPING_NOTIFICATION or MAPPER_PAUSE_MAPPING_NOTIFICATION,
    MAPPER_LAST_COMMAND
};


/**
 * @brief The NotificationType enum contains common notifications that the internal sources can
 * respond (STARTED, STOPPED, QUITTING)
 * It also contains noifications the server can respond directly to the client for maintaining
 * the connection (ACK, HEART_BEAT, POWERING_OFF, UNKNOWN_COMMAND, UNDEFINED_MODULE).
 *
 * From the client application side you can determine the source module inspecting the 2nd byte
 * Example: if( msg == msg{A, POSITIONING_NOTIFICATIONS_MODULE, STARTED} )
 */
public enum NotificationType
{
    // Notifications
    ACK = 1,  // message received. This should always be the first notification!!!
    HEART_BEAT, // I'm still alive
    STARTED,    // I started
    STOPPED,    // I stopped
    QUITTING,   // I quitted
    POWERING_OFF, // The system is powering down
    UNKNOWN_COMMAND,    // I didn't understand your last message
    UNDEFINED_MODULE,     // I didn't find the last module you asked. (It may not exist or It has not been defined in the atreyu-ini file)
    INVALID_STATUS,  // The requested command is not valid in the current state

    LAST_STD_NOTIFICATION_TYPE // Not a real notification, just used for marking the last used code. Keep it at the end
};

public enum PointcloudNotificationType
{
    PCL_SINGLE = NotificationType.LAST_STD_NOTIFICATION_TYPE,  // when data is sent this will be the 3rd byte of the header
    PCL_BLOCK,
    PCL_CAPTURING_STARTED,
    PCL_CAPTURING_STOPPED,
    PCL_LAST_COMMAND_HAS_FAILED,

    PCL_LAST_NOTIFICATION    // Keep it at the end
};

public enum PositioningNotificationType
{
    IPS_LOCALTAG_CONNECTED =  NotificationType.LAST_STD_NOTIFICATION_TYPE,  // local-TAG working properly
    IPS_DISCOVERED,  // nearby tags/anchors found correctly
    IPS_AUTOCALIBRATED,  // autocalibration performed correctly
    IPS_UPDATED_SETTINGS,  // pozyx-server has accepted the new settings
    IPS_DATA,  // when data is sent this will be the 3rd byte of the header
    IPS_DATA_ANCHOR,  // anchor location information:: id, x,y,z
    IPS_DATA_ANCHORS_LIST,  // found anchors ids list after discovering
    IPS_ANCHORS_MANUAL_CONFIG_ACCEPTED,
    IPS_ANCHORS_TOBE_AUTOCALIBRATED_ACCEPTED,
    IPS_DRONETAGS_LIST,
    IPS_DRONETAGS,
    IPS_DRONETAGS_MANUAL_CONFIG_ACCEPTED,

    IPS_NOTENOUGH_TAGSFOUND,
    IPS_NOTENOUGH_ANCHORSFOUND,
    IPS_LAST_REQUEST_HAS_FAILED,   // when system, discover, autocalibrate, or update-settings request failed
    IPS_POSITIONING_STARTED,
    IPS_POSITIONING_STOPPED,
    IPS_POZYXSETTINGS_CLEARED,
    IPS_GET_DRONEFILTER_NOTIFICATION,
    IPS_SET_DRONEFILTER_NOTIFICATION,

    IPS_LAST_NOTIFICATION    // Keep it at the end
};

// The RCam ACK notifications are provided via the RCamModule::AckNotification message, that also provides
// which command is being acknowledged
public enum RCamNotificationType
{
    RCAM_COMMAND_FAILED_NOTIFICATION  = NotificationType.LAST_STD_NOTIFICATION_TYPE, // the given command failed (its code is in the payload)
                                        // if the command is zero, then we have received from the camera an unknown message (communication error??) 
    RCAM_EMULATE_KEY_NOTIFICATION,   // emulate a key press
    RCAM_SWITCHED_TO_REC_NOTIFICATION, // changed to record mode (video)
    RCAM_SWITCHED_TO_STILL_NOTIFICATION,       // changed to photo mode
    RCAM_SWITCHED_TO_PB_NOTIFICATION,      // change to playback mode
    RCAM_STARTED_REC_NOTIFICATION,         // started recording
    RCAM_STOPPED_REC_NOTIFICATION,          // stop recording
    RCAM_CAPTURED_NOTIFICATION,           // captured a photo
    RCAM_CAPTURED_AF_NOTIFICATION,        // capture a photo with autofocus?
    RCAM_AF_NOTIFICATION,                   // not used
    RCAM_STARTED_PB_NOTIFICATION,          // start playback
    RCAM_STOPPED_PB_NOTIFICATION,           // stop playback
    RCAM_PAUSED_PB_NOTIFICATION,          // pause playback
    RCAM_RESUMED_PB_NOTIFICATION,         // resume playback
    RCAM_CONFIG_CHANGED_NOTIFICATION,        // set configuration
    RCAM_CONFIG_STATUS_NOTIFICATION,        // | cmd | key |
    RCAM_WIFI_SET_NOTIFICATION,
    RCAM_WIFI_STATUS_NOTIFICATION,
    RCAM_BATTERY_LEVEL_NOTIFICATION,       // request battery charge
    RCAM_CARD_STATUS_NOTIFICATION,   // request card status
    RCAM_MODE_NOTIFICATION,          // request mode (see RCamMode below)
    RCAM_STATUS_NOTIFICATION,        // camera status (see RCamStatus below)
    RCAM_REMAINING_REC_TIME_NOTIFICATION, // request remaining recording time
    RCAM_REMAINING_STILL_PHOTOS_NOTIFICATION, // request number of pictures left
    RCAM_CARD_FORMATTED_NOTIFICATION,         // format card
    RCAM_GET_BT_VERSION_NOTIFICATION,            // not used
    RCAM_SWITCHED_TO_MULTIPLE_MODE_CAPTURE_NOTIFICATION, //
    RCAM_CHANGED_X_CONFIG_NOTIFICATION, //
    RCAM__X_CONFIG_NOTIFICATION, //
    RCAM_RECORDING_STATUS_NOTIFICATION, //
    RCAM_BURST_CAPTURE_STARTED_NOTIFICATION,   // start burst capture
    RCAM_BURST_CAPTURE_CANCELED_NOTIFICATION,  // cancel burst capture
    RCAM_SETTING_CLEARED_NOTIFICATION,         // clear settings
    RCAM_PBMEDIA_FILE_INITED_NOTIFICATION,     // init playback media file
    RCAM_PBMEDIA_CHANGED_NEXT_FILE_NOTIFICATION,     // next playback file
    RCAM_PBMEDIA_CHANGED_PREV_FILE_NOTIFICATION,     // previous playback file
    RCAM_PBMEDIA_DELETED_FILE_NOTIFICATION,   // delete file
    RCAM_SHUTTING_DOWN_NOTIFICATION,              // shutdown

    RCAM_LAST_NOTIFICATION   // Keep it at the end
};


public enum RCamMode : byte {
    RCAM_MODE_CAPTURE = 0,
    RCAM_MODE_RECORD = 1,
    RCAM_MODE_PLAYBACK = 3
};

public enum RCamStatus : byte {
    RCAM_STATUS_RECORD_MODE = 0x00,     // record_mode,
    RCAM_STATUS_RECORDING = 0x10,       // recording,
    RCAM_STATUS_PLAYBACK_MODE = 0x11,   // playback_mode,
    RCAM_STATUS_PLAYING = 0x20,         // playing,
    RCAM_STATUS_PLAYBACK_PAUSED = 0x21, // playback_paused,
    RCAM_STATUS_STILL_MODE = 0x22,      // still_mode,
    RCAM_STATUS_STILL_MODE_TIMELAPSE_IDLE = 0x40,      // still_mode_timelapse_idle,
    RCAM_STATUS_STILL_MODE_TIMELAPSING = 0x41 // still_mode_timelapse_ing,
};


public enum FCSMultiplexerNotificationType
{
    FCS_MISSIONACK_NOTIFICATION =  NotificationType.LAST_STD_NOTIFICATION_TYPE, // Mission ACK (all WP received)
    FCS_WPREQUEST_NOTIFICATION, // Requesting  specific WP
    FCS_VERSION_NOTIFICATION,   // Version response
    FCS_WPREACHED_NOTIFICATION, // Way Point reached (Guided mode)
    FCS_ROLLPITCHYAW_NOTIFICATION,  // Current roll, pitch & yaw values
    FCS_MOTORS_NOTIFICATION,    // Motors average power
    FCS_BATTERY_NOTIFICATION,   // Battery information
    FCS_PONG_NOTIFICATION,      // Echo reply for a ping
    FCS_DATA_POS_NOTIFICATION,  // Packet informing the actual position
    FCS_NACK_NOTIFICATION,      // Command in wrong format or not compatible
    FCS_ACK_NOTIFICATION,       // Command received and launched
    FCS_UPLOADED_WAYPOINT_NOTIFICATION, // A waypoint has been uploaded to the FCS
    FCS_SPEED_CHANGED_NOTIFICATION, // Someone requested to change the drone speed

    FCS_LAST_NOTIFICATION       // Keep it at the end
};

public enum OSNotificationType
{
    OS_AVAILABLE_DISKSPACE_NOTIFICATION =  NotificationType.LAST_STD_NOTIFICATION_TYPE,  // Available disk space in the server
    OS_SYSTEM_INFO_NOTIFICATION,    // System information about the server (see SystemInfo in utils.h)
    OS_FILE_CONTENT_NOTIFICATION,   // Portion of the contents of a file
    OS_FILE_ERROR_NOTIFICATION,     // There has been an error working with a file (file not found, disk full, read only...)
    OS_INSUFFICIENT_MEMORY_NOTIFICATION,    // An operation required that could not be allocated. Maybe it is a temporary issue...
    OS_FILE_SIZE_NOTIFICATION,      // Providing the size of a given file
    OS_FILE_CREATED_NOTIFICATION,            // File created successfully
    OS_FILE_DELETED_NOTIFICATION,            // File deleted successfuly
    OS_FILE_CHUNK_WRITTEN_NOTIFICATION,      // File chunk written successfully
    OS_NETWORK_STATS_NOTIFICATION,          // Complete network stats
    OS_BLOB_RECEIVED_NOTIFICATION,      // The blob has arrived
    OS_BLOB_SENT_NOTIFICATION,

    OS_LAST_NOTIFICATION       // Keep it at the end
};

public enum OSFileError : byte {
    OS_FILE_ERROR_NO_ERROR = 0, // Should not happen
    OS_FILE_NOT_FOUND,
    OS_FILE_INVALID_NAME,   // Filenames (e.g., for creating a new file) should longer than 0 and shorter than 100 characters
    OS_FILE_DELETE_FAILED,  // A file could't be deleted
    OS_FILE_CREATION_FAILED,  // The file couldn't be created
    OS_FILE_OPEN_FAILED,    // Trying to open a file failed
    OS_FILE_END_OF_FILE,    // Trying to read beyond the eof
    OS_FILE_LOGICAL_ERROR,  // This error *maybe* temporary and you can still work with the stream
    OS_FILE_IO_ERROR,       // This is a fatal error, that requires closing the file and start over
    OS_FILE_OUT_OF_BOUNDS,   // The requested file chunk does not exists
};

public enum AtreyuNotificationType
{
    ATREYU_VERSION_NOTIFICATION =  NotificationType.LAST_STD_NOTIFICATION_TYPE,  // Version of the server in the OCS
    ATREYU_UPDATE_PACKAGE_PATHNAME_NOTIFICATION,   // The path + name of the update package (use OS_CREATE_FILE + OS_APPEND_DATA_FILE to upload it)
    ATREYU_SYSTEM_STATE_CHANGE_NOTIFICATION,       // The system has changed its state
    ATREYU_CANNOT_CHANGE_STATE_NOTIFICATION,

    ATREYU_LAST_NOTIFICATION     // Keep it at the end
};


public enum PlanLibrarianNotificationType
{
    PLAN_LIB_NUMBER_PLANS_IN_DDBB_NOTIFICATION = NotificationType.LAST_STD_NOTIFICATION_TYPE,  // returns the number of plans in the server
    PLAN_LIB_METADATA_NOTIFICATION,    // returns the metadata of the given plan id
    PLAN_LIB_THUMBNAIL_NOTIFICATION,    // returns the thumbnail of the given plan id
    PLAN_LIB_MISSION_NOTIFICATION,      // returns the requested mission
    PLAN_LIB_MAP_NOTIFICATION,         // returns the requested map
    PLAN_LIB_BASE_NAME_NOTIFICATION,        // return the base name of the plan
    PLAN_LIB_MISSION_INDEX_NOT_FOUND_NOTIFICATION,       // the mission index does not exists
    PLAN_LIB_FILE_NOT_FOUND_NOTIFICATION,    // we couldn't find the thumbnail, map or mission requested
    PLAN_LIB_LIBRARY_PATH_NOTIFICATION,          // path of the library in the server
    PLAN_LIB_MISSION_DELETED_NOTIFICATION,

    PLAN_LIB_LAST_NOTIFICATION    // Keep it at the end
};

public enum PlanExecutorNotificationType
{
    PLAN_EXEC_PLAN_LOADING_NOTIFICATION = NotificationType.LAST_STD_NOTIFICATION_TYPE,  // loading the requested plan into the fcs
    PLAN_EXEC_PLAN_LOADED_NOTIFICATION,  // plan loaded correctly
    PLAN_EXEC_ERROR_LOADING_PLAN_NOTIFICATION,   // there is an error loading the plan (it does not exist, or it has a wrong format). 
                                                // The Plan executor ends (atreyu state goes back to idle)
    PLAN_EXEC_PREFLIGHT_TESTING_NOTIFICATION,   // we are testing the systems to start a flight
    PLAN_EXEC_PREFLIGHT_TESTS_FAILED_NOTIFICATION,   // preflight tests failed (the second part has a message in asciiz).
                                                // The Plan executor ends (atreyu state goes back to idle)
    PLAN_EXEC_READY_TO_FLIGHT_NOTIFICATION,  // we are ready to execute the loaded plan
    PLAN_EXEC_FIRST_WAYPOINT_IS_TOO_FAR_NOTIFICATION, // we are ready to fly, but the first waypoint is too far away
    PLAN_EXEC_FLYING_TO_NEXT_WP_NOTIFICATION,   // we are flying towards a waypoint
    PLAN_EXEC_REACHED_WAYPOINT_NOTIFICATION,    // we have arrived a waypoint and start moving ot the next
    PLAN_EXEC_FLIGHT_PLAN_COMPLETED_NOTIFICATION,   // the drone has completed the flight plan
                                                // The Plan executor ends (atreyu state goes back to idle)
    PLAN_EXEC_REGISTRATION_MATRIX_CHANGED_NOTIFICATION,  // Provides the new matrix that transform from map to pozyx
    PLAN_EXEC_REGISTRATION_MATRIX_MISSING_NOTIFICATION, // We can't flight without registration
    PLAN_EXEC_FCS_NOT_RESPONDING_NOTIFICATION,  // The fcs is not responding. The Plan executor ends (atreyu state goes back to idle)
    PLAN_EXEC_LAUNCHED_NOTIFICATION,
    PLAN_EXEC_LANDING_NOTIFICATION,
    PLAN_EXEC_CURRENT_FLIGHT_PLAN_NOTIFICATION,

    PLAN_EXEC_LAST_NOTIFICATION   // Keep it at the end
};

public enum FPVNotificationType
{
    FPV_RESOLUTION_NOTIFICATION = NotificationType.LAST_STD_NOTIFICATION_TYPE,
    FPV_IMAGE_JPEG_NOTIFICATION,
    FPV_STREAMING_STARTED,
    FPV_STREAMING_STOPPED,
    FPV_LAST_COMMAND_HAS_FAILED,
    FPV_LAST_NOTIFICATION
};


public enum GimbalMultiplexerNotificationType : byte
{
    GIMBAL_GOTO_ANGLE_NOTIFICATION = NotificationType.LAST_STD_NOTIFICATION_TYPE, // The gimbal is going to a given angle (hopefully)
    GIMBAL_MODE_CHANGED_TO_RC_NOTIFICATION,     // Gimbal requested to change to rc mode
    GIMBAL_MODE_CHANGED_TO_RC_FIX_MODE_NOTIFICATION,         // Gimbal requested to change to rc fix mode
    GIMBAL_MODE_CHANGED_TO_RC_NOROLL_MODE_NOTIFICATION,      // Gimbal requested to change to rc no roll mode
    GIMBAL_ANGLE_CHANGED_NOTIFICATION,      // Current gimbal configuration
    GIMBAL_LAST_NOTIFICATION
};


public enum DroneNotificationType : byte
{
    DRONE_ZCAMERA_OFFSET_NOTIFICATION = NotificationType.LAST_STD_NOTIFICATION_TYPE, // Returns the position of the depth camera wrt the drone
    DRONE_POSITION_POSE_NOTIFICATION,    // Provides the position and orientation of the drone
    DRONE_GIMBAL_POSE_NOTIFICATION,     // Provides the orientation of the gimbal (it has changed)
    DRONE_LAST_NOTIFICATION
};

public enum MapperNotificationType : byte 
{
    MAPPER_IDLE_NOTIFICATION  = NotificationType.LAST_STD_NOTIFICATION_TYPE,  // The mapper has started. Waiting for the next command (create/load)
    MAPPER_MAP_READY_NOTIFICATION,  // The map is ready to start capturing
    MAPPER_STARTED_MAPPING_NOTIFICATION,
    MAPPER_PAUSE_MAPPING_NOTIFICATION,
    MAPPER_DONE_MAPPING_NOTIFICATION,
    MAPPER_NUMBER_POINTCLOUDS_IN_MAP_NOTIFICATION,  // Gives the number of point clouds in the map
    MAPPER_LIST_POINTCLOUD_IDS_NOTIFICATION,        // Gives the list of ids in the current map
    MAPPER_POINTCLOUD_NOTIFICATION,                 // Gives a pointcloud. The client is in charge of checking whether this is
                                                    // a new version of an existing cloud and replacing it
    MAPPER_ALL_POINT_CLOUDS_DELETED_NOTIFICATION,    // All the point clouds have been discarded
    MAPPER_ERROR_SAVING_DDBB_NOTIFICATION,      // There was an error saving the database to a file (disk full?)
    MAPPER_NON_EXISTENT_POINTCLOUD_NOTIFICATION,    // The requested point cloud is not in the database
    MAPPER_CURRENT_GUID_NOTIFICATION,       // The guid of the current map
    MAPPER_DELETED_POINTCLOUD_NOTIFICATION, // A point cloud was deleted from the ddbb

    MAPPER_LAST_NOTIFICATION
};

