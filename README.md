[![Website](https://img.shields.io/website-up-down-green-red/http/shields.io.svg?label=elky-essay)](https://elky84.github.io)
<img src="https://img.shields.io/badge/made%20with-JavaScript-brightgreen.svg" alt="made with JavaScript">
<img src="https://img.shields.io/badge/made%20with-.NET 5-red.svg" alt="made with .NET5">

![GitHub forks](https://img.shields.io/github/forks/elky84/lol-crawler.svg?style=social&label=Fork)
![GitHub stars](https://img.shields.io/github/stars/elky84/lol-crawler.svg?style=social&label=Stars)
![GitHub watchers](https://img.shields.io/github/watchers/elky84/lol-crawler.svg?style=social&label=Watch)
![GitHub followers](https://img.shields.io/github/followers/elky84.svg?style=social&label=Follow)

![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)
![GitHub repo size in bytes](https://img.shields.io/github/repo-size/elky84/lol-crawler.svg)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/elky84/lol-crawler.svg)

# lol-crawler

## 설명

C# .NET 5, ASP.NET CORE 3로 개발된 LOL Crawler 입니다.
현재는 등록된 유저를 추적해 Discord or Slack으로 WebHook을 날려서 알려주는 기능이 주요 기능입니다.

https://github.com/MingweiSamuel/Camille 프로젝트를 기반으로 많이 활용했습니다.

## 사용법

Cli 테스트시 `Config.json.sample` -> `Config.json` 으로 변경하시고 `라이엇 API Key`를 입력하셔야 합니다.

Server 가동시 `appsettings.Development.json`, `appsettings.Production.json` 파일을 sample 파일을 참고해서 생성하시고 RiotApiKey 부분에 `라이엇 API 키`를 입력하셔야 합니다.

또한 localhost에 mongodb를 구축하지 않으셨다면, `appsetting.[환경].json`에서 `ConnectionString` 부분을 수정하세요.

![lol-crawler](./lol-crawler.png)