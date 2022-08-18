using UnityEngine;

namespace ChallengeMod
{
    public class GuiMonoBehaviour : MonoBehaviour
    {
        public static GUIContent gc = new GUIContent("Select an option");
        public void OnGUI()
        {
            gc.text = DataStore.ChallengeType.ToString();

            Rect menuRect;
            menuRect = new(100f, 100f, 350f, (50 * Enum.GetValues(typeof(ChallengeType)).Length) + 135);
            int valX = 0;
            int valY = 0;

            GUIStyle gstyle = new();
            gstyle.alignment = UnityEngine.TextAnchor.MiddleCenter;
            gstyle.fontSize = 16;
            gstyle.fontStyle = UnityEngine.FontStyle.Normal;
            gstyle.normal.background = Texture2D.grayTexture;

            GUIStyle bstyle = new();
            bstyle.alignment = UnityEngine.TextAnchor.MiddleCenter;
            bstyle.normal.textColor = Color.black;
            bstyle.normal.background = Utils.getTextureOfColor(Color.cyan);

            if (Main.menuEnabled)
            {
                GUI.BeginGroup(menuRect, gstyle);
                foreach (ChallengeType c in Enum.GetValues(typeof(ChallengeType)))
                {
                    if (valY + 77 >= Screen.height / 2)
                    {
                        valY = 0;
                        valX += 173;
                    }
                    if (GUI.Button(new Rect(25f + (float)valX, (float)(25 + valY), 250f, 40f), new GUIContent(c.ToString()), bstyle))
                    {
                        Main.DoDataStoreChange(c);
                    }
                    valY += 47;
                }
                GUI.Box(new Rect(25f + (float)valX, (float)(25 + valY), 250f, 50f), gc);
                GUI.EndGroup();
            }
        }
    }
}
