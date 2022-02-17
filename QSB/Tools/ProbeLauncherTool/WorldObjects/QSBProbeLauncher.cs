﻿using Cysharp.Threading.Tasks;
using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.Messages;
using QSB.WorldSync;
using System.Threading;

namespace QSB.Tools.ProbeLauncherTool.WorldObjects
{
	internal class QSBProbeLauncher : WorldObject<ProbeLauncher>
	{
		public override async UniTask Init(CancellationToken ct) =>
			AttachedObject.OnLaunchProbe += OnLaunchProbe;

		public override void OnRemoval() =>
			AttachedObject.OnLaunchProbe -= OnLaunchProbe;

		public override void SendInitialState(uint to)
		{
			if (AttachedObject._preLaunchProbeProxy.activeSelf)
			{
				this.SendMessage(new RetrieveProbeMessage(false));
			}
			else
			{
				this.SendMessage(new LaunchProbeMessage(false));
			}
		}

		private void OnLaunchProbe(SurveyorProbe probe) =>
			this.SendMessage(new LaunchProbeMessage(true));

		public void RetrieveProbe(bool playEffects)
		{
			if (AttachedObject._preLaunchProbeProxy.activeSelf)
			{
				return;
			}

			AttachedObject._preLaunchProbeProxy.SetActive(true);
			if (playEffects)
			{
				AttachedObject._effects.PlayRetrievalClip();
				AttachedObject._probeRetrievalEffect.WarpObjectIn(AttachedObject._probeRetrievalLength);
			}
		}

		public void LaunchProbe(bool playEffects)
		{
			if (!AttachedObject._preLaunchProbeProxy.activeSelf)
			{
				return;
			}

			AttachedObject._preLaunchProbeProxy.SetActive(false);

			if (playEffects)
			{
				// TODO : make this do underwater stuff correctly
				AttachedObject._effects.PlayLaunchClip(false);
				AttachedObject._effects.PlayLaunchParticles(false);
			}
		}
	}
}
