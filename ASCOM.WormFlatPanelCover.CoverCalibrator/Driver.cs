//tabs=4
// --------------------------------------------------------------------------------
// TODO fill in this information for your driver, then remove this line!
//
// ASCOM CoverCalibrator driver for WormFlatPanelCover
//
// Description:	Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam 
//				nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam 
//				erat, sed diam voluptua. At vero eos et accusam et justo duo 
//				dolores et ea rebum. Stet clita kasd gubergren, no sea takimata 
//				sanctus est Lorem ipsum dolor sit amet.
//
// Implements:	ASCOM CoverCalibrator interface version: <To be completed by driver developer>
// Author:		(XXX) Your N. Here <your@email.here>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// dd-mmm-yyyy	XXX	6.0.0	Initial edit, created from ASCOM driver template
// --------------------------------------------------------------------------------
//


// This is used to define code in the template that is specific to one class implementation
// unused code can be deleted and this definition removed.
#define CoverCalibrator

using ASCOM.Astrometry.AstroUtils;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

namespace ASCOM.WormFlatPanelCover
{
    //
    // Your driver's DeviceID is ASCOM.WormFlatPanelCover.CoverCalibrator
    //
    // The Guid attribute sets the CLSID for ASCOM.WormFlatPanelCover.CoverCalibrator
    // The ClassInterface/None attribute prevents an empty interface called
    // _WormFlatPanelCover from being created and used as the [default] interface
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM CoverCalibrator Driver for WormFlatPanelCover.
    /// </summary>
    [Guid("d622d35e-07e6-4efc-8989-37b6d34a0e75")]
    [ClassInterface(ClassInterfaceType.None)]
    public class CoverCalibrator : ICoverCalibratorV1
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID = "ASCOM.WormFlatPanelCover.CoverCalibrator";
        // TODO Change the descriptive string for your driver then remove this line
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescription = "虫子电动平场镜头盖";

        internal static string comPortProfileName = "COM Port"; // Constants used for Profile persistence
        internal static string comPortDefault = "COM3";
        internal string comPort; // Variables to hold the current device configuration

        internal static string currentAngleProfileName = "Current cover opened angle";
        internal static string currentAngleDefault = "0";
        internal int currentAngle;  // Current cover opened angle

        internal static string targetAngleProfileName = "Target angle / cover travel range";
        internal static string targetAngleDefault = "520";
        internal int targetAngle;   // Target angle / cover travel range

        internal static string coverMovingSpeedProfileName = "Cover moving speed";
        internal static string coverMovingSpeedDefault = "200";
        internal int coverMovingSpeed;  // Cover moving speed

        internal static string coverMovingAccelerationProfileName = "Cover moving accerlation";
        internal static string coverMovingAccelerationDefault = "10";
        internal int coverMovingAcceleration;   // Cover moving accerlation

        internal static string lastUsedFlatPanelSerialProfileName = "Serial number string of the last used flat panel";
        internal static string lastUsedFlatPanelSerialDefault = "";
        internal string lastUsedFlatPanelSerial;  // Serial number string of the last used flat panel

        internal static string currentFlatPanelBrightnessProfileName = "Current flat panel brightness";
        internal static string currentFlatPanelBrightnessDefault = "0";   // 0 - Off, 1 - Brightness Low, 2 - Brightness High
        internal int currentFlatPanelBrightness;  // Serial number string of the last used flat panel

        internal static string traceStateProfileName = "Trace Level";
        internal string traceStateDefault = "true";

        internal static string simulationStateProfileName = "Simulation Mode";
        internal static string simulationStateDefault = "true";
        internal bool simulationState;

        /// <summary>
        /// Device wrappers
        /// </summary>
        internal WormCoverWrapper cover;
        internal WormFlatPanelWrapper flat_panel;

        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        private bool connectedState;


        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        private Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        private AstroUtils astroUtilities;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal TraceLogger tl;

        /// <summary>
        /// Initializes a new instance of the <see cref="WormFlatPanelCover"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public CoverCalibrator()
        {
            tl = new TraceLogger("", "WormFlatPanelCover");
            ReadProfile(); // Read device configuration from the ASCOM Profile store

            tl.LogMessage("CoverCalibrator", "Starting initialisation");

            connectedState = false; // Initialise connected to false
            utilities = new Util(); //Initialise util object
            astroUtilities = new AstroUtils(); // Initialise astro-utilities object

            //TODO: Implement your additional construction here
            cover = new WormCoverWrapper(this, simulationState);
            LogMessage("CoverCalibrator", "Serial port cover controller created (Simulation:{0}).", simulationState);

            flat_panel = new WormFlatPanelWrapper(this, simulationState);
            LogMessage("CoverCalibrator", "Flat panel controller created (Simulation:{0}).", simulationState);

            tl.LogMessage("CoverCalibrator", "Completed initialisation");
        }


        //
        // PUBLIC COM INTERFACE ICoverCalibratorV1 IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            //  disconnect device before entering setup dialog
            if (IsConnected)
                Connected = false;

            using (SetupDialogForm F = new SetupDialogForm(this))
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                }
                // disconnect device upon exiting setup dialog
                Connected = false;
            }
        }

        public ArrayList SupportedActions
        {
            get
            {
                tl.LogMessage("SupportedActions Get", "Returning empty arraylist");
                return new ArrayList();
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            LogMessage("", "Action {0}, parameters {1} not implemented", actionName, actionParameters);
            throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            // TODO The optional CommandBlind method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBlind must send the supplied command to the mount and return immediately without waiting for a response

            throw new ASCOM.MethodNotImplementedException("CommandBlind");
        }

        public bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            // TODO The optional CommandBool method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBool must send the supplied command to the mount, wait for a response and parse this to return a True or False value

            // string retString = CommandString(command, raw); // Send the command and wait for the response
            // bool retBool = XXXXXXXXXXXXX; // Parse the returned string and create a boolean True / False value
            // return retBool; // Return the boolean value to the client

            throw new ASCOM.MethodNotImplementedException("CommandBool");
        }

        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            // TODO The optional CommandString method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandString must send the supplied command to the mount and wait for a response before returning this to the client

            throw new ASCOM.MethodNotImplementedException("CommandString");
        }

        public void Dispose()
        {
            // Clean up the trace logger and util objects
            tl.Enabled = false;
            tl.Dispose();
            tl = null;
            utilities.Dispose();
            utilities = null;
            astroUtilities.Dispose();
            astroUtilities = null;
        }

        public bool Connected
        {
            get
            {
                LogMessage("Connected", "Get {0}", IsConnected);
                return IsConnected;
            }
            set
            {
                tl.LogMessage("Connected", "Set {0}", value);
                if (value == IsConnected)
                    return;

                if (value)
                {
                    connectedState = true;
                    LogMessage("Connected Set", "Connecting to port {0}", comPort);

                    flat_panel.Connect();
                    if (!cover.Connect())
                        connectedState = false;
                }
                else
                {
                    connectedState = false;
                    LogMessage("Connected Set", "Disconnecting from port {0}", comPort);

                    flat_panel.Disconnect();
                    cover.Disconnect();
                }
            }
        }

        public string Description
        {
            // TODO customise this device description
            get
            {
                tl.LogMessage("Description Get", driverDescription);
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // TODO customise this driver description
                string driverInfo = "Information about the driver itself. Version: " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                LogMessage("InterfaceVersion Get", "1");
                return Convert.ToInt16("1");
            }
        }

        public string Name
        {
            get
            {
                string name = "Short driver name - please customise";
                tl.LogMessage("Name Get", name);
                return name;
            }
        }

        #endregion

        #region ICoverCalibrator Implementation

        /// <summary>
        /// Returns the state of the device cover, if present, otherwise returns "NotPresent"
        /// </summary>
        public CoverStatus CoverState
        {
            get
            {
                CoverStatus status = CoverStatus.Unknown;
                if (currentAngle >= targetAngle)
                    status = CoverStatus.Open;
                else if (currentAngle == 0)
                    status = CoverStatus.Closed;
                else if (currentAngle > 0 && currentAngle < targetAngle)
                    status = CoverStatus.Moving;
                else
                    status = CoverStatus.Error;
                LogMessage("CoverState Get", "CoverStatus = {0}", status);
                return status;
            }
        }

        /// <summary>
        /// Initiates cover opening if a cover is present
        /// </summary>
        public void OpenCover()
        {
            LogMessage("OpenCover", "Opening worm cover, curr({0}), target({1}).", currentAngle, targetAngle);
            if (cover.openCover())
                cover.waitForCoverOpenCompletion();
        }

        /// <summary>
        /// Initiates cover closing if a cover is present
        /// </summary>
        public void CloseCover()
        {
            LogMessage("CloseCover", "Closing worm cover, curr({0}), target({1}).", currentAngle, targetAngle);
            if (cover.closeCover())
                cover.waitForCoverCloseComletion();
        }

        /// <summary>
        /// Stops any cover movement that may be in progress if a cover is present and cover movement can be interrupted.
        /// </summary>
        public void HaltCover()
        {
            cover.stopCoverMotor();
            cover.voidCoverMotherDriver();
            tl.LogMessage("HaltCover", "Cover movement stopped.");
        }

        /// <summary>
        /// Returns the state of the calibration device, if present, otherwise returns "NotPresent"
        /// </summary>
        public CalibratorStatus CalibratorState
        {
            get
            {
                CalibratorStatus status = CalibratorStatus.Unknown;
                if (currentFlatPanelBrightness == 0)
                    status = CalibratorStatus.Off;
                else if (currentFlatPanelBrightness == (int)WormFlatPanelWrapper.BRIGHTNESS.LOW 
                    || currentFlatPanelBrightness == (int)WormFlatPanelWrapper.BRIGHTNESS.HIGH)
                    status = CalibratorStatus.Ready;
                else
                    status = CalibratorStatus.Error;
                LogMessage("CalibratorState Get", "Flatpanel status = {0}", status);
                return status;
            }
        }

        /// <summary>
        /// Returns the current calibrator brightness in the range 0 (completely off) to <see cref="MaxBrightness"/> (fully on)
        /// </summary>
        public int Brightness
        {
            get
            {
                LogMessage("Brightness Get", $"Current Worm flat panel brightness = {currentFlatPanelBrightness}");
                return currentFlatPanelBrightness;
            }
        }

        /// <summary>
        /// The Brightness value that makes the calibrator deliver its maximum illumination.
        /// </summary>
        public int MaxBrightness
        {
            get
            {
                LogMessage("MaxBrightness Get", $"Worm flat panel maximum brightness = {(int)WormFlatPanelWrapper.BRIGHTNESS.HIGH}");
                return (int)WormFlatPanelWrapper.BRIGHTNESS.HIGH;
            }
        }

        /// <summary>
        /// Turns the calibrator on at the specified brightness if the device has calibration capability
        /// </summary>
        /// <param name="Brightness"></param>
        public void CalibratorOn(int Brightness)
        {
            if (Brightness > (int)WormFlatPanelWrapper.BRIGHTNESS.HIGH)
                Brightness = (int)WormFlatPanelWrapper.BRIGHTNESS.HIGH;
            if (Brightness < 0)
                Brightness = 0;

            switch (Brightness)
            {
                case 0: flat_panel.TurnOff(); break;
                case 1: flat_panel.TurnOn(WormFlatPanelWrapper.BRIGHTNESS.LOW); break;
                case 2: flat_panel.TurnOn(WormFlatPanelWrapper.BRIGHTNESS.HIGH); break;
            }
            tl.LogMessage("CalibratorOn", $"Worm flat panel turned on. Brightness set: {Brightness}");
        }

        /// <summary>
        /// Turns the calibrator off if the device has calibration capability
        /// </summary>
        public void CalibratorOff()
        {
            flat_panel.TurnOff();
            tl.LogMessage("CalibratorOff", "Worm flat panel turned off.");
        }

        #endregion

        #region Private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with driver development

        #region ASCOM Registration

        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "CoverCalibrator";
                if (bRegister)
                {
                    P.Register(driverID, driverDescription);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }

        #endregion

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected
        {
            get
            {
                // TODO check that the driver hardware connection exists and is connected to the hardware
                return connectedState;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "CoverCalibrator";
                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
                simulationState = Convert.ToBoolean(driverProfile.GetValue(driverID, simulationStateProfileName, string.Empty, simulationStateDefault));
                comPort = driverProfile.GetValue(driverID, comPortProfileName, string.Empty, comPortDefault);
                currentAngle = Int32.Parse(driverProfile.GetValue(driverID, currentAngleProfileName, string.Empty, currentAngleDefault));
                targetAngle = Int32.Parse(driverProfile.GetValue(driverID, targetAngleProfileName, string.Empty, targetAngleDefault));
                coverMovingSpeed = Int32.Parse(driverProfile.GetValue(driverID, coverMovingSpeedProfileName, string.Empty, coverMovingSpeedDefault));
                coverMovingAcceleration = Int32.Parse(driverProfile.GetValue(driverID, coverMovingAccelerationProfileName, string.Empty, coverMovingAccelerationDefault));
                lastUsedFlatPanelSerial = driverProfile.GetValue(driverID, lastUsedFlatPanelSerialProfileName, string.Empty, lastUsedFlatPanelSerialDefault);
                currentFlatPanelBrightness = Int32.Parse(driverProfile.GetValue(driverID, currentFlatPanelBrightnessProfileName, string.Empty, currentFlatPanelBrightnessDefault));
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "CoverCalibrator";
                driverProfile.WriteValue(driverID, traceStateProfileName, tl.Enabled.ToString());
                driverProfile.WriteValue(driverID, simulationStateProfileName, simulationState.ToString());
                driverProfile.WriteValue(driverID, comPortProfileName, comPort.ToString());
                driverProfile.WriteValue(driverID, currentAngleProfileName, currentAngle.ToString());
                driverProfile.WriteValue(driverID, targetAngleProfileName, targetAngle.ToString());
                driverProfile.WriteValue(driverID, coverMovingSpeedProfileName, coverMovingSpeed.ToString());
                driverProfile.WriteValue(driverID, coverMovingAccelerationProfileName, coverMovingAcceleration.ToString());
                driverProfile.WriteValue(driverID, lastUsedFlatPanelSerialProfileName, lastUsedFlatPanelSerial.ToString());
                driverProfile.WriteValue(driverID, currentFlatPanelBrightnessProfileName, currentFlatPanelBrightness.ToString());
            }
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(identifier, msg);
        }
        #endregion
    }
}
