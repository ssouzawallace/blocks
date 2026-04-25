# QW-001 — Desfazer / Refazer (Undo/Redo)

> **Categoria:** Quick Win  
> **Status:** Proposta  
> **Prioridade:** Alta  
> **Estimativa:** 3–5 dias  
> **Responsável:** —  

---

## Resumo

Permitir que o usuário desfaça e refaça ações de edição no canvas de programação (arrastar blocos, conectar, desconectar, deletar).

---

## Motivação / Problema

Atualmente, qualquer erro ao montar o programa — apagar um bloco errado, soltar um bloco no lugar errado — é irreversível. Isso gera frustração, especialmente em usuários iniciantes que ainda estão aprendendo a usar o editor.

---

## Solução Proposta

Implementar o **Command Pattern**:

1. Cada ação do usuário (mover, conectar, desconectar, criar, deletar bloco) é encapsulada em um objeto `ICommand` com métodos `Execute()` e `Undo()`.
2. Um `UndoRedoManager` (singleton ou componente de cena) mantém duas pilhas: `undoStack` e `redoStack`.
3. `Ctrl+Z` chama `UndoRedoManager.Undo()` e `Ctrl+Y` / `Ctrl+Shift+Z` chama `Redo()`.

---

## Critérios de Aceite

- [ ] `Ctrl+Z` desfaz a última ação de edição de blocos
- [ ] `Ctrl+Y` (ou `Ctrl+Shift+Z`) refaz a ação desfeita
- [ ] Após desfazer 5 ações consecutivas, o estado do canvas é idêntico ao estado anterior às 5 ações
- [ ] Qualquer nova ação após um undo limpa a `redoStack`
- [ ] Botões de UI (se adicionados) refletem o estado das pilhas (desabilitados quando vazias)
- [ ] Ações que ocorrem durante Play Mode não entram na pilha de undo

---

## Escopo

### Inclui
- Undo/Redo para: mover bloco, conectar dois blocos, desconectar, deletar bloco (arrastar para fora do canvas)
- Atalhos de teclado `Ctrl+Z` e `Ctrl+Y`

### Não Inclui (Out of Scope)
- Persistência do histórico de undo entre sessões
- Undo durante execução do programa (Play Mode)
- Histórico ilimitado (limite sugerido: 50 ações)

---

## Impacto Técnico

| Área | Impacto | Observação |
|------|---------|------------|
| `Assets/Scripts/Programming/Blocks/Block.cs` | Alto | Ações de drag/drop precisam registrar comandos |
| Novo arquivo: `UndoRedoManager.cs` | Alto | Novo componente de gerenciamento |
| Novo arquivo: `IBlockCommand.cs` | Alto | Interface do padrão Command |
| `Assets/Tests/EditMode/` | Médio | Testes de undo/redo de operações básicas |

---

## Dependências

Nenhuma dependência externa.

---

## Riscos

| Risco | Probabilidade | Severidade | Mitigação |
|-------|--------------|------------|-----------|
| Conexões bidirecionals dificultam reversão correta | Média | Alta | Snapshot completo dos estados de conexão antes/depois |
| Consumo de memória com histórico grande | Baixa | Baixa | Limitar pilha a 50 entradas |

---

## Notas Adicionais

- O Unity não possui sistema de undo nativo disponível em runtime (somente no Editor). A implementação deve ser feita 100% em código de jogo.
- Referência de padrão: [Game Programming Patterns — Command](https://gameprogrammingpatterns.com/command.html)
