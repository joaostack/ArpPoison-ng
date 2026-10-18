# ArpPoison-ng

Uma ferramenta de **ARP Poisoning (ARP Spoofing)** reescrita em C#/.NET 10 para fins educacionais e testes de segurança em redes próprias.

> ⚠️ **Use apenas em redes suas ou com autorização explícita.** O uso indevido pode ser ilegal e antiético.

---

## Como funciona (resumo)

```
[Vítima] ←── ARP mentiroso ──▶ [Você/Atacante] ←── ARP mentiroso ──▶ [Gateway]
```

1. Você envia pacotes ARP falsos para o **target** dizendo: *"O IP do gateway está no MEU MAC"*
2. Você envia pacotes ARP falsos para o **gateway** dizendo: *"O IP do target está no MEU MAC"*
3. Ambos passam a mandar o tráfego para você → você pode ler/modificar/encaminhar
4. Ao parar (Ctrl+C), a ferramenta **restaura** as tabelas ARP originais

---

## Instalação / Build

```bash
# Pré-requisitos: .NET 10 SDK
dotnet build ArpPoison-ng/ArpPoison-ng.csproj
```

**Dependências principais** (já no `.csproj`):
- **SharpPcap** + **PacketDotNet** — captura e injeção de pacotes
- **Spectre.Console** + **Spectre.Console.Cli** — CLI bonita e parsing de argumentos
- **Microsoft.Extensions.DependencyInjection** — injeção de dependência

---

## Uso

```bash
dotnet run --project ArpPoison-ng -i <interface> -t <ip-da-vítima>
```

**Exemplo:**
```bash
dotnet run --project ArpPoison-ng -i "eth0" -t "192.168.1.50"
```

### Parâmetros obrigatórios

| Flag | Nome | Descrição |
|------|------|-----------|
| `-i`, `--interface` | Interface de rede | Nome amigável da interface (ex: `eth0`, `Wi-Fi`, `Ethernet`) |
| `-t`, `--target` | IP alvo | IP da vítima na mesma rede local |

---

## Estrutura do Projeto

```
ArpPoison-ng/
├── Program.cs                    # Entry point + DI container
├── Commands/
│   └── StartCommand.cs           # Lógica principal do comando "start"
├── DI/
│   └── TypeRegistrar.cs          # Bridge Spectre.Console ↔ MS DI
├── Interfaces/                   # Contratos (testáveis, desacoplados)
│   ├── IArpPacketBuilder.cs      # Monta pacotes ARP/Ethernet
│   ├── IArpPoison.cs             # Spoof + Restore
│   ├── IConsoleUI.cs             # Saída no terminal (Spectre)
│   ├── INetworkDeviceProvider.cs # Dispositivos, gateway, MACs
│   └── IPacketBuild.cs           # Resolve IP → MAC via ARP request
└── Services/                     # Implementações
    ├── ArpPacketBuilder.cs       # Usa PacketDotNet
    ├── ArpPoison.cs              # Envia os pacotes forjados
    ├── ConsoleUI.cs              # Figlet banner + cores
    ├── NetworkDeviceProvider.cs  # SharpPcap + System.Net.NetworkInformation
    └── PacketBuild.cs            # ARP request + captura response
```

---

## Fluxo de Execução

1. **Banner** bonito no terminal 
2. Abre a interface em **modo promíscuo**
3. Descobre o **IP do gateway** automaticamente
4. Faz **ARP Request** para resolver:
   - MAC do target
   - MAC do gateway
5. Entra no **loop de spoofing** (intervalo de 1 segundo):
   - Envia ARP Reply forjado para o target
   - Envia ARP Reply forjado para o gateway
6. **Ctrl+C** → para o loop
7. Envia pacotes de **restore** (ARP legítimos) para limpar a bagunça
8. Sai limpo 

---

## Classes-chave (para desenvolvedores)

| Classe | Responsabilidade |
|--------|------------------|
| `StartCommand` | Orquestra tudo: abre device, resolve MACs, loop, cleanup |
| `ArpPoison` | Monta e envia pacotes ARP forjados (spoof/restore) |
| `ArpPacketBuilder` | Cria `EthernetPacket` + `ArpPacket` usando PacketDotNet |
| `PacketBuild` | Envia ARP Request e espera ARP Reply para descobrir MAC |
| `NetworkDeviceProvider` | Lista devices, abre by name, acha gateway, formata MAC |
| `ConsoleUI` | Wrapper para Spectre.Console (banner, cores, markup) |

---

## Dicas e Gotchas

- **Precisa de root/admin** — captura/injeção de pacotes raw socket
- **Interface pelo "Friendly Name"** — no Linux costuma ser `eth0`, `wlan0`; no Windows nome da conexão (ex: `Wi-Fi`)
- **Só funciona em IPv4** (código filtra `AddressFamily.InterNetwork`)
- **Target precisa estar online** — se não responder ARP, falha
- **Restauração automática** — o `finally` garante que as tabelas ARP voltam ao normal mesmo se der erro

---

## Licença
MIT
