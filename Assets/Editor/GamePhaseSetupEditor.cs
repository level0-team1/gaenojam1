using UnityEditor;
using UnityEngine;

public static class GamePhaseSetupEditor
{
    [MenuItem("Tools/Setup GamePhase Camera References")]
    public static void SetupCameraReferences()
    {
        GamePhaseManager gpm = Object.FindAnyObjectByType<GamePhaseManager>(FindObjectsInactive.Include);
        if (gpm == null) { Debug.LogError("GamePhaseManager를 찾을 수 없습니다."); return; }

        // Find cameras with CameraFollow component
        var allCams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Camera cam1 = null, cam2 = null;
        foreach (var cam in allCams)
        {
            var cf = cam.GetComponent<CameraFollow>();
            if (cf == null) continue;
            if (cam.rect.x < 0.1f) cam1 = cam;
            else cam2 = cam;
        }

        if (cam1 != null) gpm.player1Cam = cam1.GetComponent<CameraFollow>();
        if (cam2 != null) gpm.player2Cam = cam2.GetComponent<CameraFollow>();

        // Find StartCameraAnchor
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.scene.isLoaded && go.name == "StartCameraAnchor")
            {
                gpm.startCameraAnchor = go.transform;
                break;
            }
        }

        EditorUtility.SetDirty(gpm);
        Debug.Log($"[GamePhaseSetup] 완료! p1Cam={gpm.player1Cam}, p2Cam={gpm.player2Cam}, anchor={gpm.startCameraAnchor}");
    }
}
