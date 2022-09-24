using UnityEngine;

namespace ChallengeMod
{
    public class GuiMonoBehaviour : MonoBehaviour
    {
        public static GUIContent gc = new GUIContent("Select a challenge");
        public static GUIContent scribblzz_text = new GUIContent("Enable Scribblzz mode");
        public void OnGUI()
        {
            gc.text = CHHelper.ToString(DataStore.ChallengeType);

            Rect menuRect;
            menuRect = new(100f, 100f, 400f, (50 * Enum.GetValues(typeof(ChallengeType)).Length) + 200);
            int valX = 0;
            int valY = 0;

            GUIStyle gstyle = new();
            gstyle.alignment = TextAnchor.MiddleCenter;
            gstyle.fontSize = 16;
            gstyle.fontStyle = FontStyle.Normal;
            gstyle.normal.background = Texture2D.grayTexture;

            GUIStyle bstyle = new();
            bstyle.alignment = TextAnchor.MiddleCenter;
            bstyle.normal.textColor = Color.black;
            bstyle.normal.background = Utils.getTextureOfColor(Color.cyan);

            if (Main.menuEnabled)
            {
                GUI.BeginGroup(menuRect, gstyle);

                if (GUI.Button(new Rect(25 + (float)valX, (float)(25 + valY), 350f, 40f), scribblzz_text, bstyle))
                {
                    Debug.Log("SCR BTN PRESSED");
                    if (Main.scrMode)
                    {
                        Main.scrMode = false;
                        scribblzz_text.text = "Enable Scribblzz mode";
                    }
                    else
                    {
                        Main.scrMode = true;
                        scribblzz_text.text = "Disable Scribblzz mode";
                    }
                }

                valY += 50;

                foreach (ChallengeType c in Enum.GetValues(typeof(ChallengeType)))
                {
                    if (valY + 77 >= Screen.height / 2)
                    {
                        valY = 0;
                        valX += 173;
                    }
                    if (GUI.Button(new Rect(25f + (float)valX, (float)(25 + valY), 350f, 40f), new GUIContent(CHHelper.ToString(c)), bstyle))
                    {
                        Main.DoDataStoreChange(c);
                    }
                    valY += 47;
                }

                if (valY + 70 >= Screen.height / 2)
                {
                    valY = 0;
                    valX += 173;
                }

                GUI.Box(new Rect(25f + (float)valX, (float)(25 + valY), 350f, 50f), gc);

                GUI.EndGroup();
            }
        }
    }
}
