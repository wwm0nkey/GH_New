#if PLAYMAKER

using NATTraversal;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

namespace HutongGames.PlayMaker.Actions
{

	[ActionCategory(ActionCategory.Network)]
	public class Connect : FsmStateAction
	{
        public FsmObject hostConnectionInfo;

        // Code that runs on entering the state.
        public override void OnEnter()
		{
            var networkManager = (NATTraversal.NetworkManager)NetworkManager.singleton;
            MatchInfoObject info = (MatchInfoObject)hostConnectionInfo.Value;
            string internalIP, externalIP, internalIPv6, externalIPv6;
            int port;
            ulong guid;
            networkManager.ParseConnectionInfoFromMatchName(info.name, out externalIP, out internalIP, out externalIPv6, out internalIPv6, out port, out guid);

            networkManager.StartClientAll(
                externalIP,
                internalIP,
                port,
                guid,
                info.networkId,
                externalIPv6,
                internalIPv6
            );
			Finish();
		}
        
	}

}

#else
class Connect { }
#endif