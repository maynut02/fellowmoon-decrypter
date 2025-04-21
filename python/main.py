import os
import sys
from hashlib import sha1
from Crypto.Cipher import AES
from Crypto.Util import Counter


# AES CTR + 커스텀 PBKDF1 방식의 복호화를 처리하는 클래스
class AesEcbPBKDF1:
    def __init__(self):
        # 주요 암호화 구성 요소를 인스턴스에 바인딩
        self.aes, self.count, self.sha1 = AES, Counter, sha1

    # 비밀번호와 salt로 암호화 키 생성 (SHA1 반복 해싱 기반)
    def getkey(self, password, salt, keylen, count):
        index, count = 1, count - 1  # 해싱 반복 횟수 조정
        # 초기 해시 = 비밀번호와 salt를 결합한 SHA1 해시
        hashval = self.sha1(
            (password.encode("utf-8") if isinstance(password, str) else password) + salt
        ).digest()

        # count - 1 만큼 추가 해싱
        for _ in range(count - 1):
            hashval = self.sha1(hashval).digest()

        # 최종 해시 기반 파생 키 생성
        hashder = self.sha1(hashval).digest()
        while len(hashder) < keylen:
            hashder += self.sha1(bytes([index + 48]) + hashval).digest()
            index += 1

        return hashder[:keylen]

    # AES CTR 모드 복호화
    def decrypt(self, data, password, salt, keylen=32, count=100):
        key = self.getkey(password, salt, keylen, count)
        # 64비트 nonce + 64비트 suffix 조합 (suffix는 0으로 채움)
        ctr = self.count.new(64, suffix=b"\x00" * 8, little_endian=True)
        return self.aes.new(key, self.aes.MODE_CTR, counter=ctr).decrypt(data)


# 지정 폴더 내 .ab 파일을 복호화하는 메인 처리 함수
def decrypt_ab_files(input_folder, output_folder):
    pbkdf1 = AesEcbPBKDF1()
    ab_files = []

    # 재귀적으로 .ab 파일 탐색
    for root, _, files in os.walk(input_folder):
        for file in files:
            if file.endswith(".ab"):
                ab_files.append(os.path.join(root, file))

    total = len(ab_files)
    print(f"🔄 총 {total}개의 .ab 파일을 복호화합니다.\n")

    for idx, file_path in enumerate(ab_files, 1):
        file_name = os.path.basename(file_path)
        # salt = 파일 이름
        salt = file_name.replace(".ab", "").encode("utf-8")

        # 암호화된 파일 읽기
        with open(file_path, "rb") as f:
            file_bytes = f.read()

        # 고정된 패스워드로 복호화 수행
        decrypted_data = pbkdf1.decrypt(file_bytes, "System.Byte[]", salt, 32, 100)

        # 원래 경로 구조를 유지한 채 출력 파일 경로 설정
        rel_path = os.path.relpath(file_path, input_folder)
        out_filename = os.path.splitext(rel_path)[0] + "_decrypt.ab"
        output_path = os.path.join(output_folder, out_filename)
        os.makedirs(os.path.dirname(output_path), exist_ok=True)

        # 복호화된 내용 저장
        with open(output_path, "wb") as f:
            f.write(decrypted_data)

        # 진행 상황 출력 (진행률 표시 포함)
        bar_length = 30  # 진행바 길이
        progress = idx / total
        filled_len = int(bar_length * progress)
        bar = '█' * filled_len + '░' * (bar_length - filled_len)
        percent = int(progress * 100)

        sys.stdout.write(f"\r🛠️  복호화 중 {bar} [{idx}/{total}] {percent}%")
        sys.stdout.flush()

    print("\n\n✅ 모든 복호화 작업이 완료되었습니다.")


if __name__ == "__main__":
    base_path = os.path.dirname(os.path.abspath(sys.argv[0]))
    input_folder = os.path.join(base_path, "import")
    output_folder = os.path.join(base_path, "export")

    decrypt_ab_files(input_folder, output_folder)

    input("\n프로그램을 종료하려면 아무 키나 입력하세요...")
