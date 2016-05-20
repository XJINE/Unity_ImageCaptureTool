using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// ------------------------------------------------------------------------------------------------
// 現状と仕様
// ------------------------------------------------------------------------------------------------
// 
// 現在のところ無効なディレクトリまたはファイル名であることを確認しません。
// 任意のディレクトリ、ファイル名を指定するときは、
// 実行者自身が安全な名前であることを保証する必要があります。
// 
// OnGUI で各種コントロールを描画しています。
// したがって、GameView の解像度が変更されても、このウィンドウが再描画されるまで、
// このコントロールに表示される GameView の解像度の変更が視認できません。
// 
// 再度アクティブになる、リサイズするなどするとウィンドウが再描画されます。
// また内部的にはデータが更新されています(GameView の解像度が正しく取得されています)。

#if UNITY_EDITOR

/// <summary>
/// Unity の画面から画像をキャプチャするための機能を提供します。
/// </summary>
public class ImageCaptureTool : EditorWindow
{
    #region Filed

    /// <summary>
    /// キャプチャ画像を保存するためのキー。
    /// </summary>
    public KeyCode imageCaptureKey = KeyCode.Return;

    /// <summary>
    /// キー入力によるキャプチャ画像の保存の可否。
    /// true のとき、キー入力によるキャプチャ画像の保存を有効にします。
    /// </summary>
    public bool enableImageCaptureKey = false;

    /// <summary>
    /// キャプチャ画像を出力するディレクトリのパス。
    /// 空白文字列などの無効な文字列のとき、実行ファイルと同じディレクトリに出力します。
    /// </summary>
    public string outputDirectory = null;

    /// <summary>
    /// キャプチャ画像の既定のファイル名。
    /// </summary>
    public static string BaseOutputFileName = "image";

    /// <summary>
    /// キャプチャ画像の基底のファイル名。
    /// 空白文字列などの無効な文字列のとき、基底のファイル名は image となります。
    /// </summary>
    public string outputFileName = ImageCaptureTool.BaseOutputFileName;

    /// <summary>
    /// キャプチャ画像のインデックス番号。
    /// </summary>
    public int currentFileNameIndex = 0;

    /// <summary>
    /// キャプチャ画像を出力するカメラ。
    /// 指定しないとき MainCamera の画像を出力します。
    /// </summary>
    public Camera camera = null;

    /// <summary>
    /// キャプチャ画像の水平方向の解像度。
    /// 0 のとき GameView の水平方向の解像度になります。
    /// </summary>
    public int imageWidth = 0;

    /// <summary>
    /// キャプチャ画像の垂直方向の解像度。
    /// 0 のとき GameView の垂直方向の解像度になります。
    /// </summary>
    public int imageHeight = 0;

    /// <summary>
    /// キャプチャ画像の解像度の倍率。
    /// 2 を設定するとき指定された解像度の 2 倍の解像度でキャプチャ画像を生成します。
    /// </summary>
    public int imageScale = 1;

    /// <summary>
    /// 背景の透過を有効にする判定。初期値は無効です。
    /// </summary>
    public bool enableBackgroundAlpha = false;

    /// <summary>
    /// ScrollView の現在の位置。
    /// </summary>
    private Vector2 scrollPosition = Vector2.zero;

    #endregion Field

    #region Method

    /// <summary>
    /// Window を初期化(表示)するときに実行されます。
    /// </summary>
    [MenuItem("Custom/ImageCaptureTool")]
    static void Init()
    {
        EditorWindow.GetWindow<ImageCaptureTool>("ImageCapture");
    }

    /// <summary>
    /// Window が有効になったときに実行されます。
    /// </summary>
    void OnEnable()
    {
        // nothing to do.
    }

    /// <summary>
    /// GUI の出力時に実行されます。
    /// </summary>
    void OnGUI()
    {
        #region Style

        GUIStyle marginStyle = GUI.skin.label;
        marginStyle.wordWrap = true;
        marginStyle.margin = new RectOffset(5, 5, 5, 5);

        #endregion Style

        this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition, GUI.skin.box);

        EditorGUILayout.LabelField("現在の設定で画像をキャプチャします。",
                                    marginStyle);

        if (GUILayout.Button("Click to Save"))
        {
            CaptureImage();
        }

        // 指定したキー入力が実行されたとき、画像を保存します。

        if (Event.current != null
            && Event.current.type == EventType.keyDown
            && Event.current.keyCode == this.imageCaptureKey
            && this.enableImageCaptureKey)
        {
            CaptureImage();
        }

        EditorGUILayout.LabelField("有効なとき、指定したキー入力によって画像をキャプチャします。",
                                    marginStyle);

        EditorGUILayout.BeginHorizontal(GUI.skin.label);
        {
            this.enableImageCaptureKey = EditorGUILayout.Toggle(this.enableImageCaptureKey);
            this.imageCaptureKey = (KeyCode)EditorGUILayout.EnumPopup(this.imageCaptureKey);
        }
        EditorGUILayout.EndHorizontal();

        #region OutputDirectory

        EditorGUILayout.LabelField("キャプチャ画像の出力ディレクトリを設定します。",
                                    marginStyle);

        EditorGUILayout.BeginHorizontal(GUI.skin.label);
        {
            if (GUILayout.Button("Open"))
            {
                string tempPath = EditorUtility.SaveFolderPanel("Open", this.outputDirectory, "");

                // Cancel された場合を考慮します。

                if (!tempPath.Equals(""))
                {
                    this.outputDirectory = EditorGUILayout.TextField(tempPath);
                    this.Repaint();
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
            this.outputDirectory = Application.dataPath + "/";
        }

        #endregion Output Directory

        EditorGUILayout.LabelField
            ("キャプチャ画像の基底のファイル名を設定します。",
             marginStyle);

        this.outputFileName = EditorGUILayout.TextField(this.outputFileName);

        EditorGUILayout.LabelField
            ("キャプチャ画像のファイル名に与えられるインデックスです。",
             marginStyle);

        this.currentFileNameIndex = EditorGUILayout.IntField(this.currentFileNameIndex);

        EditorGUILayout.LabelField
            ("画像をキャプチャするカメラを指定します。指定しないとき MainCamera の画像をキャプチャします。",
             marginStyle);

        this.camera = EditorGUILayout.ObjectField("Camera", this.camera, typeof(Camera), true) as Camera;

        int[] gameViewResolution = GetGameViewResolution();

        EditorGUILayout.LabelField
            ("キャプチャ画像の水平方向の解像度。0 のとき、GameView の解像度(" + gameViewResolution[0] + ")になります。",
             marginStyle);

        this.imageWidth = EditorGUILayout.IntSlider(this.imageWidth, 0, 9999);

        EditorGUILayout.LabelField
            ("出力する画像の垂直方向の解像度。0 のとき、GameView の解像度(" + gameViewResolution[1] + ")になります。",
             marginStyle);

        this.imageHeight = EditorGUILayout.IntSlider(this.imageHeight, 0, 9999);

        EditorGUILayout.LabelField
            ("キャプチャ画像の解像度の倍率。2 を設定するとき、指定した解像度の 2 倍の解像度になります。",
             marginStyle);

        this.imageScale = EditorGUILayout.IntSlider(this.imageScale, 1, 10);

        EditorGUILayout.LabelField
            ("カメラの設定にかかわらず背景を透過するとき有効にします。",
             marginStyle);

        this.enableBackgroundAlpha = EditorGUILayout.Toggle(this.enableBackgroundAlpha);

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// GameView の大きさを取得します。
    /// </summary>
    /// </summary>
    /// <returns>
    /// GameView の横幅と高さの配列。
    /// </returns>
    private int[] GetGameViewResolution()
    {
        string[] gameViewResolution = UnityStats.screenRes.Split('x');
        return new int[]
        {
            int.Parse(gameViewResolution[0]),
            int.Parse(gameViewResolution[1])
        };
    }

    /// <summary>
    /// 画像の保存を実行します。
    /// </summary>
    private void CaptureImage()
    {
        // エディタ以外のときは実行しません。

        if (Application.platform != RuntimePlatform.OSXEditor
            && Application.platform != RuntimePlatform.WindowsEditor)
        {
            return;
        }

        string outputFileName = GetOutputFileName();

        try
        {
            Texture2D texture = GenerateCaptureImage();
            byte[] bytes = texture.EncodeToPNG();

            File.WriteAllBytes(outputFileName, bytes);

            this.currentFileNameIndex += 1;

            DestroyImmediate(texture);

            ShowCaptureResult(true, "Success : " + outputFileName);
        }
        catch
        {
            ShowCaptureResult(false, "Error : " + outputFileName);
        }
    }

    /// <summary>
    /// キャプチャしたイメージを Texture2D として取得します。
    /// </summary>
    /// <returns>
    /// キャプチャされたイメージが与えられた Texture2D 。
    /// </returns>
    private Texture2D GenerateCaptureImage()
    {
        Camera fixedCamera;

        if (this.camera == null)
        {
            fixedCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }
        else
        {
            fixedCamera = this.camera;
        }

        int fixedWidth = this.imageWidth;
        int fixedHiehgt = this.imageHeight;
        int bit = 32;

        int[] gameViewResolution = GetGameViewResolution();

        if (fixedWidth == 0)
        {
            fixedWidth = gameViewResolution[0];
        }

        if (fixedHiehgt == 0)
        {
            fixedHiehgt = gameViewResolution[1];
        }

        fixedWidth *= this.imageScale;
        fixedHiehgt *= this.imageScale;

        Color presetBackgroundColor = fixedCamera.backgroundColor;
        CameraClearFlags presetClearFlags = fixedCamera.clearFlags;

        if (this.enableBackgroundAlpha)
        {
            fixedCamera.backgroundColor = Color.clear;
            fixedCamera.clearFlags = CameraClearFlags.SolidColor;
        }

        RenderTexture presetRenderTexture = fixedCamera.targetTexture;

        // カメラに出力用の RenderTexture を設定してレンダリングを実行し、
        // その情報を Texture2D に保存して返す。

        RenderTexture outputRenderTexture = new RenderTexture(fixedWidth,
                                                              fixedHiehgt,
                                                              bit);
        fixedCamera.targetTexture = outputRenderTexture;

        Texture2D captureImage = new Texture2D(fixedWidth,
                                        fixedHiehgt,
                                        TextureFormat.ARGB32,
                                        false);

        fixedCamera.Render();

        RenderTexture.active = outputRenderTexture;

        captureImage.ReadPixels
            (new Rect(0, 0, fixedWidth, fixedHiehgt),
                      0,
                      0);

        // 設定を元に戻します。

        fixedCamera.backgroundColor = presetBackgroundColor;
        fixedCamera.clearFlags = presetClearFlags;
        fixedCamera.targetTexture = presetRenderTexture;

        // 解放してから終了します。

        RenderTexture.active = null;

        outputRenderTexture.Release();

        DestroyImmediate(outputRenderTexture);

        return captureImage;
    }

    /// <summary>
    /// 最終的に出力するファイル名を取得します。ディレクトリ名を含むフルパスを取得します。
    /// </summary>
    /// <returns>
    /// 出力するファイル名。
    /// </returns>
    private string GetOutputFileName()
    {
        string fixedDirectory = this.outputDirectory;
        string fixedFileName = this.outputFileName;

        if (fixedDirectory == null || fixedDirectory.Equals(""))
        {
            fixedDirectory = Application.dataPath + "/";
        }
        else
        {
            fixedDirectory = fixedDirectory + "/";
        }

        if (fixedFileName.Equals(""))
        {
            fixedFileName = ImageCaptureTool.BaseOutputFileName;
        }

        return fixedDirectory + fixedFileName + this.currentFileNameIndex + ".png";
    }

    /// <summary>
    /// キャプチャーの成否を出力します。
    /// </summary>
    /// <param name="success">
    /// キャプチャーの成否。
    /// </param>
    /// <param name="message">
    /// 通知するメッセージ。
    /// </param>
    private void ShowCaptureResult(bool success, string message)
    {
        GUIContent notification = new GUIContent(message);
        this.ShowNotification(notification);
    }

    #endregion Method
}

#endif