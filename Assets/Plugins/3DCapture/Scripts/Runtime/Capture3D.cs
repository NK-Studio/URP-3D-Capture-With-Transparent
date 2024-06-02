using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace NKStudio
{
    public class Capture3D : MonoBehaviour
    {
        [SerializeField] private EOutputResolution outputResolution = EOutputResolution.FHD;
        [SerializeField] private string filePath;
        [SerializeField] string fileName = "Capture";
        [SerializeField] private bool useHDR = true;

        private static readonly Color OriginCameraColor = new Color(0, 0.05660379f, 0, 0f);

        /// <summary>
        /// 스크린샷을 찍습니다.
        /// </summary>
        /// <returns>결과 위치</returns>
        public string ScreenShotClick()
        {
            Camera targetCamera = Camera.main;

            RenderTexture currentRT = RenderTexture.active;

            if (targetCamera != null)
            {
                targetCamera.backgroundColor = Color.clear;

                RenderTexture tempRT;

                if (useHDR)
                {
                    tempRT = new RenderTexture(targetCamera.pixelWidth, targetCamera.pixelHeight, 24, DefaultFormat.HDR);
                }
                else
                {
                    tempRT = new RenderTexture(targetCamera.pixelWidth, targetCamera.pixelHeight, 24, GraphicsFormat.R32G32B32A32_SFloat); 
                }
                
                RenderTexture.active = tempRT;

                targetCamera.targetTexture = tempRT;
                targetCamera.Render();

                Texture2D texture;

                if (useHDR)
                {
                    texture = new Texture2D(targetCamera.targetTexture.width, targetCamera.targetTexture.height,
                        TextureFormat.RGBAFloat, false);
                }
                else
                {
                    texture = new Texture2D(targetCamera.targetTexture.width, targetCamera.targetTexture.height,
                        TextureFormat.RGBA32, false);
                }

                texture.ReadPixels(new Rect(0, 0, targetCamera.targetTexture.width, targetCamera.targetTexture.height),
                    0, 0);

                texture.Apply();

                // 감마 보정 방지
                if (!useHDR)
                {
                    Color[] pixels = texture.GetPixels();
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = pixels[i].gamma;
                    }
                
                    texture.SetPixels(pixels);
                    texture.Apply();
                }

                RenderTexture.active = currentRT;

                int count = 0;
                string modifiedFileName = $"{fileName}-{count}";

                string extension = useHDR ? "exr" : "png";

                // 해당 경로에 이미 존재한다면 카운트를 적용한다.
                while (File.Exists($"{filePath}/{modifiedFileName}.{extension}"))
                {
                    modifiedFileName = $"{fileName}-{count}";
                    count += 1;
                }

                string targetPath = $"{filePath}/{modifiedFileName}.{extension}";

                if (useHDR)
                    File.WriteAllBytes(targetPath, texture.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat));
                else
                    File.WriteAllBytes(targetPath, texture.EncodeToPNG());

                Debug.Log($"{targetPath} 경로에 저장되었습니다.");

                targetCamera.targetTexture = null;
                DestroyImmediate(tempRT);
                DestroyImmediate(texture);

                targetCamera.backgroundColor = OriginCameraColor;

                return targetPath;
            }

            return null;
        }
    }
}