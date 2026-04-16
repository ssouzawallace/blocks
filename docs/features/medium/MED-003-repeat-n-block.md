# MED-003 — Bloco "Repetir N Vezes"

> **Categoria:** Médio  
> **Status:** Proposta  
> **Prioridade:** Média  
> **Estimativa:** 2–4 dias  
> **Responsável:** —  

---

## Resumo

Adicionar um bloco de controle de fluxo `Repetir [N] vezes` que executa um conjunto de instruções um número fixo de vezes, simplificando padrões comuns que hoje exigem `while` com contador manual.

---

## Motivação / Problema

Para repetir uma ação 3 vezes, o usuário precisa:
1. Criar uma variável contador (não suportado ainda)
2. Usar um `while` com condição `contador < 3`
3. Incrementar o contador dentro do loop

Isso é cognitivamente pesado para iniciantes. Em ferramentas como o Scratch, o bloco "Repetir N vezes" é um dos mais usados por novatos.

---

## Solução Proposta

Novo bloco `RepeatNBlock`, similar a `WhileBlock` em estrutura visual, mas com:
- Um slot de número embutido (`BlockWithArgument`) para informar `N`
- Corpo para encaixar as instruções a repetir

**Código gerado (Logo/Python):**
```
repeat 3 [
    thisway
    wait 1
]
```
Ou equivalente IronPython:
```python
for __i in range(3):
    robot.thisway()
    robot.wait(1)
```

---

## Critérios de Aceite

- [ ] Bloco visível na seção "Controle de fluxo" da paleta
- [ ] Campo numérico `N` aceita constante ou bloco de número conectado
- [ ] Corpo do bloco (`do`) aceita cadeia de instruções
- [ ] Código gerado é válido Python 2.7 / IronPython
- [ ] O bloco redimensiona corretamente conforme blocos são adicionados ao corpo
- [ ] Testes de código gerado passam em `Assets/Tests/EditMode/`

---

## Escopo

### Inclui
- Novo bloco `RepeatNBlock`
- Suporte a `N` via constante ou `NumberBlock`
- Corpo de instruções (como `WhileBlock`)

### Não Inclui (Out of Scope)
- Acesso à variável de iteração `i` dentro do corpo
- Repeat com expressão variável (ex: `Repetir [variável X] vezes`) — pode ser adicionado após MED-002

---

## Impacto Técnico

| Área | Impacto | Observação |
|------|---------|------------|
| Novo script: `RepeatNBlock.cs` | Alto | Subclasse de `Block` (similar a `WhileBlock`) |
| `Assets/Scripts/Programming/BlocksPallete.cs` | Baixo | Registro na seção de fluxo |
| Novo prefab `RepeatNBlock` | Médio | Visual com campo N embutido |
| `Assets/Tests/EditMode/` | Médio | Testes de código gerado |

---

## Dependências

Nenhuma dependência obrigatória. Pode ser aprimorado após MED-002 (variáveis).

---

## Riscos

| Risco | Probabilidade | Severidade | Mitigação |
|-------|--------------|------------|-----------|
| `N = 0` ou negativo causa comportamento inesperado | Alta | Baixa | Clamp `N` para mínimo 0 na geração de código |
