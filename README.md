# AupRepack
This project is forked from [AupRename](https://github.com/karoterra/AupRename)

AviUtl 拡張編集で使った動画・画像・音声等のファイルを同一フォルダに一括コピー & AUPファイルのパスを書き換え

## インストール

[Releases](https://github.com/takunnma5286/AupRepack/releases)
から最新版の ZIP ファイルをダウンロードし、好きな場所に展開してください。

- `AupRepack-<バージョン>-win-x64-fd.zip`
    - .NET 8 ランタイムをインストール済みの場合
- `AupRepack-<バージョン>-win-x64-sc.zip`
    - .NET 8 ランタイムをインストールせずに使う場合
    - よく分からないがとにかく使いたい場合

アンインストール時には展開したフォルダを削除してください。

### .NET 8 ランタイム

.NET 8 ランタイムがインストールされているか確認するにはコマンドプロンプトを起動して以下のコマンドを実行してください。

```cmd
dotnet --list-runtimes
```

実行後、画面に `Microsoft.WindowsDesktop.App` と表示されている行の中に `8.0.0` 以降の数字があれば .NET 8 ランタイムがインストールされています。
例えば `Microsoft.WindowsDesktop.App 8.0.12` と表示されていれば問題ありません。

.NET 8 ランタイムをインストールする場合には以下のページからデスクトップアプリ用のインストーラーをダウンロードしてインストールしてください。

[.NET 8.0 ランタイムのダウンロード](https://dotnet.microsoft.com/ja-jp/download/dotnet/8.0/runtime)

## 使い方

1. `AupRepack.exe` を起動してください。
2. 再梱包したい aup ファイルをウィンドウにドラッグアンドドロップしてください。
3. 「再梱包」をクリックするとフォルダ選択ダイアログが開きます、ファイルのコピー先フォルダを選択してください

## 更新履歴

更新履歴は [CHANGELOG](CHANGELOG.md) を参照してください。

## ライセンス

このソフトウェアは MIT ライセンスのもとで公開されます。
詳細は [LICENSE](LICENSE) を参照してください。

使用したライブラリ等については [CREDITS](CREDITS.md) を参照してください。
