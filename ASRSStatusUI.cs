using UnityEngine;

public class ASRSStatusUI : MonoBehaviour
{
    public ASRS asrs;
    public RackManager rackManager;

    private GUIStyle boxStyle;
    private GUIStyle labelStyle;
    private GUIStyle headerStyle;

    void OnGUI()
    {
        if (asrs == null) return;

        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.6f));

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 14;

            headerStyle = new GUIStyle(labelStyle);
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.fontSize = 15;
        }

        int totalSlots = 0, freeSlots = 0;
        if (rackManager != null)
        {
            totalSlots = rackManager.slots.Count;
            foreach (var s in rackManager.slots)
                if (!s.isOccupied) freeSlots++;
        }

        GUI.Box(new Rect(10, 10, 230, 175), "", boxStyle);
        GUILayout.BeginArea(new Rect(20, 18, 210, 160));

        GUILayout.Label("ASRS SYSTEM", headerStyle);
        GUILayout.Space(4);

        string modeColor = asrs.mode == ASRS.ASRSMode.Auto ? "#00FF88" : "#FFD700";
        GUILayout.Label($"Mode: <color={modeColor}><b>{asrs.mode}</b></color>", RichLabel());

        GUILayout.Space(6);
        GUILayout.Label("── CONTROLS ──────────────", labelStyle);
        GUILayout.Label("Arrow / WASD   Move X & Y", labelStyle);
        GUILayout.Label("Q / E          Extend / Retract Z", labelStyle);
        GUILayout.Label("Space          Grab / Release (Manual)", labelStyle);
        GUILayout.Label("Tab            Toggle Auto / Manual", labelStyle);

        if (rackManager != null)
        {
            GUILayout.Space(4);
            GUILayout.Label($"Rack slots free: {freeSlots} / {totalSlots}", labelStyle);
        }

        GUILayout.EndArea();
    }

    GUIStyle RichLabel()
    {
        var s = new GUIStyle(GUI.skin.label);
        s.normal.textColor = Color.white;
        s.fontSize = 14;
        s.richText = true;
        return s;
    }

    Texture2D MakeTex(int w, int h, Color col)
    {
        Color[] pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        var t = new Texture2D(w, h);
        t.SetPixels(pix);
        t.Apply();
        return t;
    }
}