using Fusion;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerLobbyData : NetworkBehaviour
{
    [Networked] public int CharacterIndex { get; set; }
    [Networked] public int TeamIndex { get; set; }
    [Networked] public NetworkBool IsReady { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            CharacterIndex = 0;
            TeamIndex = 0;
            IsReady = false;

        }
        Debug.Log($"LobbyData Spawned / ImputAuthoriy : {Object.InputAuthority}");

    }

    [Rpc(RpcSources.InputAuthority , RpcTargets.StateAuthority)]
    
    public void RPC_SelectCharacter(int index)
    {
        CharacterIndex = Mathf.Max(0, index);
        IsReady = false;

        Debug.Log($"{Object.InputAuthority} 캐릭터 선택 : {CharacterIndex}");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]

    public void RPC_SelectTeam(int teamlndex)
    {
        teamlndex = Mathf.Clamp(teamlndex, 0, 1);

        FusionBootstrap bootstrap = FindFirstObjectByType<FusionBootstrap>();

        //if(bootstrap = null && !bootstrap.CanJoinTeam(teamIndex))
        //{
        //              return;
        //}

        TeamIndex = teamlndex;
        IsReady = false;

        Debug.Log($"{Object.InputAuthority}EH : {TeamIndex}");
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetReady(NetworkBool ready)
    {
        IsReady = ready;

        Debug.Log($"{Object.InputAuthority} Ready : {IsReady}");
    }
}
