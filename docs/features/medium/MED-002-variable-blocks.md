# MED-002 — Blocos de Variáveis

> **Categoria:** Médio  
> **Status:** Proposta  
> **Prioridade:** Alta  
> **Estimativa:** 5–7 dias  
> **Responsável:** —  

---

## Resumo

Introduzir blocos que permitem ao usuário criar, ler e modificar variáveis numéricas dentro do programa, viabilizando algoritmos com estado (contadores, acumuladores, flags).

---

## Motivação / Problema

Atualmente, os únicos valores numéricos disponíveis são constantes literais (`ConstantNumberBlock`) e leituras de sensores. Não é possível, por exemplo, contar o número de voltas do robô ou acumular a leitura de um sensor ao longo do tempo. Isso limita drasticamente a complexidade dos programas que podem ser criados.

---

## Solução Proposta

Três novos tipos de bloco:

| Bloco | Tipo | Descrição | Código gerado (IronPython) |
|-------|------|-----------|---------------------------|
| `DefinirVariável` | Instrução | Cria/atribui valor a variável | `nome = valor` |
| `LerVariável` | Número | Retorna o valor atual da variável | `nome` |
| `AlterarVariável` | Instrução | Incrementa/decrementa variável | `nome = nome + delta` |

Um gerenciador de variáveis (`VariableManager`) mantém a lista de variáveis ativas e popula os blocos com seus nomes via dropdown.

---

## Critérios de Aceite

- [ ] Usuário consegue criar uma variável com nome personalizado
- [ ] `DefinirVariável` aparece na paleta e conecta como instrução regular
- [ ] `LerVariável` conecta no slot de número (ex: argumento do `SetSpeed`)
- [ ] `AlterarVariável` permite incremento/decremento por valor constante
- [ ] O código Python gerado é válido e executa corretamente no IronPython 2.7
- [ ] Renomear uma variável atualiza todos os blocos que a referenciam
- [ ] Deletar uma variável alerta o usuário se ela ainda está em uso

---

## Escopo

### Inclui
- Variáveis do tipo numérico (float)
- Três blocos: Definir, Ler, Alterar
- Painel simples de gerenciamento de variáveis

### Não Inclui (Out of Scope)
- Variáveis do tipo string ou booleano (podem ser adicionadas em iteração futura)
- Variáveis globais entre múltiplos programas
- Escopo de variável por bloco de controle (todas são globais ao programa)

---

## Impacto Técnico

| Área | Impacto | Observação |
|------|---------|------------|
| Novo script: `VariableManager.cs` | Alto | Gerencia lista de variáveis |
| Novos scripts: `SetVariableBlock.cs`, `ReadVariableBlock.cs`, `ChangeVariableBlock.cs` | Alto | Três novas subclasses de `Block` |
| `Assets/Scripts/Programming/BlocksPallete.cs` | Médio | Nova seção "Variáveis" na paleta |
| Novos prefabs para os blocos de variável | Médio | Visual consistente com os existentes |
| `Assets/Tests/EditMode/` | Alto | Testes de geração de código com variáveis |

---

## Dependências

Nenhuma dependência obrigatória.

---

## Riscos

| Risco | Probabilidade | Severidade | Mitigação |
|-------|--------------|------------|-----------|
| Nomes de variável conflitam com palavras reservadas do Python/Logo | Média | Alta | Validar nome ao criar (whitelist alfanumérico + underscore) |
| IronPython 2.7 tem comportamento diferente de Python 3 em escopo de variáveis | Baixa | Média | Testar geração e execução no ambiente IronPython real |
