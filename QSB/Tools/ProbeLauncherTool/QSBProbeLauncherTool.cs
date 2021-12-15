﻿using QSB.Utility;
using UnityEngine;

namespace QSB.Tools.ProbeLauncherTool
{
	public class QSBProbeLauncherTool : QSBTool
	{
		public GameObject PreLaunchProbeProxy;
		public ProbeLauncherEffects Effects;
		public SingularityWarpEffect ProbeRetrievalEffect;

		public void RetrieveProbe(bool playEffects)
		{
			DebugLog.DebugWrite($"{Player.PlayerId} retrieveProbe");

			if (Effects._owAudioSource == null)
			{
				Effects._owAudioSource = Player.AudioController._repairToolSource;
			}

			PreLaunchProbeProxy.SetActive(true);
			if (playEffects)
			{
				Effects.PlayRetrievalClip();
				ProbeRetrievalEffect.WarpObjectIn(0.3f);
			}
		}

		public void LaunchProbe()
		{
			DebugLog.DebugWrite($"{Player.PlayerId} launchProbe");
			PreLaunchProbeProxy.SetActive(false);

			if (Effects._owAudioSource == null)
			{
				Effects._owAudioSource = Player.AudioController._repairToolSource;
			}

			// TODO : make this do underwater stuff correctly
			Effects.PlayLaunchClip(false);
			Effects.PlayLaunchParticles(false);
		}
	}
}
