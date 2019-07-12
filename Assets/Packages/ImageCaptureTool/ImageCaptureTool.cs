using System.IO;
using UnityEngine;

public static class ImageCaptureTool
{
    #region Field

    public static readonly string DefaultOutputFileName = "image";

    public class CaptureResult
    {
        public readonly bool   success;
        public readonly Camera camera;
        public readonly int    width;
        public readonly int    height;
        public readonly bool   clearBack;
        public readonly string outputDirectory;
        public readonly string outputFile;
        public readonly string outputExtension;
        public readonly string outputPath;

        public CaptureResult(bool   success,
                             Camera camera,
                             int    width,
                             int    height,
                             bool   clearBack,
                             string outputDirectory,
                             string outputFile,
                             string outputExtension,
                             string outputPath)
        {
            this.success         = success;
            this.camera          = camera;
            this.width           = width;
            this.height          = height;
            this.clearBack       = clearBack;
            this.outputDirectory = outputDirectory;
            this.outputFile      = outputFile;
            this.outputExtension = outputExtension;
            this.outputPath      = outputPath;
        }
    }

    #endregion Field

    #region Method

    public static CaptureResult Capture(Camera camera, int width, int height, bool clearBack, string directory, string file)
    {
        string outputDirectory;
        string outputFile;
        string outputExtension;
        string outputPath;

        GetOutputFilePath(directory,
                          file,
                          "png",
                          out outputDirectory,
                          out outputFile,
                          out outputExtension,
                          out outputPath);
        try
        {
            Texture2D captureImage = Capture(camera, width, height, clearBack);

            File.WriteAllBytes(outputPath, captureImage.EncodeToPNG());

            GameObject.DestroyImmediate(captureImage);

            return new CaptureResult(true,
                                     camera,
                                     width,
                                     height,
                                     clearBack,
                                     outputDirectory,
                                     outputFile,
                                     outputExtension,
                                     outputPath);
        }
        catch 
        {
            return new CaptureResult(false,
                                     camera,
                                     width,
                                     height,
                                     clearBack,
                                     outputDirectory,
                                     outputFile,
                                     outputExtension,
                                     outputPath);
        }
    }

    public static Texture2D Capture(Camera camera, int width, int height, bool clearBack)
    {
        // NOTE:
        // Keep presets.

        Color            tempBackgroundColor = camera.backgroundColor;
        CameraClearFlags tempClearFlags      = camera.clearFlags;
        RenderTexture    tempTargetTexture   = camera.targetTexture;

        if (clearBack)
        {
            camera.backgroundColor = Color.clear;
            camera.clearFlags = CameraClearFlags.SolidColor;
        }

        RenderTexture captureImageTemp;
        Texture2D captureImage;

        captureImageTemp = RenderTexture.GetTemporary(width, height, 32, RenderTextureFormat.ARGB32);
        captureImage = new Texture2D(width, height, TextureFormat.ARGB32, false);

        camera.targetTexture = captureImageTemp;
        camera.Render();

        RenderTexture.active = captureImageTemp;
        captureImage.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(captureImageTemp);

        // NOTE:
        // Reset settings.

        camera.backgroundColor = tempBackgroundColor;
        camera.clearFlags      = tempClearFlags;
        camera.targetTexture   = tempTargetTexture;

        return captureImage;
    }

    private static void GetOutputFilePath(string directory,
                                          string file,
                                          string extension,
                                      out string outputDirectory,
                                      out string outputFile,
                                      out string outputExtension,
                                      out string outputPath)
    {
        outputDirectory = directory;
        outputFile      = file;
        outputExtension = extension;

        if (string.IsNullOrEmpty(outputDirectory))
        {
            outputDirectory = Application.dataPath + "/";
        }
        else
        {
            outputDirectory = outputDirectory + "/";
        }

        if (string.IsNullOrEmpty(outputFile))
        {
            outputFile = ImageCaptureTool.DefaultOutputFileName;
        }

        outputPath = outputDirectory + outputFile + "." + outputExtension;
    }

    #endregion Method
}