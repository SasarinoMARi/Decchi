# [뎃찌](https://github.com/Usagination/Decchi/releases/latest) (Decchi)

## **뎃찌** 와 함께 데스크탑에서 #NowPlaying 을 게시해요

- 우리는 이 놀라운 기능을 **뎃찌!!**라고 이름붙였답니다. 느낌표가 두개에요!!

- [**사긔**](https://github.com/Usagination)와 [**륜**](https://github.com/RyuaNerin)이 열씸히 만들었어요ㅇ.<!

- 버그나 기능 개선 문의는 언제나 환영이에요!


## 지원 클라이언트 및 파싱 구현 톺아보기

### 로컬 파일 인식 설정

- 현재 컴퓨터에서 저장된 파일을 재생하는지 인식하고 성공하면 그 음악 파일을 통해서 트윗해요!

### 기본 인식 (기본 설정에서 인식)

|클라이언트 이름|곡 이름|아티스트 이름|앨범 이름|앨범 아트|
|---|:-:|:-:|:-:|:-:|
|Windows Media Player|O|O|||
|곰오디오|O|O|||
|iTunes|O|O|O|O|
|알송|O|O|||
|Melon|O|O|||
|Winamp|O|O|||
|AIMP3|O|O|||
|Foobar 2000|O|O|O||
|웹 브라우저에서 보는 유튜브|O|||URL|
|웹 브라우저에서 보는 ニコニコ動画|O|||URL|

## 설치

- [**.NET Framework 4.5**](http://www.microsoft.com/ko-kr/download/details.aspx?id=30653) 이상 버전이 필요해요.

- 별도의 설치 과정 없이 [**여기**](https://github.com/Usagination/Decchi/releases/latest)에서 다운로드받은 파일을 실행해주면 돼요.

## 사용법

### *Just click 뎃찌!!*

### 빠른 뎃찌!!

- Ctrl + Q 를 눌러 다른 작업 중에도 뎃찌할 수 있어요. (기본 설정)

- 설정에서 단축키를 원하는 키 조합으로 바꿀 수 있어요. 단축키를 한번 클릭한 후 원하는 키 조합을 입력하면 된답니다.

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

- 예) 포맷팅 문자열 : {***/Artist/***의 }{***/Title/***{ (***/Album/***)}을 }듣고 있어요! {***/Via/***} - {***/Client/***}

 |Title|Artist|Album|Client|Output|
|:-:|:-:|:-:|:-:|---|
|夜もすがら君想ふ|Chalili|-|곰오디오|***Chalili***의 ***夜もすがら君想ふ***을 듣고 있어요! ***#뎃찌NP*** - ***곰오디오***|
|[beatmania IIDX 23 copula] STARLIGHT DANCEHALL SPA|-|-|Youtube|***[beatmania IIDX 23 copula] STARLIGHT DANCEHALL SPA***을 듣고 있어요! ***#뎃찌NP*** - ***Youtube***|
|モンタージュガー|ヒトリエ|ルームシック・ガールズエスケープ|Alsong|***ヒトリエ***의 ***モンタージュガール*** (***ルームシック・ガールズエスケープ***)을 듣고 있어요! ***#뎃찌NP*** - ***Alsong***|

## LICENSE

- 뎃찌는 [MIT LICENSE](LICENSE.txt) 를 따라요

- 사용된 오픈소스

 - [MahApps.Metro](Decchi\ExternalLibrarys\Hardcodet.NotifyIcon.Wpf-1.0.5) (입맛에 맞게 수정됨. 소스코드 포함)

 - [Hardcodet.NotifyIcon](Decchi\ExternalLibrarys\Hardcodet.NotifyIcon.Wpf-1.0.5)

 - [TabLib.Portable](Decchi\ExternalLibrarys\TagLib.Portable-1.0.3)
