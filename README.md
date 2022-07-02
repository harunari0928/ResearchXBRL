<div id="top"></div>

[![Build Status](https://dev.azure.com/nanteneus/%E6%A9%9F%E6%A2%B0%E5%AD%A6%E7%BF%92%E5%8B%89%E5%BC%B7%E4%BC%9A/_apis/build/status/Seibi0928.ResearchXBRL?branchName=main)](https://dev.azure.com/nanteneus/%E6%A9%9F%E6%A2%B0%E5%AD%A6%E7%BF%92%E5%8B%89%E5%BC%B7%E4%BC%9A/_build/latest?definitionId=9&branchName=main)

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h3 align="center">EDINETのXBRL情報で勘定項目を分析するためのツール(仮)</h3>
</div>

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
  </ol>
</details>

<!-- ABOUT THE PROJECT -->
## About The Project

EDINETのXBRL情報で勘定項目を分析するためのツール
バックエンドプロジェクト

<p align="right">(<a href="#top">back to top</a>)</p>

### Built With

* [Docker](https://www.docker.com/)
* [.NET6](https://docs.microsoft.com/ja-jp/dotnet/core/whats-new/dotnet-6)
* [C#](https://docs.microsoft.com/en-us/dotnet/csharp/)
* [PostgreSQL](https://www.postgresql.org/)

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- GETTING STARTED -->
## Getting Started

### Prerequisites

簡単に環境構築するために以下3つの導入をお願いします

* [Docker](https://docs.docker.com/get-started/)
* [VSCode](https://code.visualstudio.com/)
* [RemoteContainers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)(VSCode拡張)
  * DevContainer利用する場合に必要    

### Installation

1. リポジトリのクローン

   ```sh
   git clone https://github.com/Seibi0928/ResearchXBRL.git
   ```

2. コンテナ環境構築
  * Devcontainerを使う場合
  
    * VSCodeを開く

      ```sh
      code ./ResearchXBRL
      ```

    * F1を押下しコマンドパレットを開く
    * `Reopen in Container`と入力し選択

  * DevContainerを使わない場合

   ```sh
   cd ResearchXBRL.Frontend
   docker-compose -f ./devcontainer/docker-compose.yml up
   ```
3. [Usage](https://github.com/Seibi0928/ResearchXBRL/blob/main/README.md#usage)のアプリケーションを上から順に起動する

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- USAGE EXAMPLES -->
## Usage

### 有価証券報告書インポートバッチ

* EDINETから有価証券報告書をダウンロードしDBへインポートするバッチ

* 実行方法

```sh
dotnet run --project /src/Presentation/AquireFinancialReports/AquireFinancialReports.csproj
```

* 任意で実行時引数を指定できます
  * --from
    * 取得する書類の提出日下限を指定します
    * 指定しない場合は24時間前の日時が指定されます
    * 5年以上前の日時を指定した場合エラーになります EDINETは直近5年のデータしか返さないため

  * --to
    * 取得する書類の提出日上限を指定します
    * 指定しない場合は現時刻が指定されます
    * 5年以上前の日時を指定した場合エラーになります EDINETは直近5年のデータしか返さないため

  * --max-parallelism
    * インポート処理の並列数を指定します
    * 指定しない場合は1が指定されます

* 引数を使用したコマンドの一例

```sh
dotnet run --project /src/Presentation/AquireFinancialReports/AquireFinancialReports.csproj --from 2021-01-01 --to 2021-12-01 --max-parallelism 2
```

### 企業情報インポートバッチ

* 企業情報をDBへインポートするバッチ
  * インポートするデータは`/src/Presentation/ImportCorporationInfo/EdinetcodeDlInfo.csv`
  * 上記データはタクソノミに含まれることが分かったので`勘定項目インポートバッチ`バッチと統合予定

* 実行方法

```sh
dotnet run --project /src/Presentation/ImportCorporationInfo/ImportCorporationInfo.csproj
```

### 勘定項目インポートバッチ

* 勘定項目をダウンロードしDBへインポートするバッチ
  * 勘定項目はダウンロードしたタクソノミから抽出する
  
* 実行方法

```sh
dotnet run --project /src/Presentation/AquireAccountItems/AquireAccountItems.csproj
```

### 勘定項目CSV出力バッチ

* 入力したタクソノミデータから勘定項目を抽出し、勘定項目とXBRL特有の要素名の対応表csvを出力する
* バッチを起動するとコンソールが立ち上がる
* コンソールからスキーマファイルとラベルファイルのパスを指定する

```sh
dotnet run --project /src/Presentation/CreateAccountItemsCSV/CreateAccountItemsCSV.csproj
```

### 財務分析API

* インポートしたデータをフロントエンドへ連携するためのAPI

* 実行方法

```sh
dotnet run --project /src/Presentation/FinancialAnalysisAPI/FinancialAnalysisAPI.csproj
```

* APIエンドポイント仕様(OpenAPI)

```http
http://localhost:45613/swagger/v1/swagger.yaml
```

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- CONTRIBUTING -->
## Contributing

歓迎

1. プロジェクトをフォーク
2. featureブランチ作成 (`git checkout -b feature/AmazingFeature`)
3. 変更をコミット (`git commit -m 'Add some AmazingFeature'`)
4. ブランチをプッシュ (`git push origin feature/AmazingFeature`)
5. developブランチへプルリク

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- CONTACT -->
## Contact

ryo13579978@gmail.com

<p align="right">(<a href="#top">back to top</a>)</p>
