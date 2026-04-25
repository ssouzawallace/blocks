# 🗺️ Roadmap — Blocks Programming

> Projeto: **Unity3D Blocks Programming**  
> Plataforma: Unity 6.2 (6000.2.10f1) · IronPython 2.7 · C#  
> Público-alvo: Estudantes de robótica educacional  
> Referência: [Br-GoGo](https://br-gogo.sourceforge.net) / Scratch  

---

## Visão Geral

O **Blocks** é um editor visual de programação para robôs educacionais, inspirado no Scratch.  
O usuário monta programas arrastando blocos e o sistema gera código Python/Logo que controla o robô simulado.

Este roadmap organiza as features planejadas por **nível de esforço e impacto**, do menor ao maior:

```
Quick Wins → Features Médias → Alto Impacto / Grandes Features
```

---

## 📊 Status dos Itens

| ID | Feature | Categoria | Prioridade | Status |
|----|---------|-----------|------------|--------|
| [QW-001](features/quick-wins/QW-001-undo-redo.md) | Desfazer / Refazer (Undo/Redo) | Quick Win | Alta | Proposta |
| [QW-002](features/quick-wins/QW-002-block-search.md) | Busca e Filtro de Blocos na Paleta | Quick Win | Média | Proposta |
| [QW-003](features/quick-wins/QW-003-canvas-zoom.md) | Zoom no Canvas de Programação | Quick Win | Média | Proposta |
| [QW-004](features/quick-wins/QW-004-keyboard-shortcuts.md) | Atalhos de Teclado | Quick Win | Baixa | Proposta |
| [MED-001](features/medium/MED-001-save-load.md) | Salvar e Carregar Programas | Médio | Alta | Proposta |
| [MED-002](features/medium/MED-002-variable-blocks.md) | Blocos de Variáveis | Médio | Alta | Proposta |
| [MED-003](features/medium/MED-003-repeat-n-block.md) | Bloco "Repetir N vezes" | Médio | Média | Proposta |
| [MED-004](features/medium/MED-004-sensor-condition-blocks.md) | Blocos de Condição para Sensores | Médio | Alta | Proposta |
| [BIG-001](features/big/BIG-001-new-scenarios.md) | Novos Cenários de Simulação | Alto Impacto | Alta | Proposta |
| [BIG-002](features/big/BIG-002-export-share.md) | Exportar e Compartilhar Programas | Alto Impacto | Média | Proposta |
| [BIG-003](features/big/BIG-003-real-robot-connection.md) | Conexão com Robô Físico | Alto Impacto | Alta | Proposta |
| [BIG-004](features/big/BIG-004-guided-tutorials.md) | Sistema de Tutoriais Guiados | Alto Impacto | Média | Proposta |

---

## ⚡ Quick Wins

Features de baixo esforço com retorno rápido. Ideais para primeiras sprints ou contribuidores novos.

### QW-001 — Desfazer / Refazer (Undo/Redo)
**Problema:** Ao errar uma ação (remover um bloco, reorganizar), o usuário não tem como desfazê-la.  
**Solução:** Implementar pilha de comandos (Command Pattern) para operações de arrastar, conectar e deletar blocos.  
**Impacto:** Reduz frustração e melhora drasticamente a experiência de uso.  
→ [Ver especificação completa](features/quick-wins/QW-001-undo-redo.md)

---

### QW-002 — Busca e Filtro na Paleta de Blocos
**Problema:** Com muitos blocos disponíveis, é difícil encontrar o bloco certo rapidamente.  
**Solução:** Campo de busca no topo da paleta filtrando blocos por nome/categoria em tempo real.  
**Impacto:** Melhora a produtividade e reduz tempo de aprendizado da interface.  
→ [Ver especificação completa](features/quick-wins/QW-002-block-search.md)

---

### QW-003 — Zoom no Canvas de Programação
**Problema:** Programas maiores ficam difíceis de visualizar no tamanho padrão.  
**Solução:** Scroll do mouse ou botões +/- para aumentar/diminuir o zoom do canvas.  
**Impacto:** Permite trabalhar com programas mais complexos sem perda de visibilidade.  
→ [Ver especificação completa](features/quick-wins/QW-003-canvas-zoom.md)

---

### QW-004 — Atalhos de Teclado
**Problema:** Ações frequentes (executar programa, limpar canvas, desfazer) exigem cliques.  
**Solução:** Mapeamento de teclas para as ações mais comuns (ex: `F5` = rodar, `Ctrl+Z` = desfazer, `Delete` = remover bloco selecionado).  
**Impacto:** Fluxo de trabalho mais ágil para usuários avançados.  
→ [Ver especificação completa](features/quick-wins/QW-004-keyboard-shortcuts.md)

---

## 🔧 Features Médias

Esforço moderado (dias a semanas). Adicionam capacidade real ao sistema de programação.

### MED-001 — Salvar e Carregar Programas
**Problema:** O programa atual é perdido ao fechar o editor ou reiniciar a cena.  
**Solução:** Serialização do grafo de blocos em JSON e persistência local (`Application.persistentDataPath`).  
**Impacto:** Fundamental para uso educacional — alunos precisam continuar de onde pararam.  
→ [Ver especificação completa](features/medium/MED-001-save-load.md)

---

### MED-002 — Blocos de Variáveis
**Problema:** Não é possível armazenar valores intermediários (contadores, estados, resultados de sensores).  
**Solução:** Blocos `Definir Variável`, `Ler Variável` e `Alterar Variável` com suporte a tipos numéricos.  
**Impacto:** Desbloqueia algoritmos mais sofisticados; alinha o sistema com o que alunos aprendem em sala.  
→ [Ver especificação completa](features/medium/MED-002-variable-blocks.md)

---

### MED-003 — Bloco "Repetir N Vezes"
**Problema:** Para executar uma ação um número fixo de vezes, o usuário precisa usar `while` com contador manual.  
**Solução:** Novo bloco `Repetir [N] vezes` com campo numérico embutido, gerando `for i in range(N):`.  
**Impacto:** Simplifica programas comuns; reduz a barreira para iniciantes.  
→ [Ver especificação completa](features/medium/MED-003-repeat-n-block.md)

---

### MED-004 — Blocos de Condição para Sensores
**Problema:** Não há blocos prontos que leem sensores (ultrassônico, cor) dentro de condições (`if`/`while`).  
**Solução:** Blocos do tipo `Number` ou `Logic` que retornam leituras dos sensores — ex: `Distância Frontal`, `Cor Detectada`, `Nível de Luz`.  
**Impacto:** Permite programas reativos ao ambiente; essencial para desafios de robótica.  
→ [Ver especificação completa](features/medium/MED-004-sensor-condition-blocks.md)

---

## 🚀 Alto Impacto / Grandes Features

Alta complexidade; transformam o produto e ampliam o público.

### BIG-001 — Novos Cenários de Simulação
**Problema:** O simulador tem cenários limitados, restringindo os desafios que professores podem propor.  
**Solução:** Biblioteca de novos cenários: labirinto, linha curva, classificação por cores, corrida com obstáculos.  
**Impacto:** Aumenta o valor educacional e a vida útil do produto.  
→ [Ver especificação completa](features/big/BIG-001-new-scenarios.md)

---

### BIG-002 — Exportar e Compartilhar Programas
**Problema:** Não há como compartilhar programas entre alunos ou com o professor.  
**Solução:** Exportar o programa como arquivo `.blocks` (JSON) e importar de arquivo; opcionalmente gerar código Python legível para exportação.  
**Impacto:** Viabiliza atividades colaborativas e avaliações formais.  
→ [Ver especificação completa](features/big/BIG-002-export-share.md)

---

### BIG-003 — Conexão com Robô Físico
**Problema:** O ambiente é puramente simulado; não há integração com hardware real.  
**Solução:** Envio dos comandos gerados para um robô físico via porta serial ou Bluetooth (protocolo Br-GoGo / similar).  
**Impacto:** Fecha o ciclo simulação → mundo real; aumenta engajamento e aplicabilidade do projeto.  
→ [Ver especificação completa](features/big/BIG-003-real-robot-connection.md)

---

### BIG-004 — Sistema de Tutoriais Guiados
**Problema:** Novos usuários não sabem por onde começar; não há onboarding estruturado.  
**Solução:** Sistema de missões/tutoriais passo-a-passo incorporados ao editor, com dicas contextuais e validação automática.  
**Impacto:** Reduz churn de novos usuários; torna o produto autossuficiente para uso em sala de aula sem instrutor.  
→ [Ver especificação completa](features/big/BIG-004-guided-tutorials.md)

---

## 📅 Horizonte Sugerido de Execução

```
Sprint 1 (Semanas 1-2)   → QW-001, QW-002
Sprint 2 (Semanas 3-4)   → QW-003, QW-004, MED-003
Sprint 3 (Semanas 5-7)   → MED-001
Sprint 4 (Semanas 8-10)  → MED-002, MED-004
Sprint 5 (Semanas 11-14) → BIG-001
Sprint 6 (Semanas 15-18) → BIG-002, BIG-004
Sprint 7 (Semanas 19+)   → BIG-003
```

---

## 🛠️ Convenções para Contribuidores

- Cada feature tem seu arquivo em `docs/features/<categoria>/<ID>-<slug>.md`
- Use o template em [`docs/FEATURE_TEMPLATE.md`](FEATURE_TEMPLATE.md) para novas features
- IDs seguem o padrão `QW-XXX`, `MED-XXX`, `BIG-XXX`
- Ao iniciar o desenvolvimento, atualize o `Status` para `Em Desenvolvimento` e atribua um `Responsável`
- Blocos novos devem ter testes em `Assets/Tests/EditMode/` (ver convenção de testes do projeto)

---

## 📚 Referências

- [README.md](../README.md) — Arquitetura e guia de contribuição
- [IronPython 2.7](https://ironpython.net/)
- [Br-GoGo](https://br-gogo.sourceforge.net) — Referência do protocolo de robô
- [Scratch](https://scratch.mit.edu/) — Inspiração de UX para editores de blocos
