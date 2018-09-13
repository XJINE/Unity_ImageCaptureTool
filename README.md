# Unity_ImageCaptureTool

Capture image and output the file with specified camera.
This is useful to make some screenshots or transparent textures.

![](https://github.com/XJINE/Unity_ImageCaptureTool/blob/master/screenshot.png)

## Import to Your Project

You can import this assets from UnityPackage.

- [ImageCaptureTool.unitypackage](https://github.com/XJINE/Unity_ImageCaptureTool/blob/master/ImageCaptureTool.unitypackage)

## How to use

Open window from toolbar "Custom/ImageCaptureTool". Set the parameters & click "Capture" button.

### Features

| Name             | Description                                                           |
|:-----------------|:----------------------------------------------------------------------|
| Output Directory | Output directory.                                                     |
| Base File Name   | Output file name. File name will be combined with ``File Name Index``.|
| File Name Index  | Suffix number. This will be combined after ``Base File Name``.        |
| Camera           | Camera to make capture. If null, ``Main Camerall`` will be used.      |
| Image Width      | Output image width. If ``0``, current GameView width will be used.    |
| Image Height     | Output image height. If ``0``, current GameView height will be used.  |
| Image Scale      | Output image size magnification.                                      |
| Clear Background | If enable, output image background will be transparent.               |

## Via Script

``ImageCaptureToolCore.cs`` has functions to capture image.

## Limitation

If output directory doesn't exist, output will be failed.
