using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Note))]
public class NoteEditor : Editor {
    static private Color background = new Color(1, 1, 0.4f);
    static private GUIStyle NOTE_STYLE = null;
    static private Texture2D NOTE_TEXTURE = null;

    static private GUILayoutOption[] options = {
		GUILayout.ExpandWidth (true), 
		//GUILayout.ExpandHeight (true),
		GUILayout.MinHeight(0)
    };

    static void createTextures() {
        if (NOTE_TEXTURE == null) {
            NOTE_TEXTURE = new Texture2D(1,1);
            NOTE_TEXTURE.SetPixel(0,0, background);
            NOTE_TEXTURE.Apply();

            NOTE_STYLE = new GUIStyle();
            NOTE_STYLE.normal.background = NOTE_TEXTURE;
            NOTE_STYLE.wordWrap = true;
            NOTE_STYLE.margin = new RectOffset(2, 2, 2, 2);
        }
    }

    private Note _note;

    private void OnEnable ()
	{
        _note = target as Note;
        createTextures();
	}

    public override void OnInspectorGUI()
    {
        _note.note = EditorGUILayout.TextArea(_note.note, NOTE_STYLE, options);
    }
}