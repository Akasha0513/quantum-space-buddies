﻿using OWML.Utils;
using QSB.Events;
using QSB.Patches;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.ShipSync.Patches
{
	class ShipPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.HarmonyHelper.AddPrefix<HatchController>("OnPressInteract", typeof(ShipPatches), nameof(HatchController_OnPressInteract));
			QSBCore.HarmonyHelper.AddPrefix<HatchController>("OnEntry", typeof(ShipPatches), nameof(HatchController_OnEntry));
			QSBCore.HarmonyHelper.AddPrefix<ShipTractorBeamSwitch>("OnTriggerExit", typeof(ShipPatches), nameof(ShipTractorBeamSwitch_OnTriggerExit));
			QSBCore.HarmonyHelper.AddPrefix<InteractZone>("UpdateInteractVolume", typeof(ShipPatches), nameof(InteractZone_UpdateInteractVolume));
		}

		public override void DoUnpatches()
		{
			QSBCore.HarmonyHelper.Unpatch<HatchController>("OnPressInteract");
			QSBCore.HarmonyHelper.Unpatch<HatchController>("OnEntry");
			QSBCore.HarmonyHelper.Unpatch<ShipTractorBeamSwitch>("OnTriggerExit");
			QSBCore.HarmonyHelper.Unpatch<InteractZone>("UpdateInteractVolume");
		}

		public static bool HatchController_OnPressInteract()
		{
			if (!PlayerState.IsInsideShip())
			{
				ShipManager.Instance.ShipTractorBeam.ActivateTractorBeam();
			}
			QSBEventManager.FireEvent(EventNames.QSBHatchState, true);
			return true;
		}

		public static bool HatchController_OnEntry(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				QSBEventManager.FireEvent(EventNames.QSBHatchState, false);
			}
			return true;
		}

		public static bool ShipTractorBeamSwitch_OnTriggerExit(Collider hitCollider, bool ____isPlayerInShip, bool ____functional)
		{
			if (!____isPlayerInShip && ____functional && hitCollider.CompareTag("PlayerDetector") && !ShipManager.Instance.HatchController.GetValue<GameObject>("_hatchObject").activeSelf)
			{
				ShipManager.Instance.HatchController.Invoke("CloseHatch");
				ShipManager.Instance.ShipTractorBeam.DeactivateTractorBeam();
				QSBEventManager.FireEvent(EventNames.QSBHatchState, false);
			}
			return false;
		}

		public static bool InteractZone_UpdateInteractVolume(InteractZone __instance, OWCamera ____playerCam, ref bool ____focused)
		{
			/* Angle for interaction with the ship hatch
			 *  
			 *  \  80°  / - If in ship
			 *   \     /
			 *    \   /
			 *   [=====]  - Hatch
			 *    /   \
			 *   /     \
			 *  / 280°  \ - If not in ship
			 *  
			 */

			if (!QSBCore.WorldObjectsReady || __instance != ShipManager.Instance.HatchInteractZone)
			{
				return true;
			}

			var angle = 2f * Vector3.Angle(____playerCam.transform.forward, __instance.transform.forward);

			____focused = PlayerState.IsInsideShip() 
				? angle <= 80 
				: angle >= 280;

			__instance.CallBase<InteractZone, SingleInteractionVolume>("UpdateInteractVolume");

			return false;
		}
	}
}