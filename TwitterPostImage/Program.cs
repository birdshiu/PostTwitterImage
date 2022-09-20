using System;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace TwitterPostImage
{
    class Program
    {
        static string targetURL = "https://upload.twitter.com/1.1/media/upload.json";
        static string consumerKey = "G4q8ox9SXxxwTNAIcd4jXJz28";
        static string consumerSecret = "mNjSpNnzP8YDlP19h73FKFewE6gR1yUfLlxh71VfMfvvbpEa1T";
        static string accessToken = "1057816229774221312-5oCCqBuan6WMWaZR7iCGkGatJ15zKf";
        static string tokenSecure = "0q6OXW42GIzRi67ZaYBoUnGX1vck2huVYUnUxcyZxgY7v";
        static string filePath="filePath"
        static string nonce;
        static string timeStamp;

        public static string GenerateHash()
        {
            string parameter_string = string.Empty;
            parameter_string += WebUtility.UrlEncode("oauth_consumer_key") + "=" + WebUtility.UrlEncode(consumerKey);
            parameter_string += "&" + WebUtility.UrlEncode("oauth_nonce") + "=" + WebUtility.UrlEncode(nonce);
            parameter_string += "&" + WebUtility.UrlEncode("oauth_signature_method") + "=" + WebUtility.UrlEncode("HMAC-SHA1");
            parameter_string += "&" + WebUtility.UrlEncode("oauth_timestamp") + "=" + WebUtility.UrlEncode(timeStamp);
            parameter_string += "&" + WebUtility.UrlEncode("oauth_token") + "=" + WebUtility.UrlEncode(accessToken);
            parameter_string += "&" + WebUtility.UrlEncode("oauth_version") + "=" + WebUtility.UrlEncode("1.0");

            string base_string = "POST&" + WebUtility.UrlEncode(targetURL);
            base_string += "&" + WebUtility.UrlEncode(parameter_string);

            string signing_key = WebUtility.UrlEncode(consumerSecret) + "&" + WebUtility.UrlEncode(tokenSecure);

            return SignData(base_string, signing_key);
        }

        static string SignData(string message, string secret)
        {
            var encoding = new System.Text.UTF8Encoding();
            var keyBytes = encoding.GetBytes(secret);
            var messageBytes = encoding.GetBytes(message);
            using (var hmashal = new HMACSHA1(keyBytes))
            {
                var hashMessage = hmashal.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashMessage);
            }
        }

        static string GenerateAuthString()
        {
            nonce = Guid.NewGuid().ToString().Replace("-", "");
            timeStamp = Convert.ToString((int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            string result = string.Empty;
            result += "OAuth ";
            result += "oauth_consumer_key=" + "\"" + consumerKey + "\",";
            result += "oauth_token=" + "\"" + accessToken + "\",";
            result += "oauth_signature_method=\"HMAC-SHA1\",";
            result += "oauth_timestamp=" + "\"" + timeStamp + "\",";
            result += "oauth_nonce=" + "\"" + nonce + "\",";
            result += "oauth_version=\"1.0\",";
            result += "oauth_signature=" + "\"" + WebUtility.UrlEncode(GenerateHash()) + "\"";

            return result;
        }

        static void TweetImage(string fileName)
        {
            byte[] byteImage = File.ReadAllBytes(fileName);

            var memStream = new MemoryStream();
            string boundary = "----------" + DateTime.Now.Ticks.ToString("x");

            var beginBoundary = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
            var endBoundary = Encoding.ASCII.GetBytes("--" + boundary + "--");


            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(targetURL);
            myReq.Headers["Authorization"] = GenerateAuthString();
            myReq.Method = "POST";
            myReq.ContentType = "multipart/form-data; boundary="+boundary;
            myReq.Timeout = 3000;

            string header = "Content-Disposition: form-data; name=\"media\"; filename=\"JunInzWh.jpg\"\r\n\r\n";
            var headerbytes = Encoding.UTF8.GetBytes(header);

            memStream.Write(beginBoundary, 0, beginBoundary.Length);
            memStream.Write(headerbytes, 0, headerbytes.Length);

            memStream.Write(byteImage, 0, byteImage.Length);

            var newLine = Encoding.ASCII.GetBytes("\r\n");
            memStream.Write(newLine, 0, newLine.Length);

            memStream.Write(endBoundary, 0, endBoundary.Length);
            myReq.ContentLength = memStream.Length;

            memStream.Position = 0;
            var tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Close();

            var requestStream = myReq.GetRequestStream();
            requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            requestStream.Close();

            string result = String.Empty;
            using (HttpWebResponse response = (HttpWebResponse)myReq.GetResponse())
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }

            Console.WriteLine(result);
            myReq.Abort();
        }

        static void Main(string[] args)
        {
            TweetImage(filePath);
            Console.ReadKey();
        }
    }
}
