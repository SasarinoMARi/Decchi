# [뎃찌](https://github.com/Usagination/Decchi/releases/latest) (Decchi)

뎃찌와 함께 데스크탑에서 #Nowplaying을 게시해요.


## 설치

.NET Framework 4.5 이상 버전이 필요해요.
별도의 설치 과정 없이 [여기](https://github.com/Usagination/Decchi/releases/latest)에서 다운로드받은 파일을 실행해주면 돼요.

## 사용법

just click 뎃찌!

### 단축키

Ctrl + Q를 눌러 다른 작업 중에도 뎃찌를 실행할 수 있어요.

### 포맷팅

뎃찌는 (자칭)강력한 포맷팅 기능을 지원해요.

포맷팅이란 곡의 제목, 아티스트 이름 등을 이용해 게시를 위한 완전한 문자열을 만드는 작업입니다.

뎃찌와 약속한 몇 개의 문자열들을 사용해 손쉽게 포맷팅을 정의해보세요!

아래는 뎃찌의 약속어 목록이에요.

|약속어|치환|
|---|---|
|`Title`|곡의 제목|
|`Artist`|아티스트 이름|
|`Album`|앨범 이름|
|`Client`|재생중인 클라이언트|
|`Via`|뎃찌의 홍보문구에요 ㅇ.<|

약속어는 '{'와 '}' 안에 '/'로 감싸서 써주셔야 해요.

중괄호 안, 약속어 앞 뒤에 쓰인 문자열은 치환 값이 비어있을 경우 포맷팅에서 제외되니 유용하게 사용할 수 있어요.

예) 포맷팅 문자열 : <b>{/Artist/의 }{/Title/{(/Album/)}을 }듣고 있어요! {/Via/} - {/Client/} </b>

|Title|Artist|Album|Client|Output|
|---|---|---|---|---|
|夜もすがら君想ふ|Chalili|-|곰오디오|Chalili의 夜もすがら君想ふ을 듣고 있어요! #뎃찌NP - 곰오디오|
|[beatmania IIDX 23 copula] STARLIGHT DANCEHALL SPA|-|-|Youtube|[beatmania IIDX 23 copula] STARLIGHT DANCEHALL SPA을 듣고 있어요! #뎃찌NP - Youtube|
|モンタージュガー|ヒトリエ|ルームシック・ガールズエスケープ|Alsong|ヒトリエ의 モンタージュガール(ルームシック・ガールズエスケープ)을 듣고 있어요! #뎃찌NP - Alsong|

## 지원 클라이언트

- Windows Media Player
- 곰오디오
- iTunes
- 알송
- 웹 브라우저에서 보는 유튜브
- Melon