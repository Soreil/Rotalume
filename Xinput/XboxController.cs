using System;
using System.Threading;

namespace J2i.Net.XInputWrapper
{
    public class XboxController
    {
        readonly int _playerIndex;
        static bool keepRunning;
        static bool isRunning;
        static readonly object SyncLock;
        static Thread? pollingThread;

        bool _stopMotorTimerActive;
        DateTime _stopMotorTime;
        XInputBatteryInformation _batteryInformationGamepad;
        XInputBatteryInformation _batterInformationHeadset;

        XInputState gamepadStatePrev;
        XInputState gamepadStateCurrent;

        public static int UpdateFrequency { get; set; }

        public XInputBatteryInformation BatteryInformationGamepad
        {
            get => _batteryInformationGamepad;
            internal set => _batteryInformationGamepad = value;
        }

        public XInputBatteryInformation BatteryInformationHeadset
        {
            get => _batterInformationHeadset;
            internal set => _batterInformationHeadset = value;
        }

        private const int ControllerLimit = 4;

        static readonly XboxController[] Controllers;

        static XboxController()
        {
            Controllers = new XboxController[ControllerLimit];
            SyncLock = new object();
            for (int i = 0; i < ControllerLimit; ++i)
            {
                Controllers[i] = new XboxController(i);
            }
            UpdateFrequency = 25;
        }

        public event EventHandler<XboxControllerStateChangedEventArgs>? StateChanged;

        public static XboxController RetrieveController(int index) => Controllers[index];

        private XboxController(int playerIndex)
        {
            _playerIndex = playerIndex;
            gamepadStatePrev.Copy(gamepadStateCurrent);
        }

        public void UpdateBatteryState()
        {
            XInputBatteryInformation headset = new(),
            gamepad = new();

            _ = XInput.XInputGetBatteryInformation(_playerIndex, (byte)BatteryDeviceType.BATTERY_DEVTYPE_GAMEPAD, ref gamepad);
            _ = XInput.XInputGetBatteryInformation(_playerIndex, (byte)BatteryDeviceType.BATTERY_DEVTYPE_HEADSET, ref headset);

            BatteryInformationHeadset = headset;
            BatteryInformationGamepad = gamepad;
        }

        protected void OnStateChanged() => StateChanged?.Invoke(this, new XboxControllerStateChangedEventArgs() { CurrentInputState = gamepadStateCurrent, PreviousInputState = gamepadStatePrev });

        public XInputCapabilities GetCapabilities()
        {
            XInputCapabilities capabilities = new XInputCapabilities();
            _ = XInput.XInputGetCapabilities(_playerIndex, XInputConstants.XINPUT_FLAG_GAMEPAD, ref capabilities);
            return capabilities;
        }


        #region Digital Button States
        public bool IsDPadUpPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_DPAD_UP);

        public bool IsDPadDownPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_DPAD_DOWN);

        public bool IsDPadLeftPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_DPAD_LEFT);

        public bool IsDPadRightPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_DPAD_RIGHT);

        public bool IsAPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_A);

        public bool IsBPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_B);

        public bool IsXPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_X);

        public bool IsYPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_Y);


        public bool IsBackPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_BACK);


        public bool IsStartPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_START);


        public bool IsLeftShoulderPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_LEFT_SHOULDER);


        public bool IsRightShoulderPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_RIGHT_SHOULDER);

        public bool IsLeftStickPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_LEFT_THUMB);

        public bool IsRightStickPressed => gamepadStateCurrent.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_RIGHT_THUMB);
        #endregion

        #region Analogue Input States
        public int LeftTrigger => gamepadStateCurrent.Gamepad.bLeftTrigger;

        public int RightTrigger => gamepadStateCurrent.Gamepad.bRightTrigger;

        public Point LeftThumbStick => new(gamepadStateCurrent.Gamepad.sThumbLX, gamepadStateCurrent.Gamepad.sThumbLY);

        public Point RightThumbStick => new(gamepadStateCurrent.Gamepad.sThumbRX, gamepadStateCurrent.Gamepad.sThumbRY);

        #endregion

        public bool IsConnected { get; internal set; }

        #region Polling
        public static void StartPolling()
        {
            if (!isRunning)
            {
                lock (SyncLock)
                {
                    if (!isRunning)
                    {
                        pollingThread = new Thread(PollerLoop);
                        pollingThread.Start();
                    }
                }
            }
        }

        public static void StopPolling()
        {
            if (isRunning)
                keepRunning = false;
        }

        static void PollerLoop()
        {
            lock (SyncLock)
            {
                if (isRunning)
                    return;
                isRunning = true;
            }
            keepRunning = true;
            while (keepRunning)
            {
                foreach (var c in Controllers)
                {
                    c.UpdateState();
                }
                Thread.Sleep(UpdateFrequency);
            }
            lock (SyncLock)
            {
                isRunning = false;
            }
        }

        public void UpdateState()
        {
            int result = XInput.XInputGetState(_playerIndex, ref gamepadStateCurrent);
            IsConnected = result == 0;

            UpdateBatteryState();
            if (gamepadStateCurrent.PacketNumber != gamepadStatePrev.PacketNumber)
            {
                OnStateChanged();
            }
            gamepadStatePrev.Copy(gamepadStateCurrent);

            if (_stopMotorTimerActive && (DateTime.Now >= _stopMotorTime))
            {
                var stopStrength = new XInputVibration() { LeftMotorSpeed = 0, RightMotorSpeed = 0 };
                _ = XInput.XInputSetState(_playerIndex, ref stopStrength);
            }
        }
        #endregion

        #region Motor Functions
        public void Vibrate(double leftMotor, double rightMotor) => Vibrate(leftMotor, rightMotor, TimeSpan.MinValue);

        public void Vibrate(double leftMotor, double rightMotor, TimeSpan length)
        {
            leftMotor = Math.Max(0d, Math.Min(1d, leftMotor));
            rightMotor = Math.Max(0d, Math.Min(1d, rightMotor));

            var vibration = new XInputVibration() { LeftMotorSpeed = (ushort)(65535d * leftMotor), RightMotorSpeed = (ushort)(65535d * rightMotor) };
            Vibrate(vibration, length);
        }


        public void Vibrate(XInputVibration strength)
        {
            _stopMotorTimerActive = false;
            _ = XInput.XInputSetState(_playerIndex, ref strength);
        }

        public void Vibrate(XInputVibration strength, TimeSpan length)
        {
            _ = XInput.XInputSetState(_playerIndex, ref strength);
            if (length != TimeSpan.MinValue)
            {
                _stopMotorTime = DateTime.Now.Add(length);
                _stopMotorTimerActive = true;
            }
        }
        #endregion

        public override string ToString() => _playerIndex.ToString();
    }
}
