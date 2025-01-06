using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeartSpace.Helpers
{
    public static class ImageHelper
    {  // 將 byte[] 轉換為 Base64 字串
        public static string ToBase64String(this HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0) return null;

            using (var binaryReader = new System.IO.BinaryReader(file.InputStream))
            {
                var fileBytes = binaryReader.ReadBytes(file.ContentLength);
                return Convert.ToBase64String(fileBytes);
            }
        }

        // 將 Base64 字串轉換為 byte[]
        public static byte[] FromBase64String(this string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                return null;
            }
            return Convert.FromBase64String(base64String);
        }
    }
}