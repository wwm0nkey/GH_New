#if PLAYMAKER

using UnityEngine.Networking;

namespace HutongGames.PlayMaker.Actions
{

	[ActionCategory(ActionCategory.Network)]
	public class Disconnect : FsmStateAction
	{
        // Code that runs on entering the state.
        public override void OnEnter()
		{
            var networkManager = (NATTraversal.NetworkManager)NetworkManager.singleton;
            if (NetworkServer.active)
            {
                NetworkServer.SetAllClientsNotReady();
                networkManager.StopHost();
            }
            else
            {
                networkManager.StopClient();
            }
            Finish();
		}
        
	}

}

#else
class Disconnect { }
#endif