using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace DB
{
    public class MD5_Actions
    {
        public static String GetMd5Hash(MD5 md5Hash, String input)
        {
            // Convert the input String to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a String.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal String.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal String.
            return sBuilder.ToString();
        }

        // Verify a hash against a String.
        public static bool VerifyMd5Hash(MD5 md5Hash, String input, String hash)
        {
            // Hash the input.
            String hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            Console.WriteLine(hashOfInput);
            Console.WriteLine(hash);
            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
