﻿using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Platform;
using System;

namespace EpicRerouter.ExeSide
{
	public static class EpicPlatformManager
	{
		private const string _eosProductID = "prod-starfish";
		private const string _eosSandboxID = "starfish";
		private const string _eosDeploymentID = "e176ecc84fbc4dd8934664684f44dc71";
		private const string _eosClientID = "5c553c6accee4111bc8ea3a3ae52229b";
		private const string _eosClientSecret = "k87Nfp75BzPref4nJFnnbNjYXQQR";

		public static PlatformInterface PlatformInterface;
		public static EpicAccountId LocalUserId;

		public static OWEvent OnAuthSuccess = new(1);

		public static void Init()
		{
			if (PlatformInterface == null)
			{
				try
				{
					InitPlatform();
				}
				catch (EOSInitializeException ex)
				{
					if (ex.Result == Result.AlreadyConfigured)
					{
						Console.Error.WriteLine("[EOS] platform already configured!");
					}
				}
			}

			Auth();
		}

		public static void Tick() =>
			PlatformInterface.Tick();

		public static void Uninit()
		{
			PlatformInterface.Release();
			PlatformInterface = null;
			PlatformInterface.Shutdown();
		}

		private static void InitPlatform()
		{
			var result = PlatformInterface.Initialize(new InitializeOptions
			{
				ProductName = Program.ProductName,
				ProductVersion = Program.Version
			});
			if (result != Result.Success)
			{
				throw new EOSInitializeException("Failed to initialize Epic Online Services platform: ", result);
			}

			var options = new Options
			{
				ProductId = _eosProductID,
				SandboxId = _eosSandboxID,
				ClientCredentials = new ClientCredentials
				{
					ClientId = _eosClientID,
					ClientSecret = _eosClientSecret
				},
				DeploymentId = _eosDeploymentID
			};
			PlatformInterface = PlatformInterface.Create(options);
			Console.WriteLine("[EOS] Platform interface has been created");
		}

		private static void Auth()
		{
			Console.WriteLine("[EOS] Authenticating...");
			var loginOptions = new LoginOptions
			{
				Credentials = new Credentials
				{
					Type = LoginCredentialType.ExchangeCode,
					Id = null,
					Token = GetPasswordFromCommandLine()
				},
				ScopeFlags = 0
			};
			if (PlatformInterface == null)
			{
				Console.Error.WriteLine("[EOS] Platform interface is null!");
				return;
			}

			PlatformInterface.GetAuthInterface().Login(loginOptions, null, OnLogin);
		}

		private static string GetPasswordFromCommandLine()
		{
			var commandLineArgs = Environment.GetCommandLineArgs();
			foreach (var arg in commandLineArgs)
			{
				if (arg.Contains("AUTH_PASSWORD"))
				{
					return arg.Split('=')[1];
				}
			}

			return null;
		}

		private static void OnLogin(LoginCallbackInfo loginCallbackInfo)
		{
			if (loginCallbackInfo.ResultCode == Result.Success)
			{
				LocalUserId = loginCallbackInfo.LocalUserId;
				Console.WriteLine($"[EOS SDK] login success! user ID: {LocalUserId}");
				OnAuthSuccess.Invoke();
				return;
			}

			Console.Error.WriteLine("[EOS SDK] Login failed");
		}

		private class EOSInitializeException : Exception
		{
			public readonly Result Result;

			public EOSInitializeException(string msg, Result initResult) :
				base(msg) =>
				Result = initResult;
		}
	}
}
