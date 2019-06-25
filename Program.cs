using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace big_file_comparison
{
    class Program
    {
        static void Main(string[] args)
        {
            var file1 = Path.Combine(Directory.GetCurrentDirectory(), "jenkins.msi");
            var file2 = Path.Combine(Directory.GetCurrentDirectory(), "jenkins2.msi");

            var methods = new Func<string, string, bool>[] { CompareByMD5, CompareByString, CompareByByteArry, CompareByReadOnlySpan };
            foreach (var method in methods)
            {
                var sw = Stopwatch.StartNew();
                bool identical = method(file1, file2);
                Console.WriteLine("Method: {0}, Identical: {1}. Elapsed: {2}", method.Method.Name, identical, sw.Elapsed);
            }
        }

        private static bool CompareByReadOnlySpan(string file1, string file2)
        {
            const int BYTES_TO_READ = 1024 * 10;
            using (FileStream fs1 = File.Open(file1, FileMode.Open))
            using (FileStream fs2 = File.Open(file2, FileMode.Open))
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];
                while (true)
                {
                    int len1 = fs1.Read(one, 0, BYTES_TO_READ);
                    int len2 = fs2.Read(two, 0, BYTES_TO_READ);
                    // byte[] can be cast to ReadOnlySpan
                    if (!((ReadOnlySpan<byte>)one).SequenceEqual((ReadOnlySpan<byte>)two)) return false;
                    if (len1 == 0 || len2 == 0) break;  
                }
            }
            return true;
        }

        private static bool CompareByByteArry(string file1, string file2)
        {
            const int BYTES_TO_READ = 1024 * 10;
            using (FileStream fs1 = File.Open(file1, FileMode.Open))
            using (FileStream fs2 = File.Open(file2, FileMode.Open))
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];
                while (true)
                {
                    int len1 = fs1.Read(one, 0, BYTES_TO_READ);
                    int len2 = fs2.Read(two, 0, BYTES_TO_READ);
                    int index = 0;
                    while (index < len1 && index < len2)
                    {
                        if (one[index] != two[index]) return false;
                        index++;
                    }
                    if (len1 == 0 || len2 == 0) break;
                }
            }
            return true;
        }

        private static bool CompareByString(string file1, string file2)
        {
            const int BYTES_TO_READ = 1024 * 10;
            using (FileStream fs1 = File.Open(file1, FileMode.Open))
            using (FileStream fs2 = File.Open(file2, FileMode.Open))
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];
                while (true)
                {
                    int len1 = fs1.Read(one, 0, BYTES_TO_READ);
                    int len2 = fs2.Read(two, 0, BYTES_TO_READ);
                    if (BitConverter.ToString(one) != BitConverter.ToString(two)) return false;
                    if (len1 == 0 || len2 == 0) break;  
                }
            }
            return true;
        }

        private static bool CompareByMD5(string file1, string file2)
        {
            using (var md5 = MD5.Create())
            {
                byte[] one, two;

                using (var fs1 = File.Open(file1, FileMode.Open))
                {
                    one = md5.ComputeHash(fs1);
                }

                using (var fs2 = File.Open(file2, FileMode.Open))
                {
                    two = md5.ComputeHash(fs2);
                }

                return BitConverter.ToString(one).Equals(BitConverter.ToString(two));
            }
        }
    }
}
