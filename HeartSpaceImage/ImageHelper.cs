using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace HeartSpaceImage
{
    public class ImageHelper

    {
        // 圖片存放根路徑，從 App.config 讀取，或直接硬編碼路徑
        public static readonly string RootPath = GetRootPath();

        // 取得根路徑的方法
        private static string GetRootPath()
        {
            // 如果需要硬編碼路徑，直接返回硬編碼值
            return @"C:\Users\kelly\source\repos\HeartSpace\HeartSpaceImage\Images";

		}

        // 確保資料夾存在
        public static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(RootPath))
            {
                Directory.CreateDirectory(RootPath);
            }
        }
        public static string GetImageUrl(string fileName)
        {
            // 替換為 HeartSpaceImage 專案的實際 URL 和埠號
            string serverUrl = "https://localhost:44378"; 

            // 組合成圖片的完整 URL
            return $"{serverUrl}/Images/{fileName}";
        }


        // 取得完整圖片路徑
        public static string GetImagePath(string fileName)
        {
            return Path.Combine(RootPath, fileName);
        }

        // 刪除圖片
        public static void DeleteImage(string fileName)
        {
            string fullPath = GetImagePath(fileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
