using System;
using UnityEditor;
using UnityEngine;

public class ImageCaptureTool : EditorWindow
{
    #region Filed

    public string outputDirectory = null;

    public string outputFileName = ImageCaptureToolCore.DefaultOutputFileName;

    public int outputFileNameIndex = 0;

    public Camera camera = null;

    public int imageWidth  = 0;

    public int imageHeight = 0;

    public int imageScale  = 1;

    public bool clearBack = false;

    private Vector2 scrollPosition = Vector2.zero;

    #endregion Field

    #region Method

    [MenuItem("Custom/ImageCaptureTool")]
    static void Init()
    {
        EditorWindow.GetWindow<ImageCaptureTool>("ImageCaptureTool");
    }

    protected void OnEnable()
    {
        EditorApplication.update += ForceOnGUI;
    }

    protected void OnDisable()
    {
        EditorApplication.update -= ForceOnGUI;
    }

    protected void OnGUI()
    {
        GUIStyle marginStyle = GUI.skin.label;
                 marginStyle.wordWrap = true;
                 marginStyle.margin = new RectOffset(5, 5, 5, 5);

        this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition, GUI.skin.box);
        int[] gameViewResolution = GetGameViewResolution();

        if (GUILayout.Button("Click to Capture"))
        {
            HookAfterImageCaptured(Capture());
        }

        // Output directory.

        EditorGUILayout.LabelField("Output Directory", marginStyle);

        EditorGUILayout.BeginHorizontal(GUI.skin.label);
        {
            if (GUILayout.Button("Open"))
            {
                string tempPath = EditorUtility.SaveFolderPanel("Open", this.outputDirectory, "");

                if (!tempPath.Equals(""))
                {
                    this.outputDirectory = EditorGUILayout.TextField(tempPath);
                    base.Repaint();
                }
            }
            else
            {
                this.outputDirectory = EditorGUILayout.TextField(this.outputDirectory);
            }
        }
        EditorGUILayout.EndHorizontal();

        if (this.outputDirectory == null || this.outputDirectory.Equals(""))
        {
            this.outputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            // NOTE:
            // Application.dataPath + "/"; is not so bad.
        }

        // Base setttings.

        EditorGUILayout.LabelField("Base File Name.", marginStyle);
        this.outputFileName = EditorGUILayout.TextField(this.outputFileName);

        EditorGUILayout.LabelField("File Name Index", marginStyle);
        this.outputFileNameIndex = EditorGUILayout.IntField(this.outputFileNameIndex);

        EditorGUILayout.LabelField("Target camera. When 'null', use 'MainCamera' automatically.", marginStyle);
        this.camera = EditorGUILayout.ObjectField("Camera", this.camera, typeof(Camera), true) as Camera;

        EditorGUILayout.LabelField("Image width(px). When '0', use GameView width '" + gameViewResolution[0] + "'", marginStyle);
        this.imageWidth = EditorGUILayout.IntSlider(this.imageWidth, 0, 9999);

        EditorGUILayout.LabelField("Image height(px). When '0', use GameView height '" + gameViewResolution[1] + "'", marginStyle);
        this.imageHeight = EditorGUILayout.IntSlider(this.imageHeight, 0, 9999);

        EditorGUILayout.LabelField("Image scale. Ex: When set '2', the result will twice size of width and height.", marginStyle);
        this.imageScale = EditorGUILayout.IntSlider(this.imageScale, 1, 10);

        EditorGUILayout.LabelField("Clear the background when capture.", marginStyle);
        this.clearBack = EditorGUILayout.Toggle(this.clearBack);

        EditorGUILayout.EndScrollView();
    }

    protected void ForceOnGUI()
    {
        // NOTE:
        // Need periodic repaint to update Game View.Resolution info.

        if (System.DateTime.Now.Millisecond % 5 == 0)
        {
            Repaint();
        }
    }

    protected int[] GetGameViewResolution()
    {
        // NOTE:
        // Screen.width (& height) shows active window's resorution.
        // So in sometimes, it shows EditorWindow's resolution.

        string[] gameViewResolution = UnityStats.screenRes.Split('x');

        return new int[]
        {
            int.Parse(gameViewResolution[0]),
            int.Parse(gameViewResolution[1])
        };
    }

    protected ImageCaptureToolCore.CaptureResult Capture()
    {
        Camera camera = this.camera ?? Camera.main;

        int[] gameViewResolution = GetGameViewResolution();
        int imageWidth  = (this.imageWidth  == 0 ? gameViewResolution[0] : this.imageWidth)  * this.imageScale;
        int imageHeight = (this.imageHeight == 0 ? gameViewResolution[1] : this.imageHeight) * this.imageScale;

        ImageCaptureToolCore.CaptureResult result
        = ImageCaptureToolCore.Capture(camera,
                                       imageWidth,
                                       imageHeight,
                                       this.clearBack,
                                       this.outputDirectory,
                                       this.outputFileName + this.outputFileNameIndex.ToString());

        if (result.success)
        {
            this.ShowNotification(new GUIContent("SUCCESS : " + result.outputPath));
            this.outputFileNameIndex++;
        }
        else 
        {
            this.ShowNotification(new GUIContent("ERROR : " + result.outputPath));
        }

        return result;
    }

    protected virtual void HookAfterImageCaptured(ImageCaptureToolCore.CaptureResult result)
    {
        // Nothing to do in here. This is used for inheritance.
    }

    #endregion Method
}