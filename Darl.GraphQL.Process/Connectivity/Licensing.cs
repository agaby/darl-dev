using Microsoft.Extensions.Configuration;
using Standard.Licensing;
using Standard.Licensing.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Darl.GraphQL.Models.Connectivity
{
    public class ProductLicensing : ILicensing
    {


        public ProductLicensing(IConfiguration config)
        {
            publicKey = config["Licensing:publicLicenseGeneratorKey"];
            privateKey = config["Licensing:privateLicenseGeneratorKey"];
            passPhrase = config["Licensing:privateLicensePassPhrase"];
        }

        private readonly string publicKey;
        private readonly string privateKey;
        private readonly string passPhrase;

        public string CreateKey(DateTime endDate, string company, string email)
        {
            var license = License.New()
                .As(LicenseType.Standard)
                .ExpiresAt(endDate)
                .LicensedTo(company, email)
                .CreateAndSignWithPrivateKey(privateKey, passPhrase);
            var licenseText = license.ToString();
            return Compress(licenseText);
        }


        public static string Compress(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream();
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }
            ms.Position = 0;
            MemoryStream outStream = new MemoryStream();
            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);
            byte[] gzBuffer = new byte[compressed.Length + 4];
            System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return Convert.ToBase64String(gzBuffer);
        }

        public static string Decompress(string compressedText)
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

        public bool CheckKey(string key)
        {
            List<IValidationFailure> validationFailures = new List<IValidationFailure>();
            try
            {
                var decompressed = Decompress(key);
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
    }
}

