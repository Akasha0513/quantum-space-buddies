﻿using HarmonyLib;
using QSB.Animation.NPC.WorldObjects;
using QSB.Events;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Animation.NPC.Patches
{
	[HarmonyPatch]
	public class SolanumPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SolanumAnimController), nameof(SolanumAnimController.LateUpdate))]
		public static bool SolanumLateUpdateReplacement(SolanumAnimController __instance)
		{
			if (__instance._animatorStateEvents == null)
			{
				__instance._animatorStateEvents = __instance._animator.GetBehaviour<AnimatorStateEvents>();
				__instance._animatorStateEvents.OnEnterState += __instance.OnEnterAnimatorState;
			}

			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBSolanumAnimController, SolanumAnimController>(__instance);
			var playersInHeadZone = qsbObj.GetPlayersInHeadZone();

			var targetCamera = playersInHeadZone == null || playersInHeadZone.Count == 0
				? __instance._playerCameraTransform
				: QSBPlayerManager.GetClosestPlayerToWorldPoint(playersInHeadZone, __instance.transform.position).CameraBody.transform;

			var targetValue = Quaternion.LookRotation(targetCamera.position - __instance._headBoneTransform.position, __instance.transform.up);
			__instance._currentLookRotation = __instance._lookSpring.Update(__instance._currentLookRotation, targetValue, Time.deltaTime);

			var position = __instance._headBoneTransform.position + (__instance._currentLookRotation * Vector3.forward);
			__instance._localLookPosition = __instance.transform.InverseTransformPoint(position);

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiConversationManager), nameof(NomaiConversationManager.OnEnterWatchVolume))]
		public static bool EnterWatchZone(NomaiConversationManager __instance)
		{
			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBSolanumAnimController, SolanumAnimController>(__instance._solanumAnimController);
			QSBEventManager.FireEvent(EventNames.QSBEnterNomaiHeadZone, qsbObj.ObjectId);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiConversationManager), nameof(NomaiConversationManager.OnExitWatchVolume))]
		public static bool ExitWatchZone(NomaiConversationManager __instance)
		{
			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBSolanumAnimController, SolanumAnimController>(__instance._solanumAnimController);
			QSBEventManager.FireEvent(EventNames.QSBExitNomaiHeadZone, qsbObj.ObjectId);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiConversationManager), nameof(NomaiConversationManager.Update))]
		public static bool ReplacementUpdate(NomaiConversationManager __instance)
		{
			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBSolanumAnimController, SolanumAnimController>(__instance._solanumAnimController);
			__instance._playerInWatchVolume = qsbObj.GetPlayersInHeadZone().Any();

			if (!__instance._initialized)
			{
				__instance.InitializeNomaiText();
			}

			//var heldItem = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
			//var holdingConversationStone = heldItem != null && heldItem is NomaiConversationStone;
			var holdingConversationStone = QSBPlayerManager.GetPlayerCarryItems().Any(x => x.Item2 != null && x.Item2.GetItemType() == ItemType.ConversationStone);

			switch (__instance._state)
			{
				case NomaiConversationManager.State.WatchingSky:
					if (__instance._playerInWatchVolume)
					{
						DebugLog.DebugWrite($"{__instance._state} => WACHING PLAYER");
						__instance._state = NomaiConversationManager.State.WatchingPlayer;
						__instance._solanumAnimController.StartWatchingPlayer();
					}
					break;

				case NomaiConversationManager.State.WatchingPlayer:
					if (!__instance._solanumAnimController.isPerformingAction)
					{
						// player left watch zone
						if (!__instance._playerInWatchVolume)
						{
							DebugLog.DebugWrite($"{__instance._state} => WATCHING SKY");
							__instance._state = NomaiConversationManager.State.WatchingSky;
							__instance._solanumAnimController.StopWatchingPlayer();
						}
						else if (__instance._dialogueComplete)
						{
							// create conversation stones
							if (__instance._dialogueComplete && !__instance._conversationStonesCreated)
							{
								__instance._stoneCreationTimer -= Time.deltaTime;
								if (__instance._stoneCreationTimer <= 0f)
								{
									DebugLog.DebugWrite($"{__instance._state} => CREATING STONES");
									__instance._state = NomaiConversationManager.State.CreatingStones;
									__instance._solanumAnimController.PlayCreateWordStones();
								}
							}
							else if (__instance._conversationStonesCreated && !__instance._cairnRaised)
							{
								if (!holdingConversationStone)
								{
									__instance._stoneGestureTimer -= Time.deltaTime;
									if (__instance._stoneGestureTimer <= 0f)
									{
										__instance._solanumAnimController.PlayGestureToWordStones();
										__instance._stoneGestureTimer = UnityEngine.Random.Range(8f, 16f);
									}
								}
								// raise cairns
								else if (__instance._solanumAnimController.IsPlayerLooking())
								{
									DebugLog.DebugWrite($"{__instance._state} => RAISING CAIRNS");
									__instance._state = NomaiConversationManager.State.RaisingCairns;
									__instance._solanumAnimController.PlayRaiseCairns();
									__instance._cairnAnimator.SetTrigger("Raise");
									__instance._cairnCollision.SetActivation(true);
								}
							}
							else if (__instance._activeResponseText == null && __instance._hasValidSocketedStonePair)
							{
								DebugLog.DebugWrite($"{__instance._state} => WRITING RESPONSE");
								__instance._activeResponseText = __instance._pendingResponseText;
								__instance._pendingResponseText = null;
								__instance._state = NomaiConversationManager.State.WritingResponse;
								__instance._solanumAnimController.StartWritingMessage();
							}
							else if (__instance._activeResponseText != null && (!__instance._hasValidSocketedStonePair || __instance._pendingResponseText != null))
							{
								DebugLog.DebugWrite($"{__instance._state} => ERASING RESPONSE");
								__instance._state = NomaiConversationManager.State.ErasingResponse;
								__instance._solanumAnimController.StartWritingMessage();
							}
							else if (!holdingConversationStone)
							{
								if (__instance._playerWasHoldingStone)
								{
									__instance.ResetStoneGestureTimer();
								}
								__instance._stoneGestureTimer -= Time.deltaTime;
								if (__instance._stoneGestureTimer < 0f)
								{
									__instance._solanumAnimController.PlayGestureToWordStones();
									__instance.ResetStoneGestureTimer();
								}
							}
							else
							{
								if (!__instance._playerWasHoldingStone)
								{
									__instance.ResetCairnGestureTimer();
								}
								__instance._cairnGestureTimer -= Time.deltaTime;
								if (__instance._cairnGestureTimer < 0f)
								{
									__instance._solanumAnimController.PlayGestureToCairns();
									__instance.ResetCairnGestureTimer();
								}
							}
						}
					}
					break;

				case NomaiConversationManager.State.CreatingStones:
					if (!__instance._solanumAnimController.isPerformingAction)
					{
						DebugLog.DebugWrite($"{__instance._state} => WATCHING PLAYER");
						__instance._state = NomaiConversationManager.State.WatchingPlayer;
						__instance._conversationStonesCreated = true;
					}
					break;

				case NomaiConversationManager.State.RaisingCairns:
					if (!__instance._solanumAnimController.isPerformingAction)
					{
						DebugLog.DebugWrite($"{__instance._state} => WATCHING PLAYER");
						__instance._state = NomaiConversationManager.State.WatchingPlayer;
						__instance._cairnRaised = true;
						__instance._stoneSocketATrigger.SetActivation(true);
						__instance._stoneSocketBTrigger.SetActivation(true);
					}
					break;

				case NomaiConversationManager.State.ErasingResponse:
					if (!__instance._solanumAnimController.isStartingWrite && !__instance._activeResponseText.IsAnimationPlaying())
					{
						__instance._activeResponseText = null;
						if (__instance._pendingResponseText == null)
						{
							DebugLog.DebugWrite($"{__instance._state} => WATCHING PLAYER");
							__instance._state = NomaiConversationManager.State.WatchingPlayer;
							__instance._solanumAnimController.StopWritingMessage(false);
						}
						else
						{
							DebugLog.DebugWrite($"{__instance._state} => WATCHING WRITING RESPONSE");
							__instance._activeResponseText = __instance._pendingResponseText;
							__instance._pendingResponseText = null;
							__instance._state = NomaiConversationManager.State.WritingResponse;
							__instance._activeResponseText.Show();
						}
					}
					break;

				case NomaiConversationManager.State.WritingResponse:
					if (!__instance._solanumAnimController.isStartingWrite && !__instance._activeResponseText.IsAnimationPlaying())
					{
						DebugLog.DebugWrite($"{__instance._state} => WATCHING PLAYER");
						__instance._state = NomaiConversationManager.State.WatchingPlayer;
						__instance._solanumAnimController.StopWritingMessage(true);
					}
					break;
			}

			__instance._playerWasHoldingStone = holdingConversationStone;

			return false;
		}
	}
}
