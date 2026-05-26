# Mini Rotina

Mini Rotina e um app infantil de rotina com timers, alarmes, reforco positivo e visual amigavel.

O projeto foi criado para ser simples, leve e facil de manter, sem backend, sem login e sem dependencias desnecessarias.

![Icone do Mini Rotina](windows/TimerInfantilWindows/Assets/MiniRotinaIconGrande.png)

## Plataformas

- Windows nativo com C# + WPF
- Android nativo com Java + XML

## Funcionalidades

- Tela inicial com botoes grandes para atividades infantis.
- Timer com pausa e botao para iniciar.
- Relogio na tela inicial.
- Configuracao de horarios por tarefa.
- Alarme que abre a tarefa no horario configurado.
- Alarme tocando por ate 2 minutos quando uma tarefa agendada dispara.
- 4 modelos de alarme gerados por codigo, sem arquivos de audio com copyright.
- Sistema leve de estrelas com persistencia local.
- Temas rosa e azul.
- Tarefa especial de Brincadeira livre sem cobranca de performance.
- Tarefa Jogos online.

## Atividades

- Escovar os dentes
- Ir para escola
- Arrumar quarto
- Dever de casa
- Leitura
- Dormir
- Brincadeira livre
- Jogos online

## Tecnologias

### Windows

- C#
- WPF
- .NET 10
- Persistencia local em arquivos no AppData
- Sons gerados por codigo com bipes simples do Windows

### Android

- Java
- XML Layouts
- CountDownTimer
- SharedPreferences

## Como rodar no Windows

Com o .NET SDK instalado:

```powershell
dotnet run --project windows\TimerInfantilWindows\TimerInfantilWindows.csproj
```

Para gerar o executavel:

```powershell
dotnet publish windows\TimerInfantilWindows\TimerInfantilWindows.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o Executavel
```

## Como abrir no Android Studio

Abra a pasta raiz do projeto no Android Studio e execute o modulo `app`.

## Objetivo do produto

Ajudar criancas a seguirem pequenas rotinas com leveza, previsibilidade e reforco positivo, sem transformar rotina infantil em pressao ou competicao.
