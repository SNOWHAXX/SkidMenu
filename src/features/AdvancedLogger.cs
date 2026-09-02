using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace SkidMenu.features
{
	public static class AdvancedLogger
	{
		private static readonly object _lock = new object();
		private static StreamWriter _writer;
		private static bool _failed;

		public static string LogDirectory => Path.Combine(Paths.BepInExRootPath, "Logs", "SkidMenu");
		public static string CurrentLogFile { get; private set; }
		public static string UnityLogPath { get; private set; }

		public static void Init()
		{
			try
			{
				Directory.CreateDirectory(LogDirectory);
				CurrentLogFile = Path.Combine(LogDirectory, $"Advanced_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");
				UnityLogPath = Application.consoleLogPath;

				BepInEx.Logging.Logger.Listeners.Add(new BepInExListener());

				Header($"AdvancedLogger initialized. SkidMenu {SkidMenu.hyperVersion} ({SkidMenu.hyperBuild})");
				Header($"Among Us version: {Application.version}");
				Header($"Log file: {CurrentLogFile}");
				Header($"Unity log file: {UnityLogPath}");
			}
			catch (Exception ex)
			{
				SkidMenu.Log.LogWarning($"[AdvancedLogger] Init failed: {ex.Message}");
			}
		}

		public static void OpenLogFolder()
		{
			try
			{
				Directory.CreateDirectory(LogDirectory);
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = LogDirectory, UseShellExecute = true });
			}
			catch { }
		}

		public static void SaveUnityLog()
		{
			try
			{
				if (string.IsNullOrEmpty(UnityLogPath) || !File.Exists(UnityLogPath)) return;
				Directory.CreateDirectory(LogDirectory);
				var dest = Path.Combine(LogDirectory, $"Unity_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");
				File.Copy(UnityLogPath, dest, true);
				Info($"Copied Unity log to {dest}");
			}
			catch { }
		}

		public static void Info(string msg) => Write("[INFO]", msg);
		public static void Warn(string msg) => Write("[WARN]", msg);
		public static void Error(string msg) => Write("[ERROR]", msg);
		public static void Mirror(string msg) => Write("[CONSOLE]", msg);
		public static void Rpc(string caller, string rpc, string detail = "") => Write("[RPC]", $"{caller} -> {rpc}{(detail.Length > 0 ? " | " + detail : "")}");
		public static void Event(string name, string detail = "") => Write("[EVENT]", $"{name}{(string.IsNullOrEmpty(detail) ? "" : " | " + detail)}");

		private static void Header(string msg) => Write("[HEADER]", msg);

		private static void Write(string level, string msg)
		{
			if (!SkidMenu.advancedLogging) return;
			if (string.IsNullOrEmpty(CurrentLogFile) || _failed) return;
			try
			{
				lock (_lock)
				{
					_writer ??= new StreamWriter(CurrentLogFile, true) { AutoFlush = true };
					_writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {level} {msg}");
				}
			}
			catch
			{
				_failed = true;
			}
		}

		private sealed class BepInExListener : ILogListener
		{
			public LogLevel LogLevelFilter => LogLevel.All;
			public void LogEvent(object sender, LogEventArgs eventArgs)
			{
				if (eventArgs.Data?.ToString() is not { } data) return;
				if (data.Length == 0) return;
				switch (eventArgs.Level)
				{
					case LogLevel.Error:
					case LogLevel.Fatal:
						Error(data);
						break;
					case LogLevel.Warning:
						Warn(data);
						break;
					default:
						Info(data);
						break;
				}
			}
			public void Dispose() { }
		}
	}
}
