// Utils/CodeGenerator.cs
using System;
using System.Linq;

namespace ProductManager.Utils
{
    public static class CodeGenerator
    {
        // 包含大小寫英文與數字，共 62 個字元
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        
        public static string Generate(int length = 8)
        {
            // 使用 .NET 8 推薦的 Random.Shared 兼顧效能與執行緒安全
            return string.Create(length, Alphabet, (span, alphabet) =>
            {
                for (int i = 0; i < span.Length; i++)
                {
                    span[i] = alphabet[Random.Shared.Next(alphabet.Length)];
                }
            });
        }
    }
}