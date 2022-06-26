# Yamux
Описание: Это клиент для Яндекс Музыки в Unix

## Демонстрация
![](https://raw.githubusercontent.com/KirMozor/Yamux/main/Demonstration/MainWindow.png)
![](https://raw.githubusercontent.com/KirMozor/Yamux/main/Demonstration/LoginWindow.png)
![](https://raw.githubusercontent.com/KirMozor/Yamux/main/Demonstration/ImagineDragons.png)

## Сборка на C#
В вашей системе должна находится библиотека Bass (в Yamux она используется для вывода звука). Для установки в ArchLinux используется следующая команда:

```
git clone https://aur.archlinux.org/libbass.git
cd libbass
makepkg -si
```
#### А как установить библиотеку не в ArchLinux, а на Ubuntu например?
А для вас я подготовил скрипт install_lib.sh, запускать вот так:

`chmod +x install_lib.sh && ./install_lib.sh`

### И так, я установил библиотеку, что дальше? 
Теперь вам нужно установить dotnet 6 версии, команду для своего дистрибутива сами делайте (да, опять вам всё самим). Потом после установки, введите следующие команды:

```
git clone https://github.com/KirMozor/Yamux.git
cd Yamux
dotnet build --configuration Release
cp -r Svg bin/Release/net6.0/linux-x64
```
А теперь файл для запуска будет лежать по пути bin/Release/net6.0/linux-64/Yamux. Наслаждайтесь! (после такой инструкции наслаждения мало получишь (⌣̀_⌣́ ) )

### Кстатиии

У меня есть телеграм блог куда я пишу прогресс разработки. Ссылочка внизу, подписывайся!
https://t.me/kirmozor
