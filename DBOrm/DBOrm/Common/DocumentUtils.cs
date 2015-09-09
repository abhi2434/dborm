using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBOrm.Common
{
    public class DocumentUtils
    {
        public static string GetFilenameOnly(string qualifiedName)
        {
            return Path.GetFileName(qualifiedName);
        }

        public static string GetFileExtensionOnly(string qualifiedName)
        {
            return Path.GetExtension(qualifiedName);
        }

        /// <summary>
        /// Encodes the filename much in the same way as the aspx page is encoded
        /// when it is requested.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string EncodeFilename(string filename)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            byte[] bytes = utf8.GetBytes(filename);
            char[] chars = new char[bytes.Length];
            for (int index = 0; index < bytes.Length; index++)
            {
                chars[index] = Convert.ToChar(bytes[index]);
            }

            string s = new string(chars);
            return s;
        }

        public static byte[] ReadDocument(string path)
        {
            if (!File.Exists(path))
                return new byte[0];

            FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Close();

            return bytes;
        }

        /// <summary>
        /// Encodes non-US-ASCII characters in a string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToHexString(string s)
        {
            char[] chars = s.ToCharArray();
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < chars.Length; index++)
            {
                bool needToEncode = NeedToEncode(chars[index]);
                if (needToEncode)
                {
                    string encodedString = ToHexString(chars[index]);
                    builder.Append(encodedString);
                }
                else
                {
                    builder.Append(chars[index]);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Determines if the character needs to be encoded.
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        private static bool NeedToEncode(char chr)
        {
            string reservedChars = "$-_.+!*'(),@=&";

            if (chr > 127)
                return true;
            if (char.IsLetterOrDigit(chr) || reservedChars.IndexOf(chr) >= 0)
                return false;

            return true;
        }

        /// <summary>
        /// Encodes a non-US-ASCII character.
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        private static string ToHexString(char chr)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            byte[] encodedBytes = utf8.GetBytes(chr.ToString());
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < encodedBytes.Length; index++)
            {
                builder.AppendFormat("%{0}", Convert.ToString(encodedBytes[index], 16));
            }

            return builder.ToString();
        }

    }
}

