using UnityEditor;
using UnityEngine;

namespace InnerDriveStudios.DiceCreator {

    /**
     * Helper class to show a small DieCreator / IDS Logo.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    public static class DiceCreatorLogoRenderer  {
        private static Texture _logoTexture = null;
        private static GUIStyle _logoTextureStyle = null;

        /**
         * Cache a static logo and logo style.
         */
        private static void ensureLogoTexture()
        {
            if (_logoTexture == null)
            {
                _logoTexture = Resources.Load<Texture>(PathConstants.IDS_LOGO);

                _logoTextureStyle = new GUIStyle();
                _logoTextureStyle.normal.background = _logoTexture as Texture2D;
                _logoTextureStyle.margin = new RectOffset(4, 4, 4, 4);
                _logoTextureStyle.fixedHeight = 0;
                _logoTextureStyle.fixedWidth = 0;
            }
        }


        public static void ShowLogo()
        {
            ensureLogoTexture();
            if (_logoTexture != null) showLogo();
        }

        private static void showLogo()
        {
            //use this a base width
            float maxWidth = EditorGUIUtility.currentViewWidth - 40;
            //small trick/hack to try and get the actual width of the editor
            Rect r = EditorGUILayout.GetControlRect(true, 0);
            //but only use the value if it makes sense
            if (r.width > 1) maxWidth = r.width;
            //calculate height based on image ratio
            float maxHeight = maxWidth * (_logoTexture.height / (float)_logoTexture.width);
            _logoTextureStyle.fixedWidth = maxWidth;
            _logoTextureStyle.fixedHeight = maxHeight;
            //render the logo
            GUILayout.Box("", _logoTextureStyle);
        }



    }
}
	

