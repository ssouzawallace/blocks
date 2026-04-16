# BIG-004 — Sistema de Tutoriais Guiados

> **Categoria:** Alto Impacto  
> **Status:** Proposta  
> **Prioridade:** Média  
> **Estimativa:** 12–20 dias  
> **Responsável:** —  

---

## Resumo

Criar um sistema de missões/tutoriais passo-a-passo integrado ao editor que guia o usuário desde o primeiro bloco até programas reativos com sensores, tornando o produto autossuficiente para uso em sala de aula sem necessidade de um instrutor presente.

---

## Motivação / Problema

Novos usuários não sabem por onde começar. Não há onboarding, exemplos interativos ou feedback contextual. Em contexto escolar, isso exige presença constante do professor para orientar cada aluno — limitando a escala de uso. Produtos como Code.org e Scratch oferecem tutoriais embutidos que permitem aprendizado autônomo.

---

## Solução Proposta

### Estrutura
1. **`TutorialManager`** — ScriptableObject que define a lista ordenada de missões.
2. **`TutorialStep`** — Cada passo tem: instrução textual, ação esperada (ex: "conecte o bloco X ao bloco Y"), validação automática, e dica opcional.
3. **Overlay de UI** — Painel lateral com o passo atual, seta apontando para o elemento relevante, botão "Próximo" (quando a validação passa).
4. **Missões iniciais sugeridas:**

| # | Título | Objetivo de Aprendizagem |
|---|--------|--------------------------|
| 1 | Olá, Robô! | Montar o primeiro programa (Start + Frente) |
| 2 | Andar e Parar | Usar SetSpeed e Brake |
| 3 | Fazer um Quadrado | Usar repetição com sequência de movimentos |
| 4 | Desviar de Obstáculo | Usar if + sensor ultrassônico |
| 5 | Seguir Linha | Usar while + sensor de luz |

---

## Critérios de Aceite

- [ ] Menu inicial oferece opção "Iniciar Tutorial" e "Modo Livre"
- [ ] Cada passo exibe instrução clara e destaca visualmente o elemento alvo (highlight/seta)
- [ ] Validação automática detecta quando o usuário completou a ação esperada
- [ ] O usuário pode pular um passo (com confirmação)
- [ ] Progresso do tutorial é salvo localmente (`PlayerPrefs`)
- [ ] Modo tutorial não interfere no Modo Livre — pode ser ativado/desativado a qualquer momento
- [ ] As 5 missões iniciais descritas acima estão implementadas

---

## Escopo

### Inclui
- Motor de tutoriais (TutorialManager + TutorialStep)
- Overlay de UI com instruções e highlights
- As 5 missões iniciais

### Não Inclui (Out of Scope)
- Editor de missões para professores (pode ser feature futura)
- Gamificação (pontos, badges, ranking)
- Sincronização de progresso com servidor

---

## Fluxo de Validação (exemplo — Missão 1)

```
Passo 1: "Arraste o bloco Início para o canvas"
  → Validação: StartBlock instanciado no CodeContent?
  → Se sim: avança para passo 2

Passo 2: "Conecte o bloco 'Frente' abaixo do bloco Início"
  → Validação: SimpleInstructionBlock('thisway') conectado ao StartBlock?
  → Se sim: avança para passo 3

Passo 3: "Clique em Executar"
  → Validação: programa executado sem erro?
  → Se sim: missão concluída — confete 🎉
```

---

## Impacto Técnico

| Área | Impacto | Observação |
|------|---------|------------|
| Novo script: `TutorialManager.cs` | Alto | Motor de missões |
| Novo ScriptableObject: `TutorialData.cs` | Alto | Definição declarativa das missões |
| Novo prefab: `TutorialOverlay` | Alto | UI de instruções e highlight |
| `Assets/Scripts/Programming/Blocks/Block.cs` | Baixo | Expor eventos de drag/connect para validação |
| `Assets/Tests/EditMode/` | Médio | Testes da lógica de validação das missões |

---

## Dependências

- MED-004 (Blocos de Sensor) — necessário para as missões 4 e 5
- BIG-001 (Cenários) — missão de seguidor de linha requer o cenário correspondente

---

## Riscos

| Risco | Probabilidade | Severidade | Mitigação |
|-------|--------------|------------|-----------|
| Validação frágil quebra com refatorações nos tipos de bloco | Alta | Média | Usar interfaces/eventos em vez de comparações de tipo direto |
| Usuário frustrado por validação que não reconhece ação correta | Alta | Alta | Testes extensivos com usuários reais; adicionar botão "Ajuda" com dica extra |
| Localização (PT-BR vs EN) de textos do tutorial | Média | Baixa | Centralizar strings em ScriptableObject; i18n simples com tabela de strings |
