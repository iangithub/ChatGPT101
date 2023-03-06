# ChatGPT101ChatGPT Code Fun Sample Porject 

### GPTLinebot

> 結合 OpenAI API 的 Linebot 範例，僅做為研究性質，程式碼並未做最佳化處理，用於生產環境時，請自行優化，此外串接 Line Platform 的部份，使用自行撰寫之 DotNetLineBotSDK，目前為alpha版本，支援回應文字、影音、圖卡等基本功能，發佈於Nuget https://www.nuget.org/packages/DotNetLineBotSDK

* LinebotController
使用 text-davinci-003 模型，不具備上下文管理

* LinebotController2
使用 text-davinci-003 模型，具備上下文管理，透過注入UserHistoryPrompt靜態物件記錄過往談話內容，做為示範，用於生產環境時，您應該考慮使用額外的儲存方式，例如redis等，此外您應該考慮處理Token數上限問題，text-davinci-003 模型上限是4000 Token數。

text-davinci-003 上下文管理是透過將過去的對話記錄回送，進而使text-davinci-003產生完整內容生成達到回應的效果，例如:

```
ME:句子1
AI:xxxxxxx
ME:句子2
AI:xxxxxxx
ME:句子3
AI:
```
此時下一輪時只要將上述內容做為Prompt送出，text-davinci-003就會補足後續內容。

* LinebotController3
使用 gpt-3.5-turbo 模型，具備上下文管理，透過注入ChatGptRequestModel靜態物件記錄過往談話內容，做為示範，用於生產環境時，您應該考慮使用額外的儲存方式，例如redis等，此外您應該考慮處理Token數上限問題，gpt-3.5-turbo 模型上限是4096 Token數。

gpt-3.5-turbo 本身即做為一個以 chat 為應用的模型，因此比 text-davinci-003 模型更適合用於 chat 的情境，上下文管理與text-davinci-003 相同是透過將過去的對話記錄回送，不過格式並不同，請參考如下（取自OpenAI）:

```
openai.ChatCompletion.create(
  model="gpt-3.5-turbo",
  messages=[
        {"role": "system", "content": "You are a helpful assistant."},
        {"role": "user", "content": "Who won the world series in 2020?"},
        {"role": "assistant", "content": "The Los Angeles Dodgers won the World Series in 2020."},
        {"role": "user", "content": "Where was it played?"}
    ]
)
```

其中 messages 以陣列格式做為 API 參數，並納入 role 與 content 概念，可讓 gpt-3.5-turbo 模型明確識別對話的情境。
