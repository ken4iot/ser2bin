using System;
using System.IO;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace SER2BIN
{
	class MainClass
	{
		public static int BUFFER_SIZE = 16 * 1024 * 1024;

		public static void PrintUsage ()
		{
			Console.WriteLine();
			Console.WriteLine("SER2BIN <port> <baud_rate> <time_out>");
			Console.WriteLine();
			Console.WriteLine("     port  序列埠裝置名稱或是通訊埠名稱。");
			Console.WriteLine("           Mac 的裝置名稱請用 ls /dev 列出，例如 cu.usbserial-DA01R0JG");
			Console.WriteLine("           Windows 請用 COM1, COM2, ... 的通訊埠名稱");
			Console.WriteLine();
			Console.WriteLine("baud_rate  傳輸速率。請用 115200 或是 57600。");
			Console.WriteLine();
			Console.WriteLine("time_out   等待幾毫秒之後，如果沒有接收到資料，程式會立刻結束。");
			Console.WriteLine("           如果 time_out 設為 10000 毫秒，代表等待 10 秒鐘。");
		}

		public static bool IsNumber (string Value)
		{
			return Regex.IsMatch(Value, @"^\d+$");
		}

		public static void CheckParams (string[] args)
		{
			bool Result = true;
			Result = (args.Length == 3);
			if (Result) {
				string PortName = args [0];
				Result = Result & (PortName.Length > 0);
				Result = Result & (IsNumber(args [1]));
				Result = Result & (IsNumber(args [2]));
			}

			if (!Result) {
				Console.WriteLine("參數錯誤!");
				PrintUsage();
				Environment.Exit(1);
			}
		}


		public static void Main (string[] args)
		{
			CheckParams(args);

			var SerialPort = new SerialPort ();
			SerialPort.PortName = args [0];
			SerialPort.BaudRate = int.Parse(args [1]);
			SerialPort.Parity = Parity.None;
			SerialPort.DataBits = 8;
			SerialPort.StopBits = StopBits.One;
			SerialPort.ReadTimeout = int.Parse(args [2]);
			SerialPort.ReadBufferSize = BUFFER_SIZE;
			int BytesReceived = 0;
			byte[] ReadBuffer = new byte[BUFFER_SIZE];
			int BufferIndex = 0;

			try {
				SerialPort.Open();
				Console.WriteLine("{0} 已開啟，等待資料中...", args [0]);

				string OutputFileName = DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".bin";
				var OutputFile = new FileStream (OutputFileName, FileMode.CreateNew);
				Console.WriteLine("已建立輸出檔  " + OutputFileName);

				while (true) {
					try {
						int n = SerialPort.ReadByte();
						if (n == -1)
							break;
						BytesReceived++;
						if (BufferIndex < BUFFER_SIZE) {
							ReadBuffer [BufferIndex++] = (byte)n;
						} else {
							Console.Write("*");
							OutputFile.Write(ReadBuffer, 0, BufferIndex);
							BufferIndex = 0;
							ReadBuffer [BufferIndex++] = (byte)n;
						}
					} catch {
						break;
					}
				}
				if (BufferIndex > 0) {
					OutputFile.Write(ReadBuffer, 0, BufferIndex);
				}
				OutputFile.Flush();
				OutputFile.Close();

				SerialPort.Close();
				Console.WriteLine("\r\n接收完畢");
				Console.WriteLine("總共收到 {0} bytes", BytesReceived);
			} catch {
				Console.WriteLine("開啟通訊埠或裝置 {0} 時，發生錯誤。", SerialPort.PortName);
				PrintUsage();
			}
		}
	}
}