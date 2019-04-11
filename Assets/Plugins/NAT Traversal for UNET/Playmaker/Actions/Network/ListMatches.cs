#if PLAYMAKER

using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

namespace HutongGames.PlayMaker.Actions
{

	[ActionCategory(ActionCategory.Network)]
	public class ListMatches : FsmStateAction
	{
        [ArrayEditor(typeof(MatchInfoObject))]
        public FsmArray results;

        // Code that runs on entering the state.
        public override void OnEnter()
		{
            var networkManager = NetworkManager.singleton;
            if (networkManager.matchMaker == null) networkManager.matchMaker = networkManager.gameObject.AddComponent<NetworkMatch>();
#if !UNITY_5_3
            networkManager.matchMaker.ListMatches(0, 10, "", true, 0, 0, OnMatchList);
#else
            networkManager.matchMaker.ListMatches(0, 10, "", OnMatchList);
#endif
            Finish();
        }

#if !UNITY_5_3
        public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
        {
#else
        public void OnMatchList(ListMatchResponse response)
        {
            var matchList = response.matches;
#endif
            results.Resize(matchList.Count);
            for (int i = 0; i < matchList.Count; i++)
            {
                results.Set(i, new MatchInfoObject(matchList[i]));
            }
        }
        
	}

}
#else
class ListMatches { }
#endif