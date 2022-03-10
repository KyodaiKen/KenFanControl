using KenFanControl.DataStructures;
using KenFanControl.DriverHelpers;
using Microsoft.Extensions.Logging;
using RJCP.IO.Ports;
using System.Buffers;
using System.Text;

namespace KenFanControl
{
    public class FanController : IDisposable
    {
        #region "Events"
        public delegate void ErrorEvent(byte DeviceId, string description, Memory<byte> Data);
        public event ErrorEvent? OnError;

        public delegate void WarningEvent(byte DeviceId, string description, Memory<byte> Data);
        public event WarningEvent? OnWarning;
        #endregion "Events"

        #region Private locals

        private static readonly MemoryPool<byte> MemoryPool = MemoryPool<byte>.Shared;
        private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;

        private readonly SerialPortStream SerialPort;
        private bool Listening;
        private bool disposedValue;
        private readonly EventWaitHandle waitHandle = new AutoResetEvent(true);

        private readonly ILogger? Logger;
        #endregion

        #region Properties
        public byte DeviceID { get; set; }
        public string DeviceName { get; set; }

        public bool EEPROM_OK { get; private set; }

        public DeviceCapabilities DeviceCapabilities { get; set; }
        public ControllerConfig ControllerConfig { get; set; }
        public FanControlConfig FanControlConfig { get; set; }
        #endregion

        internal FanController(SerialPortStream SerialPort, byte DeviceID, ILogger? Logger = null)
        {
            this.Logger = Logger;
            this.SerialPort = SerialPort;
            this.DeviceID = DeviceID;
        }

        #region Listener
        public void StartListening()
        {
            if (Listening)
            {
                return;
            }

            if (!SerialPort.IsOpen)
            {
                SerialPort.Open();
            }

            Listening = true;
        }

        public void StopListening()
        {
            if (!Listening)
            {
                return;
            }

            if (SerialPort.IsOpen)
            {
                SerialPort.Close();
                SerialPort.Dispose();
            }

            Listening = false;
        }

        private async Task<ReadResults> ProcessCommand(byte command, byte[]? payload = null)
        {
#warning consider using timeout if everything goes wrong
            waitHandle.WaitOne();
            await SendCommand(command, payload);
            var readResults = await ReadAnswer();
            waitHandle.Set();
            return readResults;
        }

        // Temporal error till we migrate all the methods
        [Obsolete($"Replace with '{nameof(ProcessCommand)}' call", true)]
        private async Task SendCommand(byte command, byte[]? payload = null)
        {
#warning consider using "ArrayPool"
            int payload_len = 0;
            if (payload != null)
                payload_len = payload.Length;

            var preparedCommand = new byte[payload_len + 1];
            preparedCommand[0] = command;

            if (payload is not null)
            {
                if (payload.Length > Protocol.MaxPayloadSize)
                {
                    throw new ArgumentException($"Payload '{payload.Length}' is too large. Maximum allowed is '{Protocol.MaxPayloadSize}'");
                }

                Array.Copy(payload, 0, preparedCommand, 1, payload.Length);
            }

            await SerialPort.SendCommand(preparedCommand);
        }

        private async Task<ReadResults> ReadAnswer()
        {
            var buffer = MemoryPool.Rent(Protocol.BufferSize);

            var results = new ReadResults()
            {
                ReadStatus = ReadStatus.Ok,
            };

            // Proposal => Read till ending pattern is found, use if needed to process larger inputs that needs several packages to arrive

#warning maybe adding a cancelation task is a good idea, just in case it doesn't automatically do cleanup
            var readTask = SerialPort.ReadAsync(buffer.Memory).AsTask();

            if (await Task.WhenAny(readTask, Task.Delay(Protocol.Timeout)) != readTask)
            {
                const string errmsg = "Data receive timeout";
                Logger?.LogWarning(errmsg);
                OnWarning?.Invoke(DeviceID, errmsg, null);

                buffer.Dispose();

                //Abort operation

                results.ReadStatus = ReadStatus.Error;
                return results;
            }

            var readDataLength = await readTask;

            if (readDataLength < 1)
            {
                const string errmsg = "No data received!";
                Logger?.LogError(errmsg);
                OnError?.Invoke(DeviceID, errmsg, null);

                buffer.Dispose();

                //Abort operation
                results.ReadStatus = ReadStatus.Error;
                return results;
            }

            var data = buffer.Memory[..readDataLength];

            Logger?.LogDebug($"Data Received {Convert.ToHexString(data.Span)}");

            // Status Byte
            var status = data.Span[0];

            if (status == Protocol.Status.RESP_ERR)
            {
                const string errmsg = "Error received";
                Logger?.LogError(errmsg, data);
                OnError?.Invoke(DeviceID, errmsg, data);

                buffer.Dispose();

                // Abort Operation
                results.ReadStatus = ReadStatus.Error;
                results.Data = data;
                return results;
            }

            if (status == Protocol.Status.RESP_WRN)
            {
                buffer.Dispose();

                const string errmsg = "Warning received";
                Logger?.LogWarning(errmsg, data);
                OnWarning?.Invoke(DeviceID, errmsg, data);

                // Abort Operation
                results.ReadStatus = ReadStatus.Warning;
            }

            // Kind of response Byte Id
            results.ResponseKind = data.Span[1];
            results.Data = data;
            return results;
        }

        internal async Task InitializeDevice()
        {
            StartListening();

            DeviceCapabilities = await GetDeviceCapabilities();
            ControllerConfig = await GetControllerConfig();

            //Gather FanControlConfig
            FanControlConfig = new FanControlConfig
            {
                Curves = new Curve[DeviceCapabilities.NumberOfChannels],
                Matrixes = new Matrix[DeviceCapabilities.NumberOfChannels]
            };

            for (byte c = 0; c < DeviceCapabilities.NumberOfChannels; c++)
            {
                FanControlConfig.Curves[c] = await GetCurve(c);
                FanControlConfig.Matrixes[c] = await GetMatrix(c);
            }
        }

        #endregion

        #region Get Commands
        private async Task<DeviceCapabilities> GetDeviceCapabilities()
        {
            const byte commandKey = Protocol.Request.RQST_CAPABILITIES;

            Logger?.LogInformation($"Sending get capabilities command...");

            var result = await ProcessCommand(commandKey);

            if (result.ReadStatus == ReadStatus.Error)
            {

            }
            else if (result.ReadStatus == ReadStatus.Warning)
            {

            }
            else
            {

            }

            var dc = new DeviceCapabilities();

            dc.Deserialize(result.DataSpan);

            Logger?.LogInformation($"NumberOfSensors => {dc.NumberOfSensors}, NumberOfChannels => {dc.NumberOfChannels}");

            return dc;
        }

        public async Task<ControllerConfig> GetControllerConfig()
        {
            byte commandKey = Protocol.Request.RQST_GET_CAL_RESISTRS;

            Logger?.LogInformation($"Sending get calibration resistor values command...");

            var resultCalResist = await ProcessCommand(commandKey);

            if (resultCalResist.ReadStatus == ReadStatus.Error)
            {

            }
            else if (resultCalResist.ReadStatus == ReadStatus.Warning)
            {

            }
            else
            {

            }

            commandKey = Protocol.Request.RQST_GET_CAL_OFFSETS;
            Logger?.LogInformation($"Sending get calibration offset values command...");

            var resultCalOffsets = await ProcessCommand(commandKey);

            if (resultCalOffsets.ReadStatus == ReadStatus.Error)
            {

            }
            else if (resultCalOffsets.ReadStatus == ReadStatus.Warning)
            {

            }
            else
            {

            }

            commandKey = Protocol.Request.RQST_GET_CAL_SH_COEFFS;
            Logger?.LogInformation($"Sending get calibration Steinhart-Hart coefficients command...");

            var resultSteinhartHartCoefficients = await ProcessCommand(commandKey);

            if (resultSteinhartHartCoefficients.ReadStatus == ReadStatus.Error)
            {

            }
            else if (resultSteinhartHartCoefficients.ReadStatus == ReadStatus.Warning)
            {

            }
            else
            {

            }

            commandKey = Protocol.Request.RQST_GET_PINS;
            Logger?.LogInformation($"Sending get thermistor and PWM channel pin numbers command...");

            var resultPins = await ProcessCommand(commandKey);

            if (resultPins.ReadStatus == ReadStatus.Error)
            {

            }
            else if (resultPins.ReadStatus == ReadStatus.Warning)
            {

            }
            else
            {

            }

            var cc = new ControllerConfig();

            cc.Deserialize(new ControllerConfigDeserializationHelper()
            {
                RawOffsets = resultCalOffsets.DataSpan,
                RawResistors = resultCalResist.DataSpan,
                RawSteinhartHartCoefficients = resultSteinhartHartCoefficients.DataSpan,
                RawPins = resultPins.DataSpan,
            }, DeviceCapabilities);

            commandKey = Protocol.Request.RQST_GET_PINS;
            
            cc.PWMChannels = new PWMChannel[DeviceCapabilities.NumberOfChannels];

#warning shouldn't this be the first call?
            await CheckEEPROMHealth();

            return cc;
        }

        public async Task CheckEEPROMHealth()
        {
            //Check EEPROM health
            var commandKey = Protocol.Request.RQST_GET_EERPOM_HEALTH;
            Logger?.LogInformation($"Sending get EEPROM health command...");

            var result = await ProcessCommand(commandKey);

            if (result.ReadStatus == ReadStatus.Ok)
            {
                EEPROM_OK = true;
                Logger?.LogInformation("EEPROM OK!");
                return;
            }

            EEPROM_OK = false;
            Logger?.LogWarning($"The controller had an EEPROM error and was reset to factory defaults!");
        }

        public async Task<Readings> GetReadings()
        {
            const byte commandKey = Protocol.Request.RQST_GET_SENSOR_READINGS;

            Logger?.LogInformation($"Sending get readings command...");

            var result = await ProcessCommand(commandKey);

            if (result.ReadStatus == ReadStatus.Error)
            {

            }
            else if (result.ReadStatus == ReadStatus.Warning)
            {

            }
            else
            {

            }

            var Readings = new Readings();

            Readings.Deserialize(result.DataSpan, DeviceCapabilities);

            return Readings;
        }

        public async Task<Curve> GetCurve(byte channelId)
        {
            const byte commandKey = Protocol.Request.RQST_GET_CURVE;

            Logger?.LogInformation($"Sending get curve command for curve ID {channelId}...");

            byte[] payload = new byte[1] { channelId };

            var result = await ProcessCommand(commandKey, payload);

            if (result.ReadStatus == ReadStatus.Error)
            {

            }
            else if (result.ReadStatus == ReadStatus.Warning)
            {

            }
            else
            {

            }

            var curve = new Curve();

            curve.Deserialize(result.DataSpan, channelId);

            return curve;
        }

        public async Task<Matrix> GetMatrix(byte channelId)
        {
            const byte commandKey = Protocol.Request.RQST_GET_MATRIX;
            
            byte[] payload = new byte[1] { channelId };

            var result = await ProcessCommand(commandKey, payload);

            if (result.ReadStatus == ReadStatus.Error)
            {

            }
            else if (result.ReadStatus == ReadStatus.Warning)
            {

            }
            else
            {

            }

            var matrixObj = new Matrix();

            matrixObj.Deserialize(result.DataSpan, channelId, DeviceCapabilities);

            return matrixObj;
        }
        #endregion

        #region Set Commands
        public async Task<bool> SetCurve(byte curveID, Curve curve)
        {
            const byte commandKey = Protocol.Request.RQST_SET_CURVE;

            Logger?.LogInformation($"Sending set curve command...");

            var data = curve.Serialize();

            var payload = curveID.Concatenate(data);

            Logger?.LogDebug($"Sending payload {Convert.ToHexString(payload)}");

            var result = await ProcessCommand(commandKey, payload);

            if (result.ReadStatus == ReadStatus.Error)
            {
                // Maybe throw?
                Logger?.LogError("Err!");
                return false;
            }
            else if (result.ReadStatus == ReadStatus.Warning)
            {
                Logger?.LogWarning("Warn!");
            }
            else
            {
                Logger?.LogInformation("OK!");
            }

            return true;
        }

        public async Task<bool> SetMatrix(byte curveID, Matrix matrix)
        {
            const byte commandKey = Protocol.Request.RQST_SET_MATRIX;

            Logger?.LogInformation($"Sending set matrix command...");

            byte[] payload = new byte[13];

            payload[0] = curveID;

            for (int i = 0; i < 3; i++)
            {
                Array.Copy(BitConverter.GetBytes(matrix.MatrixPoints[i]), 0, payload, i * 4 + 1, 4);
            }

            Logger?.LogDebug($"Sending payload {Convert.ToHexString(payload)}");

            await SendCommand(commandKey, payload);

            await DoWhenAnswerRecivedWithTimeoutAsync(commandKey, (data) =>
            {
                Logger?.LogInformation("OK!");
            });

            return true;
        }

        public async Task<bool> SetControllerConfig(ControllerConfig ControllerConfig)
        {
            byte commandKey = Protocol.Request.RQST_SET_CAL_RESISTRS;

            Logger?.LogInformation($"Sending set calibration resistor values command...");

            byte[] payload = new byte[DeviceCapabilities.NumberOfSensors * 4];

            for (int i = 0; i < DeviceCapabilities.NumberOfSensors; i++)
            {
                Array.Copy(BitConverter.GetBytes(ControllerConfig.ThermalSensors[i].CalibrationResistorValue), 0, payload, i * 4, 4);
            }

            Logger?.LogDebug($"Sending payload {Convert.ToHexString(payload)}");

            await SendCommand(commandKey, payload);

            await DoWhenAnswerRecivedWithTimeoutAsync(commandKey, (data) =>
            {
                Logger?.LogInformation("OK!");
            });

            commandKey = Protocol.Request.RQST_SET_CAL_OFFSETS;
            Logger?.LogInformation($"Sending set calibration offsets command...");

            for (int i = 0; i < DeviceCapabilities.NumberOfSensors; i++)
            {
                Array.Copy(BitConverter.GetBytes(ControllerConfig.ThermalSensors[i].CalibrationOffset), 0, payload, i * 4, 4);
            }

            Logger?.LogDebug($"Sending payload {Convert.ToHexString(payload)}");

            await SendCommand(commandKey, payload);

            await DoWhenAnswerRecivedWithTimeoutAsync(commandKey, (data) =>
            {
                Logger?.LogInformation("OK!");
            });

            commandKey = Protocol.Request.RQST_SET_CAL_SH_COEFFS;
            Logger?.LogInformation($"Sending set calibration Steinhart-Hart coefficients command...");

            payload = new byte[DeviceCapabilities.NumberOfSensors * 12];

            for (int i = 0; i < DeviceCapabilities.NumberOfSensors; i++)
            {
                Array.Copy(BitConverter.GetBytes(ControllerConfig.ThermalSensors[i].CalibrationSteinhartHartCoefficients[0]), 0, payload, i * 12, 4);
                Array.Copy(BitConverter.GetBytes(ControllerConfig.ThermalSensors[i].CalibrationSteinhartHartCoefficients[1]), 0, payload, i * 12 + 4, 4);
                Array.Copy(BitConverter.GetBytes(ControllerConfig.ThermalSensors[i].CalibrationSteinhartHartCoefficients[2]), 0, payload, i * 12 + 8, 4);
            }

            Logger?.LogDebug($"Sending payload {Convert.ToHexString(payload)}");

            await SendCommand(commandKey, payload);

            await DoWhenAnswerRecivedWithTimeoutAsync(commandKey, (data) =>
            {
                Logger?.LogInformation("OK!");
            });

            commandKey = Protocol.Request.RQST_SET_PINS;
            Logger?.LogInformation($"Sending set pins command...");

            payload = new byte[DeviceCapabilities.NumberOfChannels + DeviceCapabilities.NumberOfSensors];

            for (int i = 0; i < DeviceCapabilities.NumberOfSensors; i++)
            {
                payload[i] = ControllerConfig.ThermalSensors[i].Pin;
            }

            for (int i = 0; i < DeviceCapabilities.NumberOfChannels; i++)
            {
                payload[i + 3] = ControllerConfig.PWMChannels[i].Pin;
            }

            Logger?.LogDebug($"Sending payload {Convert.ToHexString(payload)}");

            await SendCommand(commandKey, payload);

            await DoWhenAnswerRecivedWithTimeoutAsync(commandKey, (data) =>
            {
                Logger?.LogInformation("OK!");
            });

            return true;
        }
        #endregion

        #region Requests
        public async Task<bool> RequestStoreToEEPROM()
        {
            const byte commandKey = Protocol.Request.RQST_WRITE_TO_EEPROM;

            Logger?.LogInformation($"Sending RQST_WRITE_TO_EEPROM...");
            await SendCommand(commandKey);

            await DoWhenAnswerRecivedWithTimeoutAsync(commandKey, (data) =>
            {
                Logger?.LogInformation("OK!");
            });

            return true;
        }

        public async Task<bool> RequestReadFromEEPROM()
        {
            const byte commandKey = Protocol.Request.RQST_READ_FROM_EEPROM;

            Logger?.LogInformation($"Sending RQST_READ_FROM_EEPROM...");
            await SendCommand(commandKey);

            await DoWhenAnswerRecivedWithTimeoutAsync(commandKey, (data) =>
            {
                Logger?.LogInformation("OK!");
            });

            return true;
        }
        #endregion

        #region IDisposable Implementation
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StopListening();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~FanController()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}