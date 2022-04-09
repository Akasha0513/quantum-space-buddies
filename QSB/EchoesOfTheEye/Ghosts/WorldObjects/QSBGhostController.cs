﻿using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.WorldObjects;

public class QSBGhostController : WorldObject<GhostController>, IGhostObject
{
	public override void SendInitialState(uint to)
	{

	}

	public QSBGhostEffects _effects;

	public void Initialize(GhostNode.NodeLayer layer, QSBGhostEffects effects)
	{
		_effects = effects;
		AttachedObject._nodeRoot = AttachedObject.transform.parent = AttachedObject._nodeMap.transform;
		AttachedObject._nodeLayer = layer;
		AttachedObject._grabController.Initialize(effects.AttachedObject);
		AttachedObject._lantern.SetLit(true);
		AttachedObject.MoveLanternToCarrySocket(false, 0.1f);
		AttachedObject._playerCollider = Locator.GetPlayerBody().GetComponent<CapsuleCollider>();
	}

	public void SetLanternConcealed(bool concealed, bool playAudio = true)
	{
		if (playAudio && AttachedObject._lantern.IsConcealed() != concealed)
		{
			_effects.PlayLanternAudio(concealed ? global::AudioType.Artifact_Conceal : global::AudioType.Artifact_Unconceal);
		}

		AttachedObject._lantern.SetConcealed(concealed);
		if (concealed)
		{
			AttachedObject._lantern.SetFocus(0f);
			AttachedObject._updateLantern = false;
		}
	}

	public void ChangeLanternFocus(float focus, float focusRate = 2f)
	{
		if (focus > 0f)
		{
			AttachedObject._lantern.SetConcealed(false);
		}

		if (focus > AttachedObject._targetLanternFocus)
		{
			_effects.PlayLanternAudio(global::AudioType.Artifact_Focus);
		}
		else if (focus < AttachedObject._targetLanternFocus)
		{
			_effects.PlayLanternAudio(global::AudioType.Artifact_Unfocus);
		}

		AttachedObject._updateLantern = true;
		AttachedObject._targetLanternFocus = focus;
		AttachedObject._lanternFocusRate = focusRate;
	}

	public void FacePlayer(PlayerInfo player, TurnSpeed turnSpeed)
	{
		AttachedObject._facingState = GhostController.FacingState.FaceTransform;
		AttachedObject._faceTransform = player.Camera.transform;
		AttachedObject._targetDegreesPerSecond = GhostConstants.GetTurnSpeed(turnSpeed);
		AttachedObject._angularAcceleration = GhostConstants.GetTurnAcceleration(turnSpeed);
	}

	public void FaceLocalPosition(Vector3 localPosition, TurnSpeed turnSpeed)
	{
		FaceLocalPosition(localPosition, GhostConstants.GetTurnSpeed(turnSpeed), GhostConstants.GetTurnAcceleration(turnSpeed));
	}

	public void FaceLocalPosition(Vector3 localPosition, float degreesPerSecond, float turnAcceleration = 360f)
	{
		if (!QSBCore.IsHost)
		{
			return;
		}

		AttachedObject.FaceLocalPosition(localPosition, degreesPerSecond, turnAcceleration);
	}

	public void FaceNodeList(GhostNode[] nodeList, int numNodes, TurnSpeed turnSpeed, float nodeDelay, bool autoFocusLantern = false)
	{
		if (!QSBCore.IsHost)
		{
			return;
		}

		AttachedObject.FaceNodeList(nodeList, numNodes, turnSpeed, nodeDelay, autoFocusLantern);
	}

	public void FaceVelocity()
	{
		if (!QSBCore.IsHost)
		{
			return;
		}

		AttachedObject.FaceVelocity();
	}

	public void MoveToLocalPosition(Vector3 localPosition, MoveType moveType)
	{
		this.MoveToLocalPosition(localPosition, GhostConstants.GetMoveSpeed(moveType), GhostConstants.GetMoveAcceleration(moveType));
	}

	public void MoveToLocalPosition(Vector3 localPosition, float speed, float acceleration = 10f)
	{
		if (!QSBCore.IsHost)
		{
			return;
		}

		AttachedObject.MoveToLocalPosition(localPosition, speed, acceleration);
	}

	public void PathfindToLocalPosition(Vector3 localPosition, MoveType moveType)
	{
		this.PathfindToLocalPosition(localPosition, GhostConstants.GetMoveSpeed(moveType), GhostConstants.GetMoveAcceleration(moveType));
	}

	public void PathfindToLocalPosition(Vector3 localPosition, float speed, float acceleration = 10f)
	{
		if (!QSBCore.IsHost)
		{
			return;
		}

		var brain = AttachedObject.GetComponent<GhostBrain>();
		this.SendMessage(new PathfindLocalPositionMessage(brain._effects._ghostIKController._sector, AttachedObject.LocalToWorldPosition(localPosition), speed, acceleration));
		AttachedObject.PathfindToLocalPosition(localPosition, speed, acceleration);
	}

	public void PathfindToNode(GhostNode node, MoveType moveType)
	{
		this.PathfindToNode(node, GhostConstants.GetMoveSpeed(moveType), GhostConstants.GetMoveAcceleration(moveType));
	}

	public void PathfindToNode(GhostNode node, float speed, float acceleration = 10f)
	{
		if (!QSBCore.IsHost)
		{
			return;
		}

		AttachedObject.PathfindToNode(node, speed, acceleration);
	}

	public void FaceNode(GhostNode node, TurnSpeed turnSpeed, float nodeDelay, bool autoFocusLantern = false)
	{
		if (!QSBCore.IsHost)
		{
			return;
		}

		AttachedObject.FaceNode(node, turnSpeed, nodeDelay, autoFocusLantern);
	}
}
