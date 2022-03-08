import requests

linkPost = "https://oauth.yandex.com/token"
userAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36"
header = {
    "user-agent": userAgent
}

login = str(input("Введите ваш логин от Яндекс Музыки: "))
password = str(input("Введите ваш пароль от Яндекс Музыки: "))

try:
    requestPost = f"grant_type=password&client_id=23cabbbdc6cd418abb4b39c32c41195d&client_secret=53bc75238f0c4d08a118e51fe9203300&username={login}&password={password}"
    requestAuth = requests.post(linkPost, data=requestPost, headers=header)

    print(requestAuth.status_code)

    if requestAuth.status_code == 400:
        print("Error, неправильные данные")
    if requestAuth.status_code == 200:
        jsonData = requestAuth.json()
        print(jsonData.get('access_token'))
except requests.exceptions.ConnectionError:
    print("Проблема с интернетом")