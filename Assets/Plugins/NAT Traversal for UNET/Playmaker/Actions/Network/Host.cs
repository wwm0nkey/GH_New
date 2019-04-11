#if PLAYMAKER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Network)]
	public class Host : FsmStateAction
	{
        public FsmObject hostInfo;
        public string matchName = "default";
        public int maxPlayers = 4;
        public bool advertise = true;
        public string password = "";
        public int eloScore = 0;
        public int requestDomain = 0;
        public int directConnectPort = 0;

		// Code that runs on entering the state.
		public override void OnEnter()
		{
            var networkManager = (NATTraversal.NetworkManager)NetworkManager.singleton;
            networkManager.StartHostAll(matchName, (uint)maxPlayers, advertise, password, eloScore, requestDomain, directConnectPort);
            networkManager.StartCoroutine(WaitForMatch());
		}

        IEnumerator WaitForMatch()
        {
            var networkManager = (NATTraversal.NetworkManager)NetworkManager.singleton;
            Debug.Log("started waiting");
            while (networkManager.matchInfo == null) yield return 0;
            Debug.Log("done waiting");
            hostInfo.Value = new MatchInfoObject() {
                networkId = networkManager.matchInfo.networkId,
                hostNodeId = networkManager.matchInfo.nodeId,
                name = matchName,
                averageEloScore = eloScore,
                maxSize = maxPlayers,
                currentSize = 1,
                isPrivate = !advertise
            };
            Finish();
        }
        
	}

}
#else
class Host { }
#endif