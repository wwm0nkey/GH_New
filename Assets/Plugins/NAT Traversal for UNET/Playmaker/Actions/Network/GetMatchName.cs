#if PLAYMAKER
using UnityEngine.Networking.Match;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.Network)]
	public class GetMatchName : FsmStateAction
	{
        public FsmObject match;
        public FsmString matchName;

        // Code that runs on entering the state.
        public override void OnEnter()
		{
            MatchInfoObject matchInfo = (MatchInfoObject)match.Value;
            matchName.Value = matchInfo.name.Split('|')[0];
            Finish();
        }
        
	}

}
#else
class GetMatchName { }
#endif