using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

namespace FMCOLOR
{
    public class FMCOLOR_DemoScreenshot : MonoBehaviour
    {
        // Update is called once per frame
        private void Update()
        {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.S))SaveScreenshot();
#endif
        }

        int order = 0;
        private void SaveScreenshot()
        {
            string path = Directory.GetParent(Application.dataPath).ToString() + "/Screenshots/";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string SavePath = path + SceneManager.GetActiveScene().name + order + ".png";
            ScreenCapture.CaptureScreenshot(SavePath);
            order++;
            print(SavePath);
        }
    }
}