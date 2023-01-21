using UnityEngine;
using UI.HUD;
using Utils;
using Stats;

namespace Control
{
    public interface IRaycastable
    {
        bool HandleRaycast(PlayerController callingController, bool actionAvailable);

        void Interact(PlayerController callingController);

        Vector3 GetPosition();

        CursorType GetCursorType(PlayerController callingController);

        int GetInteractCost(PlayerController callingController);

        float GetInteractRange(PlayerController callingController);

        void EnableStatusBar();

        void SetupStatusBar();

        void EnableHintDisplay();

        void EnableObstacle(bool isEnable);

        void EnableBattleMarker(bool isEnable, bool isSelfTarget = false);

        BaseStats GetBaseStats();

        Transform GetTransform();
    }
}