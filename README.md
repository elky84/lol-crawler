[![Website](https://img.shields.io/website-up-down-green-red/http/shields.io.svg?label=elky-essay)](https://elky84.github.io)
![Made with](https://img.shields.io/badge/made%20with-.NET6-brightgreen.svg)
![Made with](https://img.shields.io/badge/made%20with-JavaScript-blue.svg)
![Made with](https://img.shields.io/badge/made%20with-MongoDB-red.svg)

![GitHub forks](https://img.shields.io/github/forks/elky84/lol-crawler.svg?style=social&label=Fork)
![GitHub stars](https://img.shields.io/github/stars/elky84/lol-crawler.svg?style=social&label=Stars)
![GitHub watchers](https://img.shields.io/github/watchers/elky84/lol-crawler.svg?style=social&label=Watch)
![GitHub followers](https://img.shields.io/github/followers/elky84.svg?style=social&label=Follow)

![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)
![GitHub repo size in bytes](https://img.shields.io/github/repo-size/elky84/lol-crawler.svg)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/elky84/lol-crawler.svg)

# lol-crawler

## 설명

* C# .NET 5, ASP.NET CORE 3로 개발된 LOL Crawler 입니다.
* 현재는 등록된 유저를 추적해 Discord or Slack으로 WebHook을 날려서 알려주는 기능이 주요 기능입니다.
* DB로는 mongoDB를 사용합니다.
* https://github.com/MingweiSamuel/Camille 프로젝트를 기반으로 많이 활용했습니다.

## 사용법

* Cli 테스트시 `Config.json.sample` -> `Config.json` 으로 변경하시고 `라이엇 API Key`를 입력하셔야 합니다.
* Server 가동시 `appsettings.Development.json`, `appsettings.Production.json` 파일을 sample 파일을 참고해서 생성하시고 RiotApiKey 부분에 `라이엇 API 키`를 입력하셔야 합니다.
* localhost에 mongodb를 구축하지 않으셨다면, `appsetting.[환경].json`에서 `ConnectionString` 부분을 수정하세요.

![lol-crawler](./lol-crawler.png)


## 각종 API 예시
* VS Code의 RestClient Plugin의 .http 파일용으로 작성
  * <https://marketplace.visualstudio.com/items?itemName=humao.rest-client>
* .http 파일 경로
  * <https://github.com/elky84/lol-crawler/tree/master/Http>
* 해당 경로 아래에 .vscode 폴더에 settings.json.sample을 복사해, settings.json으로 변경하면, VSCode로 해당 기능 오픈시 환경에 맞는 URI로 호출 가능하게 됨
  * <https://github.com/elky84/lol-crawler/blob/master/Http/.vscode/settings.json.sample>
* Swagger로 확인해도 무방함
  * <http://localhost:5000/swagger/index.html>