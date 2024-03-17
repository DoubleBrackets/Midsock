using UnityEngine;

public class SessionServiceFinder : MonoBehaviour
{
    public static SessionStateManager SessionStateManager { get; private set; }

    public static PlayerDataNetworkService PlayerDataNetworkService { get; private set; }

    public static PlayerCharacterService PlayerCharacterService { get; private set; }

    public static void SetSessionStateManager(SessionStateManager sessionStateManager)
    {
        SessionStateManager = sessionStateManager;
    }

    public static void SetPlayerDataNetworkService(PlayerDataNetworkService playerDataNetworkService)
    {
        PlayerDataNetworkService = playerDataNetworkService;
    }

    public static void SetPlayerCharacterService(PlayerCharacterService playerCharacterService)
    {
        PlayerCharacterService = playerCharacterService;
    }
}