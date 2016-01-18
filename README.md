# [뎃찌](https://github.com/Usagination/Decchi/releases/latest) (Decchi)

- [뎃찌 와 함께 데스크탑에서 #NowPlaying 을 게시해요](#뎃찌-와-함께-데스크탑에서-nowplaying-을-게시해요)
- [설치](#설치)
- [사용법 : Just click 뎃찌!!](#사용법--just-click-뎃찌)
- [뎃찌의 기능 톺아보기](#뎃찌의-기능-톺아보기)
  - [빠른 뎃찌!!](#빠른-뎃찌)
  - [작은 뎃찌](#작은-뎃찌)
  - [로컬 파일 인식 설정](#로컬-파일-인식-설정)
  - [뮤직 플레이어 뎃찌!!](#뮤직-플레이어-뎃찌)
    - [뎃찌EXT](#뎃찌ext)
  - [웹 페이지 뎃찌!!](#웹-페이지-뎃찌)
  - [클라이언트 자동 선택](#클라이언트-자동-선택)
  - [포맷팅](#포맷팅)
- [LICENSE](#license)

## **뎃찌** 와 함께 데스크탑에서 #NowPlaying 을 게시해요

- 우리는 이 놀라운 기능을 **뎃찌!!**라고 이름붙였답니다. **느낌표가 두개**에요!!

- [**사긔**](https://github.com/Usagination)와 [**륜**](https://github.com/RyuaNerin)이 열씸히 만들었어요ㅇ.<!

- 버그나 기능 개선 문의는 언제나 환영이에요!

- 패치 노트는 [여기](patch-notes/) 를 읽어주세요!

## 설치

- [**.NET Framework 4.5**](http://www.microsoft.com/ko-kr/download/details.aspx?id=30653) 이상 버전이 필요해요.

- 별도의 설치 과정 없이 [**여기**](https://github.com/Usagination/Decchi/releases/latest)에서 다운로드받은 파일을 실행해주면 돼요.

## 사용법 : *Just click 뎃찌!!*

## 뎃찌의 기능 톺아보기

### 빠른 뎃찌!!

- Ctrl + Q 를 눌러 다른 작업 중에도 뎃찌할 수 있어요. (기본 설정)

- 설정에서 단축키를 원하는 키 조합으로 바꿀 수 있어요. 단축키 설정 상자를 클릭한 후 원하는 키 조합을 입력하면 된답니다.

### 작은 뎃찌

- 설정에서 `작은 뎃찌로 변경` 을 누르면 뎃찌가 작아져요!

### 로컬 파일 인식 설정

- **현재 컴퓨터에서 저장된 파일을 재생하는지 인식**하고 성공하면 그 음악 파일을 통해서 트윗해요!

- 사용하려면 `로컬 파일 인식` 설정을 켜주세요!

### 뮤직 플레이어 뎃찌!!

- 기본 설정일때 인식이 제일 잘 되어요.

|클라이언트 이름|곡 이름|아티스트 이름|앨범 이름|앨범 아트|
|---|:-:|:-:|:-:|:-:|
|iTunes|O|O|O|O|
|Foobar 2000|O|O|O||
|곰오디오|O|O|||
|알송|O|O|||
|멜론|O|O|||
|AIMP3|O|O|||
|Winamp|O|O|||
|Windows Media Player|O|O||||

#### 뎃찌EXT

- 음악 플레이어에 데찌와 연동되는 확장 기능을 설치해서 더 정확하게 정보를 얻어올 수 있어요!

- 확장 기능을 지원하는 플레이어에서만 사용할 수 있어요.

- 현재 지원하는 플레이어

 - Foobar 2000

### 웹 페이지 뎃찌!!

- 지원하는 웹 페이지

 - Youtube

 - ニコニコ動画

- 지원하는 브라우저

 - Chrome

 - FireFox

- `심화 웹 브라우저 인식` 설정

 - 현재 **선택된 탭**은 주소도 같이 뎃찌!! 한답니다!

 - 현재 선택된 탭이 아닌 **다른 탭** 을 인식 할 수 있어요!

 - **Chrome 의 경우에는 기술상의 문제로 최소화를 하면 다른 탭을 인식하지 못해요** . _.)

### 클라이언트 자동 선택

- 뎃찌는 여러개의 플레이어가 감지되면 어떤것을 트윗할지 물어본답니다!

- 하지만 귀찮은 여러분들을 위해서 `플레이어 자동 선택` 설정을 준비했어요!

### 포맷팅

- 뎃찌는 (자칭)**강력한 포맷팅** 기능을 지원해요.

- 포맷팅이란 곡의 제목, 아티스트 이름 등을 이용해 게시를 위한 완전한 문자열을 만드는 작업입니다.

- 뎃찌와 약속한 몇 개의 문자열들을 사용해 손쉽게 포맷팅을 정의해보세요!

- 아래는 뎃찌의 약속어 목록이에요.

 |약속어|치환|
|:-:|---|
|`Title`|곡의 제목|
|`Artist`|아티스트 이름|
|`Album`|앨범 이름|
|`Client`|재생중인 클라이언트|
|`Via`|뎃찌의 홍보문구에요 ㅇ.<|

 - 약속어는 '{'와 '}' 안에 '/'로 감싸서 써주셔야 해요.

 - 중괄호 안, 약속어 앞 뒤에 쓰인 문자열은 치환 값이 비어있을 경우 포맷팅에서 제외되니 유용하게 사용할 수 있어요.

 - 만약 { 이나 } 을 쓰고싶으면 \\{ 이나 \\} 으로 적으면 돼요!

 - \\n 을 적어서 줄바꿈을 할 수도 있어요!

- 예) 포맷팅 문자열 : {***/Artist/*** 의 }{***/Title/***{ (***/Album/***)} 을 }듣고 있어요! {***/Via/***} - {***/Client/***}

 |Title|Artist|Album|Client|Output|
|:-:|:-:|:-:|:-:|---|
|夜もすがら君想ふ|Chalili|-|곰오디오|***Chalili***의 ***夜もすがら君想ふ*** 을 듣고 있어요! ***#뎃찌NP*** - ***곰오디오***|
|Dreams|Imagine Dragons|Smoke + Mirrors|iTunes|***Imagine Dragons*** 의 ***Dreams*** (***Smoke + Mirrors***) 을 듣고 있어요! ***#뎃찌NP*** - ***iTunes***|
|Shanhai Alice in 1884 feat.nomico|-|-|알송|***Shanhai Alice in 1884 feat.nomico*** 을 듣고 있어요! ***#뎃찌NP*** - ***알송***|

## LICENSE

- 뎃찌는 [MIT LICENSE](LICENSE.txt) 를 따라요

- 사용된 오픈소스

 - [RyuaNerin/MahApps.Metro v1.2.2 (forked from MahApps/MahApps.Metro)](https://github.com/RyuaNerin/MahApps.Metro)

 - [Hardcodet.NotifyIcon v1.0.5](Decchi/ExternalLibrarys/Hardcodet.NotifyIcon.Wpf-1.0.5)

 - [TabLib.Portable v1.0.3](Decchi/ExternalLibrarys/TagLib.Portable-1.0.3)

 - [UIAComWrapper-1.1.0.14](Decchi/ExternalLibrarys/UIAComWrapper-1.1.0.14)
