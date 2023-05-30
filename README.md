# ChatDiscordPicker

マイクラ用のDiscordチャットピッカーです。  
Discordのチャットをマイクラ上にオーバーレイとして表示し、メッセージをクリックすることでマイクラのチャットに送信できます。  

![プレビュー](https://github.com/Kamesuta/ChatDiscordPicker/assets/16362824/978cff27-cd73-4bc8-9d73-da544ef5f552)

## ダウンロード

1. [リリースページ](https://github.com/Kamesuta/ChatDiscordPicker/releases) から `ChatDiscordPicker.zip` をダウンロード
2. 展開して、exeを起動すれば使えます。

## セットアップ

1. [Discord StreamKit](https://streamkit.discord.com/overlay) のページを開きます。
2. `Install for OBS` をクリックし、`CHAT WIDGET` タブを選択します。
3. (任意) `BACKGROUND SETTINGS` の `Opacity` を0にします。
4. 右下に表示されているURLをコピーします。
5. 本アプリを起動し、URL欄に貼り付けます。
6. マイクラを開き、「Tキー」を押してチャット欄を開きます。
7. Discordチャンネルのメッセージが表示されていたら成功です。
8. (任意) ウィンドウの縁をドラッグして移動し、マイクラの画面の右上らへんに配置しましょう。サイズは右下のつまみで変更できます。

## 使い方

1. マイクラを開き、「Tキー」を押してチャット欄を開きます。
2. 表示されているDiscordのメッセージをクリックするとチャット欄に自動入力されます。
3. そのままEnterキーを押すと送信されます。

## トラブルシューティング

- Q. `You must install .NET Desktop Runtime to run this application.` と表示される。
    - .NET6.0 が必要です。足りない場合はエラー画面で「はい」を押すと表示されるサイトから .NET をダウンロードしてください。
- Q. 画面が透明じゃありません
    - Discord StreamKit の設定で `Opacity` の値を下げてみてください。
      <img src="https://github.com/Kamesuta/ChatDiscordPicker/assets/16362824/8658a85a-dca4-427f-b405-f780b04f0e23" width="500px">
