namespace VRfreePluginUnity {
    public static class VRfreeStatusCode {
        //some vrFree device status flags
        //public const byte NOT_CONNECTED = 0x01;
        //public const byte CONNECTING = 0x02;
        //public const byte CONNECTION_FAILED = 0x04; //will be cleared on reconnect
        //public const byte START_STREAMING = 0x08;
        //public const byte STREAMING = 0x10;
        //public const byte READING_FAILED = 0x20; //will be cleared on next valid read
        //public const byte INVALID_ARGUMENTS = 0x40; //will be cleared on getHandData with valid input
        //public const byte IDLE = 0x80; //not doing anything, possible error during startup

        private const string NOT_CONNECTED_STRING = "Please plug-in the VRfree device";
        private const string CONNECTING_STRING = "Connecting to VRfree device...";
        private const string START_STREAMING_STRING = "Starting VRfree data stream...";
        private const string STREAMING_STRING = "Streaming VRfree data...";
        private const string IDLE_STRING = "Idle";
        private const string UNKNOWN_STRING = "unknown";
        private const string CONNECTION_FAILED_STRING = "Connection failed, please re-connect the device";
        private const string READING_FAILED_STRING = "Reading failed, please restart the device";
        private const string INVALID_ARGUMENTS_STRING = "Invalid arguments, please pass correct data to the driver";
        private const string NONE_STRING = "none";

        public static string statusCodeToString(VRfree.StatusCode statusCode) {
            if((statusCode & VRfree.StatusCode.NOT_CONNECTED) > 0) {
                return NOT_CONNECTED_STRING;
            } else if((statusCode & VRfree.StatusCode.CONNECTING) > 0) {
                return CONNECTING_STRING;
            } else if((statusCode & VRfree.StatusCode.START_STREAMING) > 0) {
                return START_STREAMING_STRING;
            } else if((statusCode & VRfree.StatusCode.STREAMING) > 0) {
                return STREAMING_STRING;
            } else if((statusCode & VRfree.StatusCode.IDLE) > 0) {
                return IDLE_STRING;
            } else {
                return UNKNOWN_STRING;
            }
        }

        public static string statusCodeToErrorString(VRfree.StatusCode statusCode) {
            //check for errors and give them out
            if((statusCode & VRfree.StatusCode.CONNECTION_FAILED) > 0) {
                return CONNECTION_FAILED_STRING;
            } else if((statusCode & VRfree.StatusCode.READING_FAILED) > 0) {
                return READING_FAILED_STRING;
            } else if((statusCode & VRfree.StatusCode.INVALID_ARGUMENTS) > 0) {
                return INVALID_ARGUMENTS_STRING;
            } else {
                return NONE_STRING;
            }
        }
    }
}