using DarlLanguage.Processing;
using Standard.Licensing.Validation;
using Standard.Licensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Darl.Licensing
{
    public static class DarlLicense
    {
        public static bool licensed { get { return _licensed ?? false; } }

        public static string license { get { return _license; } set { _license = value; _licensed = ProcessLicense(value); } }

        private static readonly string publicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEv5iZM5k8XaSHaEg7g7IBAQKAGgdjt5ePjWXWJwLJnYgiotX/uYt4uKrimOsz5jR5U5b+sG+EuT9d3hHRZld/UQ==";

        private static string _license = "";

        private static bool? _licensed = true;

        internal static bool ProcessLicense(string license)
        {
            List<IValidationFailure> validationFailures = new List<IValidationFailure>();
            try
            {
                var decompressed = Decompress(license);
                var licenseObj = License.Load(decompressed);
                validationFailures = licenseObj.Validate()
                                    .ExpirationDate()
                                    .And()
                                    .Signature(publicKey)
                                    .AssertValidLicense().ToList();
            }
            catch
            {
                return false;
            }
            if (validationFailures.Any())
            {
                return false;
            }
            return true;
        }

        private static string Decompress(string compressedText)
        {
            byte[] gzBuffer = Convert.FromBase64String(compressedText);
            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
