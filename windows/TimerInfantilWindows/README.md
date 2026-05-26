# Timer Infantil Windows

Versao nativa para Windows criada com C# + WPF.

## Como executar

1. Instale o .NET 10 SDK com suporte a apps desktop Windows.
2. Abra esta pasta no Visual Studio.
3. Execute o projeto `TimerInfantilWindows`.

Pelo terminal, quando o SDK estiver instalado:

```powershell
dotnet run --project windows\TimerInfantilWindows\TimerInfantilWindows.csproj
```

## Como gerar um .exe

```powershell
dotnet publish windows\TimerInfantilWindows\TimerInfantilWindows.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o Executavel
```

## O que ja existe

- Tela inicial com botoes grandes.
- Timer com pausa e continuar.
- Progresso visual.
- Som nativo suave ao finalizar.
- Estrelas salvas localmente.
- Feedback especial para Brincadeira livre, sem cobranca ou competicao.
