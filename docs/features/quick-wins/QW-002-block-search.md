# QW-002 — Busca e Filtro de Blocos na Paleta

> **Categoria:** Quick Win  
> **Status:** Proposta  
> **Prioridade:** Média  
> **Estimativa:** 2–3 dias  
> **Responsável:** —  

---

## Resumo

Adicionar um campo de busca ao topo da paleta de blocos que filtra em tempo real os blocos exibidos por nome ou categoria.

---

## Motivação / Problema

À medida que novas categorias e blocos são adicionados, a paleta cresce e localizar o bloco correto torna-se lento. Estudantes iniciantes perdem tempo procurando visualmente ao invés de focar na lógica do programa.

---

## Solução Proposta

1. Adicionar um `InputField` (TMP) no topo do painel da paleta.
2. Ao digitar, filtrar dinamicamente os blocos exibidos comparando o texto digitado com o nome/label de cada bloco.
3. Seções sem resultados ficam ocultas (`gameObject.SetActive(false)`).
4. Campo vazio restaura a visualização completa.

---

## Critérios de Aceite

- [ ] Campo de busca visível no topo da paleta
- [ ] Digitar "move" exibe apenas blocos de movimento
- [ ] Digitar texto sem correspondência exibe mensagem "Nenhum bloco encontrado"
- [ ] Limpar o campo restaura todos os blocos imediatamente
- [ ] A busca é insensível a maiúsculas/minúsculas

---

## Escopo

### Inclui
- Campo de busca por nome de bloco
- Ocultação de seções vazias
- Mensagem de estado vazio

### Não Inclui (Out of Scope)
- Busca por palavra-chave no código gerado pelo bloco
- Filtros avançados (por tipo de conexão, por sensor)

---

## Impacto Técnico

| Área | Impacto | Observação |
|------|---------|------------|
| `Assets/Scripts/Programming/BlocksPallete.cs` | Alto | Lógica de filtro adicionada aqui |
| `Assets/Scripts/Programming/PalleteSection.cs` | Médio | Expor método para ocultar/exibir seção |
| Prefab da paleta (cena) | Médio | Adicionar InputField ao layout |
| `Assets/Tests/EditMode/` | Baixo | Testes de filtro opcionais |

---

## Dependências

Nenhuma dependência externa.

---

## Riscos

| Risco | Probabilidade | Severidade | Mitigação |
|-------|--------------|------------|-----------|
| Nomes dos blocos não estão centralizados (apenas no prefab) | Alta | Média | Adicionar campo `blockName` à classe `Block` ou ao prefab |
