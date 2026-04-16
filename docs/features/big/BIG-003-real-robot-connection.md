# BIG-003 — Conexão com Robô Físico

> **Categoria:** Alto Impacto  
> **Status:** Proposta  
> **Prioridade:** Alta  
> **Estimativa:** 15–25 dias  
> **Responsável:** —  

---

## Resumo

Permitir que o programa gerado pelo editor de blocos seja enviado e executado em um robô físico real (compatível com o protocolo Br-GoGo ou similar), fechando o ciclo simulação → mundo real.

---

## Motivação / Problema

Atualmente, o editor é 100% simulado. O ciclo completo de aprendizagem em robótica educacional prevê que o aluno veja seu programa funcionando em hardware real — isso aumenta drasticamente o engajamento, o senso de conquista e a aplicabilidade do aprendizado. Sem integração com robô físico, o projeto fica restrito ao ambiente de simulação.

---

## Solução Proposta

### Fase 1 — Conexão Serial (USB)
1. Detectar portas seriais disponíveis via `System.IO.Ports.SerialPort` (disponível em Mono/.NET).
2. Painel no Robot Simulation Editor para selecionar porta e baudrate.
3. Ao clicar "Enviar para Robô", serializar o programa gerado e transmitir via protocolo Br-GoGo.
4. Exibir log de comunicação em tempo real.

### Fase 2 — Conexão Bluetooth (opcional)
1. Usar plugin Unity para Bluetooth clássico (ex: `BluetoothLE` para WebGL/mobile ou serial emulado).
2. Interface de pareamento de dispositivos.

### Protocolo de Comunicação (Br-GoGo / compatível)
O `BoardController` já processa strings de comando como `"thisway"`, `"setpower 50"`, `"ledon 0"`. O protocolo de envio encapsularia esses mesmos comandos no formato aceito pelo hardware alvo.

---

## Critérios de Aceite

- [ ] Lista de portas seriais disponíveis exibida no painel
- [ ] Conexão e desconexão com robô via porta serial
- [ ] Programa compilado é enviado e executado no robô físico
- [ ] Log de comunicação mostra comandos enviados e confirmações recebidas
- [ ] Tratamento de erro em caso de desconexão durante execução
- [ ] Documentação de hardware compatível no README

---

## Escopo

### Inclui
- Conexão via porta serial USB
- Envio do programa gerado pelo `GetCode()` pipeline
- Log de comunicação no Robot Simulation Editor

### Não Inclui (Out of Scope)
- Conexão Bluetooth (Fase 2 separada)
- Debug remoto (ler estado do robô físico de volta ao editor)
- Suporte a múltiplos robôs simultaneamente

---

## Hardware de Referência

| Hardware | Protocolo | Observação |
|----------|-----------|------------|
| Br-GoGo Board | Serial / USB | Referência original do projeto |
| Lego EV3 (via ev3dev) | Serial + SSH | Compatível com adaptação |
| Arduino custom | Serial | Requer firmware personalizado |

---

## Impacto Técnico

| Área | Impacto | Observação |
|------|---------|------------|
| Novo script: `SerialRobotConnector.cs` | Alto | Gerencia `SerialPort` e protocolo |
| `Assets/Editor/PythonEditorWindow.cs` | Alto | Painel de conexão e envio |
| `Assets/Scripts/Robot/BoardController.cs` | Médio | Modo "real" vs "simulado" |
| Novo painel no Robot Simulation Editor | Alto | UI de conexão/log |

---

## Dependências

- Sistema de build deve incluir `System.IO.Ports` (requer configuração no `.asmdef` e no Player Settings)
- MED-001 (Salvar/Carregar) — para ter o programa compilado disponível

---

## Riscos

| Risco | Probabilidade | Severidade | Mitigação |
|-------|--------------|------------|-----------|
| `System.IO.Ports` não disponível em todas as plataformas de build Unity | Alta | Alta | Restringir feature a builds Windows/macOS/Linux; desabilitar em WebGL |
| Protocolo Br-GoGo não documentado publicamente | Alta | Alta | Engenharia reversa a partir do código existente + contato com mantenedores |
| Latência serial causa dessincronização entre comandos | Média | Média | Buffer de comandos + ACK por comando antes do próximo |

---

## Notas Adicionais

- Referência de protocolo: [Br-GoGo](https://br-gogo.sourceforge.net)
- `System.IO.Ports` no Unity: requer `Api Compatibility Level: .NET 4.x` ou `.NET Standard 2.1` no Player Settings
