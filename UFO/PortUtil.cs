using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFO
{
    class PortUtil
    {
        public static PortUtil Instance = new PortUtil();

        public SerialPort Port { get; private set; } = new SerialPort();

        public void SetPort(string portName)
        {
            Port.Close();
            Port = new SerialPort(portName, 19200, Parity.None, 8, StopBits.One);
            Port.Open();

            Port.DtrEnable = true;
            Port.RtsEnable = true;
            Port.ReadTimeout = 100;
        }

        readonly byte[] StopCmd = new byte[] { 0x02, 0x01, 0x00 };
        readonly byte[] CheckCmd = new byte[] { 0xF0, 0x01, 0x00 };

        /// <summary>
        /// ポート一覧を返す
        /// </summary>
        /// <returns></returns>
        public static string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// データ送信
        /// </summary>
        /// <param name="direction">正方向ならtrue</param>
        /// <param name="value">0 ~ 100</param>
        public void PushData(bool direction, int value)
        {
            if (!Port.IsOpen)
                return;

            if (value > 100)
                value = 100;
            else if (value < 0)
                value = 0;
            // 逆回転
            if (!direction)
                value = value | 0x80;

            byte[] data = new byte[] { 0x02, 0x01, (byte)value };
            Port.Write(data, 0, data.Length);
        }

        public void TypeCheck()
        {

            Port.Write(CheckCmd, 0, CheckCmd.Length);
        }

        public void Close()
        {
            Port.Close();
        }
    }
}
