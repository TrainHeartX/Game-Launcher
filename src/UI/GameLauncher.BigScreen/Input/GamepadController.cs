using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace GameLauncher.BigScreen.Input
{
    /// <summary>
    /// Controlador de gamepad Xbox via P/Invoke directo a xinput1_4.dll.
    /// Soporta controles USB y Bluetooth sin dependencias externas.
    /// Polling a 60 FPS en el UI thread (DispatcherTimer).
    /// Auto-detecta el player index (0-3) al inicializar.
    /// </summary>
    public class GamepadController : IDisposable
    {
        #region XInput P/Invoke

        [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
        private static extern uint NativeGetState(uint dwUserIndex, out XINPUT_STATE pState);

        [DllImport("xinput1_4.dll", EntryPoint = "XInputGetCapabilities")]
        private static extern uint NativeGetCapabilities(uint dwUserIndex, uint dwFlags, out XINPUT_CAPABILITIES pCapabilities);

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_STATE
        {
            public uint PacketNumber;
            public XINPUT_GAMEPAD Gamepad;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_GAMEPAD
        {
            public ushort Buttons;
            public byte LeftTrigger;
            public byte RightTrigger;
            public short ThumbLX;
            public short ThumbLY;
            public short ThumbRX;
            public short ThumbRY;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_CAPABILITIES
        {
            public byte Type;
            public byte SubType;
            public ushort Flags;
            public XINPUT_GAMEPAD Gamepad;
            public XINPUT_VIBRATION Vibration;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_VIBRATION
        {
            public ushort LeftMotorSpeed;
            public ushort RightMotorSpeed;
        }

        // Button masks
        private const ushort DPAD_UP        = 0x0001;
        private const ushort DPAD_DOWN      = 0x0002;
        private const ushort DPAD_LEFT      = 0x0004;
        private const ushort DPAD_RIGHT     = 0x0008;
        private const ushort BTN_START      = 0x0010;
        private const ushort BTN_BACK       = 0x0020;
        private const ushort BTN_LEFT_THUMB = 0x0040;
        private const ushort BTN_RIGHT_THUMB= 0x0080;
        private const ushort BTN_LB         = 0x0100;
        private const ushort BTN_RB         = 0x0200;
        private const ushort BTN_A          = 0x1000;
        private const ushort BTN_B          = 0x2000;
        private const ushort BTN_X          = 0x4000;
        private const ushort BTN_Y          = 0x8000;

        private const uint ERROR_SUCCESS = 0;
        private const uint XINPUT_FLAG_GAMEPAD = 0x00000001;

        #endregion

        // Umbrales
        private const float ThumbStickDeadZone = 0.3f;
        private const float TriggerThreshold = 0.5f;

        private readonly DispatcherTimer _pollTimer;
        private uint _playerIndex;
        private bool _isConnected;
        private uint _lastPacketNumber;

        // Estado anterior para detectar transiciones
        private ushort _prevButtons;
        private float _prevLeftX, _prevLeftY;
        private float _prevLeftTrigger, _prevRightTrigger;

        // Eventos de navegación (D-Pad only)
        public event Action? NavigateUp;
        public event Action? NavigateDown;
        public event Action? NavigateLeft;
        public event Action? NavigateRight;

        // Eventos continuos de sticks analógicos (fire every poll, valores -1..1)
        public event Action<float, float>? LeftStickAxis;
        public event Action<float, float>? RightStickAxis;

        // Eventos de acción (mapeo: B=Select, X=Images, Y=Manage, A=Back, RT=Play)
        public event Action? SelectPressed;    // B  - Seleccionar/Confirmar
        public event Action? BackPressed;      // A  - Volver atrás
        public event Action? ImagesPressed;    // X  - Ver imágenes del juego
        public event Action? ManagePressed;    // Y  - Editar opciones del juego
        public event Action? PlayPressed;      // RT - Ejecutar juego
        public event Action? PageUpPressed;    // LB - Página arriba
        public event Action? PageDownPressed;  // RB - Página abajo

        // Combo: Select+Start simultáneo (para matar emulador/juego)
        public event Action? KillComboPressed;

        // Evento de diagnóstico
        public event Action<string>? DiagnosticMessage;

        public bool IsConnected => _isConnected;
        public uint PlayerIndex => _playerIndex;
        public string? InitError { get; private set; }

        public GamepadController(int playerIndex = -1)
        {
            // Si playerIndex == -1, auto-detectar
            if (playerIndex >= 0)
            {
                _playerIndex = (uint)playerIndex;
            }
            else
            {
                _playerIndex = AutoDetectPlayerIndex();
            }

            // Timer para polling a 60 FPS
            _pollTimer = new DispatcherTimer(DispatcherPriority.Input)
            {
                Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0)
            };
            _pollTimer.Tick += Poll;

            // Leer estado inicial
            try
            {
                var result = NativeGetState(_playerIndex, out var state);
                if (result == ERROR_SUCCESS)
                {
                    _isConnected = true;
                    _lastPacketNumber = state.PacketNumber;
                    _prevButtons = state.Gamepad.Buttons;
                    _prevLeftX = NormalizeThumb(state.Gamepad.ThumbLX);
                    _prevLeftY = NormalizeThumb(state.Gamepad.ThumbLY);
                    _prevLeftTrigger = state.Gamepad.LeftTrigger / 255f;
                    _prevRightTrigger = state.Gamepad.RightTrigger / 255f;
                    Log($"Gamepad conectado en index {_playerIndex}");
                }
                else
                {
                    Log($"Gamepad no detectado en index {_playerIndex} (error: {result})");
                }
            }
            catch (Exception ex)
            {
                InitError = ex.Message;
                Log($"Error al leer estado inicial: {ex.Message}");
            }
        }

        /// <summary>
        /// Busca en los 4 player indices un control conectado.
        /// </summary>
        private uint AutoDetectPlayerIndex()
        {
            for (uint i = 0; i < 4; i++)
            {
                try
                {
                    if (NativeGetState(i, out _) == ERROR_SUCCESS)
                    {
                        Log($"Auto-detect: control encontrado en index {i}");
                        return i;
                    }
                }
                catch
                {
                    // Continuar buscando
                }
            }

            Log("Auto-detect: ningun control encontrado, usando index 0");
            return 0;
        }

        public void Start()
        {
            _pollTimer.Start();
            Log("Polling iniciado");
        }

        public void Stop()
        {
            _pollTimer.Stop();
            Log("Polling detenido");
        }

        private int _reconnectCounter;

        private void Poll(object? sender, EventArgs e)
        {
            try
            {
                var result = NativeGetState(_playerIndex, out var state);

                if (result != ERROR_SUCCESS)
                {
                    if (_isConnected)
                    {
                        _isConnected = false;
                        Log($"Gamepad desconectado (index {_playerIndex})");
                    }
                    _prevButtons = 0;
                    _prevLeftX = 0;
                    _prevLeftY = 0;
                    _prevLeftTrigger = 0;
                    _prevRightTrigger = 0;

                    // Intentar re-detectar cada ~2 segundos (120 frames)
                    _reconnectCounter++;
                    if (_reconnectCounter >= 120)
                    {
                        _reconnectCounter = 0;
                        TryReconnect();
                    }
                    return;
                }

                if (!_isConnected)
                {
                    _isConnected = true;
                    _lastPacketNumber = state.PacketNumber;
                    Log($"Gamepad reconectado (index {_playerIndex})");
                }

                // Sticks analógicos: fire continuously (every poll) for smooth scrolling
                float lx = NormalizeThumb(state.Gamepad.ThumbLX);
                float ly = NormalizeThumb(state.Gamepad.ThumbLY);
                float rx = NormalizeThumb(state.Gamepad.ThumbRX);
                float ry = NormalizeThumb(state.Gamepad.ThumbRY);

                bool leftActive = Math.Abs(lx) > ThumbStickDeadZone || Math.Abs(ly) > ThumbStickDeadZone;
                bool rightActive = Math.Abs(rx) > ThumbStickDeadZone || Math.Abs(ry) > ThumbStickDeadZone;
                bool leftWasActive = Math.Abs(_prevLeftX) > ThumbStickDeadZone || Math.Abs(_prevLeftY) > ThumbStickDeadZone;

                // Fire axis events when active or when returning to center (so consumers can reset)
                if (leftActive || leftWasActive)
                    LeftStickAxis?.Invoke(lx, ly);
                if (rightActive)
                    RightStickAxis?.Invoke(rx, ry);

                _prevLeftX = lx;
                _prevLeftY = ly;

                // Optimización: si el paquete no cambió, no hay input nuevo de botones
                if (state.PacketNumber == _lastPacketNumber)
                    return;
                _lastPacketNumber = state.PacketNumber;

                var btns = state.Gamepad.Buttons;
                var prev = _prevButtons;

                // Botones de acción (nuevo mapeo)
                // B = Seleccionar/Confirmar
                if (Pressed(btns, prev, BTN_B))  SelectPressed?.Invoke();
                // A = Volver atrás
                if (Pressed(btns, prev, BTN_A))  BackPressed?.Invoke();
                // X = Ver imágenes del juego
                if (Pressed(btns, prev, BTN_X))  ImagesPressed?.Invoke();
                // Y = Editar opciones del juego
                if (Pressed(btns, prev, BTN_Y))  ManagePressed?.Invoke();

                // D-Pad
                if (Pressed(btns, prev, DPAD_UP))    NavigateUp?.Invoke();
                if (Pressed(btns, prev, DPAD_DOWN))  NavigateDown?.Invoke();
                if (Pressed(btns, prev, DPAD_LEFT))  NavigateLeft?.Invoke();
                if (Pressed(btns, prev, DPAD_RIGHT)) NavigateRight?.Invoke();

                // Select+Start combo detection (kill emulator/game)
                bool comboNow = (btns & (BTN_START | BTN_BACK)) == (BTN_START | BTN_BACK);
                bool comboPrev = (prev & (BTN_START | BTN_BACK)) == (BTN_START | BTN_BACK);

                if (comboNow && !comboPrev)
                {
                    KillComboPressed?.Invoke();
                }
                else if (!comboNow)
                {
                    // Only fire individual Start/Back if combo is NOT active
                    if (Pressed(btns, prev, BTN_START)) SelectPressed?.Invoke();
                    if (Pressed(btns, prev, BTN_BACK))  BackPressed?.Invoke();
                }

                // LB/RB = Página arriba/abajo
                if (Pressed(btns, prev, BTN_LB)) PageUpPressed?.Invoke();
                if (Pressed(btns, prev, BTN_RB)) PageDownPressed?.Invoke();

                // Triggers
                float lt = state.Gamepad.LeftTrigger / 255f;
                float rt2 = state.Gamepad.RightTrigger / 255f;

                // RT = Ejecutar juego
                if (rt2 > TriggerThreshold && _prevRightTrigger <= TriggerThreshold)
                    PlayPressed?.Invoke();
                // LT = Página arriba (alternativa)
                if (lt > TriggerThreshold && _prevLeftTrigger <= TriggerThreshold)
                    PageUpPressed?.Invoke();

                // Guardar estado
                _prevButtons = btns;
                _prevLeftTrigger = lt;
                _prevRightTrigger = rt2;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GamepadController] Poll error: {ex.Message}");
            }
        }

        private void TryReconnect()
        {
            for (uint i = 0; i < 4; i++)
            {
                try
                {
                    if (NativeGetState(i, out _) == ERROR_SUCCESS)
                    {
                        _playerIndex = i;
                        Log($"Reconnect: control encontrado en index {i}");
                        return;
                    }
                }
                catch { }
            }
        }

        private static bool Pressed(ushort current, ushort previous, ushort mask)
            => (current & mask) != 0 && (previous & mask) == 0;

        private static float NormalizeThumb(short value)
            => Math.Max(-1f, value / 32767f);

        private void Log(string message)
        {
            Debug.WriteLine($"[GamepadController] {message}");
            DiagnosticMessage?.Invoke(message);
        }

        /// <summary>
        /// Obtiene un resumen del estado actual para diagnóstico.
        /// </summary>
        public string GetDiagnosticInfo()
        {
            try
            {
                var info = $"PlayerIndex={_playerIndex}, Connected={_isConnected}";
                for (uint i = 0; i < 4; i++)
                {
                    var result = NativeGetState(i, out var state);
                    info += $"\n  Index {i}: {(result == ERROR_SUCCESS ? $"OK (btns=0x{state.Gamepad.Buttons:X4})" : $"Error {result}")}";
                }
                return info;
            }
            catch (Exception ex)
            {
                return $"Error al obtener diagnóstico: {ex.Message}";
            }
        }

        public void Dispose() => Stop();
    }
}
