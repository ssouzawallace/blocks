# QW-004 — Atalhos de Teclado

> **Categoria:** Quick Win  
> **Status:** Proposta  
> **Prioridade:** Baixa  
> **Estimativa:** 1–2 dias  
> **Responsável:** —  

---

## Resumo

Mapear ações frequentes do editor (executar programa, limpar canvas, desfazer, deletar bloco selecionado) a atalhos de teclado padronizados.

---

## Motivação / Problema

Usuários mais avançados precisam clicar em botões para cada ação, tornando o fluxo lento. Atalhos de teclado reduzem a fricção e são uma expectativa padrão em editores de qualquer tipo.

---

## Solução Proposta

Criar um `KeyboardShortcutManager` (componente na cena principal) que escuta `Input.GetKeyDown` e dispara as ações correspondentes:

| Atalho | Ação |
|--------|------|
| `F5` | Executar o programa |
| `Ctrl+Z` | Desfazer (depende de QW-001) |
| `Ctrl+Y` / `Ctrl+Shift+Z` | Refazer (depende de QW-001) |
| `Delete` / `Backspace` | Deletar bloco selecionado |
| `Ctrl+A` | Selecionar todos os blocos |
| `Escape` | Deselecionar / cancelar ação atual |
| `Ctrl+S` | Salvar programa (depende de MED-001) |

---

## Critérios de Aceite

- [ ] `F5` inicia a execução do programa gerado
- [ ] `Delete` remove o bloco que está em foco/selecionado
- [ ] `Ctrl+Z` e `Ctrl+Y` funcionam (quando QW-001 estiver implementado)
- [ ] Atalhos não disparam quando o foco está em um campo de texto (InputField)
- [ ] Documentação de atalhos acessível via `?` ou menu Ajuda

---

## Escopo

### Inclui
- Script gerenciador de atalhos
- Atalhos listados na tabela acima
- Tela de referência rápida (overlay com `?`)

### Não Inclui (Out of Scope)
- Atalhos customizáveis pelo usuário
- Suporte a gamepads

---

## Impacto Técnico

| Área | Impacto | Observação |
|------|---------|------------|
| Novo script: `KeyboardShortcutManager.cs` | Médio | Componente na cena principal |
| `Assets/Scripts/Programming/Blocks/Block.cs` | Baixo | Expor seleção de bloco se ainda não existir |

---

## Dependências

- QW-001 (Undo/Redo) — para os atalhos `Ctrl+Z` / `Ctrl+Y`
- MED-001 (Salvar/Carregar) — para `Ctrl+S`

---

## Riscos

| Risco | Probabilidade | Severidade | Mitigação |
|-------|--------------|------------|-----------|
| `Delete` acionado acidentalmente ao digitar em campos de texto | Alta | Média | Verificar `EventSystem.current.currentSelectedGameObject` antes de agir |
