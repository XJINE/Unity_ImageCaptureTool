using System.IO;
using UnityEngine;

public static class ImageCaptureToolCore
{
    #region Field

    public static string DefaultOutputFileName = "image";

    #endregion Field

    #region Method

    public static bool Capture(Camera camera, int width, int height, bool clearBack, string directory, string file, int suffixNum = -1) 
    {
        try
        {
            string path = ImageCaptureToolCore.GetOutputPath(directory, file, suffixNum) + ".png";

            Texture2D captureImage = Capture(camera, width, height, clearBack);

            File.WriteAllBytes(path, captureImage.EncodeToPNG());

            GameObject.DestroyImmediate(captureImage);

            return true;
        }
        catch 
        {
            return false;
        }
    }

    public static Texture2D Capture(Camera camera, int width, int height, bool clearBack)
    {
        Camera captureCamera = camera == null ? Camera.main : camera;

        // Keep presets.

        Color presetBackgroundColor = captureCamera.backgroundColor;
        CameraClearFlags presetClearFlags = captureCamera.clearFlags;
        RenderTexture presetRenderTexture = captureCamera.targetTexture;

        if (clearBack)
        {
            captureCamera.backgroundColor = Color.clear;
            captureCamera.clearFlags = CameraClearFlags.SolidColor;
        }

        RenderTexture captureImageTemp = RenderTexture.GetTemporary(width, height, 32, RenderTextureFormat.ARGB32);
        Texture2D captureImage = new Texture2D(width, height, TextureFormat.ARGB32, false);

        captureCamera.targetTexture = captureImageTemp;
        captureCamera.Render();

        RenderTexture.active = captureImageTemp;
        captureImage.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(captureImageTemp);

        // Reset settings.

        captureCamera.backgroundColor = presetBackgroundColor;
        captureCamera.clearFlags = presetClearFlags;
        captureCamera.targetTexture = presetRenderTexture;

        return captureImage;
    }

    public static string GetOutputPath(string directory, string file, int suffixNum = -1)
    {
        string outputDirectory = directory;
        string outputFile = file;

        if (outputDirectory == null || outputDirectory.Equals(""))
        {
            outputDirectory = Application.dataPath + "/";
        }
        else
        {
            outputDirectory = outputDirectory + "/";
        }

        if (outputFile == null || outputFile.Equals(""))
        {
            outputFile = ImageCaptureToolCore.DefaultOutputFileName;
        }

        return outputDirectory + outputFile + (-1 < suffixNum ? suffixNum.ToString() : "");
    }

    #endregion Method
}