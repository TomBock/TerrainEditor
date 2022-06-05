using UnityEngine;

namespace DFD.TerrainEditor
{
    /// <summary>
    /// Code copied from https://assetstore.unity.com/packages/tools/terrain/runtime-terrain-editor-222184
    /// </summary>
    public static class Texture2DExtensions
    {
        
        public static Texture2D Rescale(this Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            var textureRect = new Rect(0, 0, width, height);
            ScaleTexture(src, width, height, mode);
            
            //  Get rendered data back to a new texture
            var result = new Texture2D(width, height, TextureFormat.ARGB32, true);
            result.Reinitialize(width, height);
            result.ReadPixels(textureRect, 0, 0, true);
            return result;
        }
        
        private static void ScaleTexture(Texture2D src, int width, int height, FilterMode fmode)
        {
            //  We need the source texture in VRAM because we render with it
            src.filterMode = fmode;
            src.Apply(true);
            //  Using RTT for best quality and performance.
            RenderTexture rtt = new RenderTexture(width, height, 32);
            //  Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);
            //  Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);
            //  Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
        }
    }

}
