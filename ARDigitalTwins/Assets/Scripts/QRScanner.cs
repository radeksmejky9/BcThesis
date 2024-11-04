using System.Collections;
using UnityEngine;
using ZXing;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;
using System;
using TMPro;

public class QRScanner : MonoBehaviour
{
    /* public static Action<String> OnQRScanned;

     public ARCameraManager CameraManager;

     [SerializeField] private TextMeshProUGUI txt;

     public bool ScanningMode
     {
         get { return scanningMode; }
         private set
         {
             qrCode = string.Empty;

             if (value == true)
             {
                 txt.text = "Scanning...";
                 StartCoroutine(GetQRCode());
             }

             scanningMode = value;
         }
     }


     private bool scanningMode = false;
     private string qrCode = string.Empty;

     void Start()
     {
         ScanningMode = true;
     }

     private IEnumerator GetQRCode()
     {
         IBarcodeReader barCodeReader = new BarcodeReader();
         while (string.IsNullOrEmpty(qrCode))
         {
             if (CameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
             {
                 var cameraImageTexture = GetImageTexture(image);
                 try
                 {
                     var Result = barCodeReader.Decode(cameraImageTexture.GetPixels32(), cameraImageTexture.width, cameraImageTexture.height);
                     if (Result != null)
                     {
                         qrCode = Result.Text;
                         if (!string.IsNullOrEmpty(qrCode))
                         {
                             txt.text = qrCode;
                             OnQRScanned?.Invoke(qrCode);
                             ScanningMode = false;
                             break;
                         }
                     }
                 }
                 catch (Exception ex)
                 {
                     Debug.Log("Failed to scan");
                     Debug.LogError(ex.Message);
                     qrCode = string.Empty;
                 }
                 finally
                 {
                     image.Dispose();
                 }
             }
             yield return null;
         }
     }

     private Texture2D GetImageTexture(XRCpuImage image)
     {

         int regionWidth = image.width / 2;
         int regionHeight = image.height / 2;

         var outputDimensions = new Vector2Int(regionWidth / 2, regionHeight / 2);
         var textureFormat = TextureFormat.ARGB32;

         var conversionParams = new XRCpuImage.ConversionParams
         {
             outputDimensions = outputDimensions,
             outputFormat = textureFormat,
             transformation = XRCpuImage.Transformation.None
         };

         var cameraImageTexture = new Texture2D(regionWidth / 2, regionHeight / 2, textureFormat, false);
         var rawImageData = new NativeArray<byte>(image.GetConvertedDataSize(conversionParams), Allocator.Temp);
         image.Convert(conversionParams, rawImageData);

         cameraImageTexture.LoadRawTextureData(rawImageData);
         cameraImageTexture.Apply();

         rawImageData.Dispose();
         return cameraImageTexture;
     }*/

}