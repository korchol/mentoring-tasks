﻿using System.Security.Cryptography;

namespace ProfilingTools.Task1.Hash
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GeneratePasswordHashUsingSalt("passwordText", new byte[] { 143, 108, 122, 29, 155, 62, 76, 95, 106, 123, 140, 157, 14, 31, 42, 59 });
        }

        public static string GeneratePasswordHashUsingSalt(string passwordText, byte[] salt)
        {
            var iterate = 10000;
            using (var pbkdf2 = new Rfc2898DeriveBytes(passwordText, salt, iterate))
            {
                byte[] hash = pbkdf2.GetBytes(20);
                byte[] hashBytes = new byte[36];
                
                Buffer.BlockCopy(salt, 0, hashBytes, 0, 16);
                Buffer.BlockCopy(hash, 0, hashBytes, 16, 20);

                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}