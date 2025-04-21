using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace fellowmoon_decrypter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 실행 파일 기준으로 import/export 폴더 경로
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var inputFolder = Path.Combine(basePath, "import");
            var outputFolder = Path.Combine(basePath, "export");

            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine($"입력 폴더가 없습니다: {inputFolder}");
                return;
            }

            var abFiles = Directory.GetFiles(inputFolder, "*.ab", SearchOption.AllDirectories);
            Console.WriteLine($"총 {abFiles.Length}개의 .ab 파일을 복호화합니다.\n");

            int idx = 0;
            foreach (var file in abFiles)
            {
                idx++;
                // 상대경로 → 출력파일 경로 (_decrypt.ab 붙이기)
                var relPath = Path.GetRelativePath(inputFolder, file);
                var outRel = Path.ChangeExtension(relPath, null) + "_decrypt.ab";
                var outputPath = Path.Combine(outputFolder, outRel);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)
                                          ?? throw new InvalidOperationException());

                // salt: 파일명(.ab 제외)
                var salt = Encoding.UTF8.GetBytes(Path.GetFileNameWithoutExtension(file));
                var encrypted = File.ReadAllBytes(file);

                // 복호화
                var decrypted = DecryptAesCtrPbkdf1(encrypted,
                                                    password: "System.Byte[]",
                                                    salt: salt,
                                                    keyLen: 32,
                                                    countParam: 100);

                // UnityFS 헤더 검사
                var header = Encoding.ASCII.GetString(decrypted, 0, Math.Min(5, decrypted.Length));
                if (header != "Unity")
                    Console.WriteLine($"[{idx}/{abFiles.Length}] {relPath} → 헤더 불일치: \"{header}\"");

                File.WriteAllBytes(outputPath, decrypted);
                Console.WriteLine($"[{idx}/{abFiles.Length}] {relPath} → {outRel}");
            }

            Console.WriteLine("\n모든 복호화 작업이 완료되었습니다.");
            Console.WriteLine("프로그램을 종료하려면 아무 키나 누르세요...");
            Console.ReadKey();
        }

        // Python 코드와 동일한 PBKDF1‑SHA1 반복 해싱
        static byte[] GetKeyPbkdf1(string password, byte[] salt, int keyLen, int countParam)
        {
            using var sha1 = SHA1.Create();

            int index = 1;
            int count = countParam - 1;            // Python 쪽: index, count = 1, countParam-1

            // 1) 초기 해시: sha1(password || salt)
            var pwdBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashval = sha1.ComputeHash(pwdBytes.Concat(salt).ToArray());

            // 2) (count - 1)회 추가 해싱
            for (int i = 0; i < count - 1; i++)
                hashval = sha1.ComputeHash(hashval);

            // 3) 최종 해시(hashder) 생성
            byte[] hashder = sha1.ComputeHash(hashval);

            // 4) keyLen이 될 때까지 확장
            while (hashder.Length < keyLen)
            {
                // sha1( byte(index+48) || hashval )
                byte b = (byte)(index + 48);
                var extra = sha1.ComputeHash(new[] { b }.Concat(hashval).ToArray());
                hashder = hashder.Concat(extra).ToArray();
                index++;
            }

            return hashder.Take(keyLen).ToArray();
        }

        // AES-CTR 모드 직접 구현 (ECB 블록 암호화 → XOR)
        static byte[] DecryptAesCtrPbkdf1(byte[] data, string password, byte[] salt, int keyLen, int countParam)
        {
            var key = GetKeyPbkdf1(password, salt, keyLen, countParam);

            using var aes = Aes.Create();
            aes.KeySize = keyLen * 8;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            aes.Key = key;

            int blockSize = aes.BlockSize / 8;     // =16
            var result = new byte[data.Length];
            ulong counter = 1;                     // Python default initial_value=1

            for (int offset = 0; offset < data.Length; offset += blockSize)
            {
                // 64비트 LE 카운터 + 8바이트 0
                var counterBlock = new byte[blockSize];
                var ctrBytes = BitConverter.GetBytes(counter);
                // 플랫폼이 LittleEndian이라면 그대로 복사
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(ctrBytes);
                Array.Copy(ctrBytes, 0, counterBlock, 0, 8);

                // ECB로 카운터 블록 암호화 → keystream
                byte[] keystream;
                using (var enc = aes.CreateEncryptor())
                    keystream = enc.TransformFinalBlock(counterBlock, 0, blockSize);

                // XOR
                int chunk = Math.Min(blockSize, data.Length - offset);
                for (int i = 0; i < chunk; i++)
                    result[offset + i] = (byte)(data[offset + i] ^ keystream[i]);

                counter++;
            }

            return result;
        }
    }
}
