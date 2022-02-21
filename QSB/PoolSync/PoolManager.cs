﻿using Cysharp.Threading.Tasks;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;

namespace QSB.PoolSync
{
	internal class PoolManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
		{
			foreach (var streaming in QSBWorldSync.GetUnityObjects<NomaiRemoteCameraStreaming>().SortDeterministic())
			{
				streaming.gameObject.AddComponent<CustomNomaiRemoteCameraStreaming>();
			}

			foreach (var camera in QSBWorldSync.GetUnityObjects<NomaiRemoteCamera>().SortDeterministic())
			{
				camera.gameObject.AddComponent<CustomNomaiRemoteCamera>();
			}

			foreach (var platform in QSBWorldSync.GetUnityObjects<NomaiRemoteCameraPlatform>().SortDeterministic())
			{
				platform.gameObject.AddComponent<CustomNomaiRemoteCameraPlatform>();
			}
		}

		public override void UnbuildWorldObjects()
		{
			foreach (var platform in QSBWorldSync.GetUnityObjects<CustomNomaiRemoteCameraPlatform>())
			{
				Destroy(platform);
			}

			foreach (var camera in QSBWorldSync.GetUnityObjects<CustomNomaiRemoteCamera>())
			{
				Destroy(camera);
			}

			foreach (var streaming in QSBWorldSync.GetUnityObjects<CustomNomaiRemoteCameraStreaming>())
			{
				Destroy(streaming);
			}
		}
	}
}
