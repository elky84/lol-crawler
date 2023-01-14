[![Website](https://img.shields.io/website-up-down-green-red/http/shields.io.svg?label=elky-essay)](https://elky84.github.io)
![Made with](https://img.shields.io/badge/made%20with-.NET6-brightgreen.svg)
![Made with](https://img.shields.io/badge/made%20with-JavaScript-blue.svg)
![Made with](https://img.shields.io/badge/made%20with-MongoDB-red.svg)

[![Publish Docker image](https://github.com/elky84/lol-crawler/actions/workflows/publish_docker.yml/badge.svg)](https://github.com/elky84/lol-crawler/actions/workflows/publish_docker.yml)

![GitHub forks](https://img.shields.io/github/forks/elky84/lol-crawler.svg?style=social&label=Fork)
![GitHub stars](https://img.shields.io/github/stars/elky84/lol-crawler.svg?style=social&label=Stars)
![GitHub watchers](https://img.shields.io/github/watchers/elky84/lol-crawler.svg?style=social&label=Watch)
![GitHub followers](https://img.shields.io/github/followers/elky84.svg?style=social&label=Follow)

![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)
![GitHub repo size in bytes](https://img.shields.io/github/repo-size/elky84/lol-crawler.svg)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/elky84/lol-crawler.svg)

# lol-crawler

## 설명

* C# .NET 6, ASP.NET CORE 6로 개발된 LOL Crawler 입니다.
* 현재는 등록된 유저를 추적해 Discord or Slack으로 WebHook을 날려서 알려주는 기능이 주요 기능입니다.
* DB로는 mongoDB를 사용합니다.
* Riot LOL 전적 조회 기능으로는 <https://github.com/MingweiSamuel/Camille> 프로젝트를 많이 활용했습니다.

## 사용법

* Cli 프로젝트, Server 프로젝트
  * `RIOT_API_KEY` 환경 변수에 `라이엇 API 키 입력`
* MongoDB 설정 (Server 프로젝트)
  * `MONGODB_CONNECTION` 환경 변수에 `MONGODB 커넥션 문자열` 입력
* 선택적 MongoDB 데이터베이스
  * 기본 값은 `lol-crawler`
  * `MONGODB_DATABASE` 환경 변수 사용시 override
* 환경 변수 미사용시, appSettings.[환경].json 파일에 있는 값을 사용합니다. (환경에 맞는 파일 미제공시, appSettings.json 의 값을 그대로 이용)

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