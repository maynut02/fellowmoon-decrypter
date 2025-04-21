<p align="center">
  <img src="icon.png" width="200" alt="icon" />
</p>

<h1 align="center">FellowMoon AssetBundle Decryption Tool</h1>

<div align="center">

A decryption tool for .ab files from [**新月同行**](https://xytx.firewick.net/home)

[**신월동행**](https://xytx.firewick.net/home) .ab 파일 복호화 도구
  
</div>

<p align="center">
  <img alt="license" src="https://img.shields.io/github/license/maynut02/fellowmoon-decrypter">
  <img alt=".NET" src="https://img.shields.io/badge/.NET-≥%208-512BD4?logo=csharp">
  <img src="https://img.shields.io/github/v/release/maynut02/fellowmoon-decrypter.svg" />
  <img src="https://img.shields.io/badge/platform-Windows-blue" />
</p>

<p align="center">
  <a href="https://github.com/maynut02/fellowmoon-decrypter/releases/latest">
    <img src="https://img.shields.io/badge/Download-ZIP-brightgreen?style=for-the-badge&logo=windows" />
  </a>
</p>

<div><br></div>

> This tool is an unofficial utility and is not affiliated with Firewick Network or the developers of FellowMoon.<br>
> All rights to the assets of FellowMoon belong to Firewick Network.<br>
> The developer of this tool assumes no responsibility for any issues or damages caused by its use.

###

> 이 도구는 비공식 유틸리티이며, Firewick Network 또는 신월동행 제작진과는 아무런 관련이 없습니다.<br>
> 신월동행의 모든 에셋에 대한 권리는 Firewick Network에 있습니다.<br>
> 이 도구의 사용으로 인해 발생하는 문제나 피해에 대해 제작자는 어떠한 책임도 지지 않습니다.

<div><br></div>

## 요구사항

- .NET 8.0

<div><br></div>

## 🚀 How to Use

1. [Download the latest release](https://github.com/maynut02/fellowmoon-decrypter/releases/latest)
2. Place `.ab` files from FellowMoon into the `import/` folder
3. Run `FMDC.exe`
4. Decrypted `.ab` files will be automatically saved in the `export/` folder
5. Load the `export/` folder using an unpacking tool like [AssetStudio](https://github.com/zhangjiequan/AssetStudio)

###

1. [마지막 릴리즈 파일 다운로드](https://github.com/maynut02/fellowmoon-decrypter/releases/latest)
2. 신월동행의 `.ab` 파일을 `import/` 폴더 내부에 넣기
3. `FMDC.exe` 실행
4. 복호화된 `.ab` 파일이 `export/` 폴더 내부에 자동으로 생성
5. [AssetStudio](https://github.com/zhangjiequan/AssetStudio)와 같은 언패킹 도구에서 `export/` 폴더를 로드

<div><br></div>

## 🔧 Parse

- Encryption Algorithm – `AES-256 (CTR mode)`
- Key Derivation Method – `Fixed password + salt → Repeated SHA-1 hashing`
- Counter Initialization – `64-bit counter + 64-bit zero-filled suffix`
- Fixed Password – `System.Byte[]`
- Salt – `Filename without the extension`

###

- 암호화 알고리즘 - `AES-256 (CTR 모드)`
- 복호화 키 파생 방식 - `고정 비밀번호 + salt → SHA-1 해시 반복`
- 카운터 초기값 - `64비트 카운터 + 64비트 0으로 채운 suffix`
- 고정 비밀번호 - `System.Byte[]`
- salt - `확장자를 제외한 파일명`

###

[Python Code](https://github.com/maynut02/fellowmoon-decrypter/blob/master/python/main.py)

<div><br></div>

## 📄 License

[MIT License](https://github.com/maynut02/fellowmoon-decrypter/blob/main/LICENSE)