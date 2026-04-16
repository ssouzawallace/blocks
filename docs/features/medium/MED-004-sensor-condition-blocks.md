# MED-004 — Blocos de Condição para Sensores

> **Categoria:** Médio  
> **Status:** Proposta  
> **Prioridade:** Alta  
> **Estimativa:** 4–6 dias  
> **Responsável:** —  

---

## Resumo

Criar blocos que expõem leituras dos sensores do robô (ultrassônico, cor, nível de luz) como valores numéricos ou lógicos conectáveis a blocos de condição (`if`, `while`), permitindo programas reativos ao ambiente.

---

## Motivação / Problema

O robô já possui sensores funcionando na simulação (`UltrasonicSensorController`, `ColorSensorController`), mas não há blocos que exponham essas leituras ao programa visual. O usuário não consegue escrever "se obstáculo próximo, desvie" sem esses blocos — que é exatamente o tipo de programa mais ensinado em robótica educacional.

---

## Solução Proposta

Novos blocos do tipo `NumberBlock` e `ConditionBlock` que delegam ao `RobotController` para obter leituras em tempo de execução:

| Bloco | Tipo | Retorno | Código gerado |
|-------|------|---------|---------------|
| `DistanciaFrontal` | Número | `float` (cm) | `robot.read_ultrasonic(0)` |
| `NivelDeLuz` | Número | `float` (0–1) | `robot.read_light(0)` |
| `CorDetectada` | Texto/Enum | `string` | `robot.read_color(0)` |
| `ObstaculoAFrente` | Lógica | `bool` | `robot.read_ultrasonic(0) < threshold` |
| `SobreLinhaEscura` | Lógica | `bool` | `robot.read_light(0) < 0.3` |

---

## Critérios de Aceite

- [ ] Bloco `DistanciaFrontal` retorna número conectável a `ConditionOperatorBlock`
- [ ] Bloco `ObstaculoAFrente` conecta diretamente no slot de condição lógica do `IfThenBlock`
- [ ] Programa `while ObstaculoAFrente [brake]` executa corretamente no simulador
- [ ] Blocos aparecem na seção correta da paleta (Números / Condição)
- [ ] Blocos de sensores são condicionalmente visíveis conforme `RobotConfiguration` ativa
- [ ] Testes de geração de código em `Assets/Tests/EditMode/`

---

## Escopo

### Inclui
- Blocos para: distância ultrassônica, nível de luz, obstáculo à frente, sobre linha escura
- Visibilidade condicional baseada em `RobotConfiguration`

### Não Inclui (Out of Scope)
- Blocos para array de sensores (ultrassônicos laterais) — complexidade extra
- Leitura de cor exata como bloco (difícil de comparar visualmente)

---

## Impacto Técnico

| Área | Impacto | Observação |
|------|---------|------------|
| Novos scripts para cada bloco de sensor | Alto | Subclasses de `NumberBlock` ou `ConditionBlock` |
| `Assets/Scripts/Robot/RobotController.cs` | Baixo | API já existe; expor via interface de runtime |
| `Assets/Scripts/Programming/BlocksPallete.cs` | Médio | Novas seções ou entradas |
| Novos prefabs de blocos sensor | Médio | Um por tipo de bloco |
| `Assets/Tests/EditMode/` | Médio | Testes com mock de sensor |

---

## Dependências

Nenhuma dependência obrigatória. Melhor integração com `RobotConfiguration` já existente.

---

## Riscos

| Risco | Probabilidade | Severidade | Mitigação |
|-------|--------------|------------|-----------|
| Leitura de sensor em código estático (EditMode) não é possível sem mock | Alta | Alta | Usar valores mock/stub nos testes de código gerado |
| Blocos de sensor aparecem mesmo com robô sem o sensor configurado | Alta | Média | Filtrar visibilidade na paleta por `RobotConfiguration.hasUltrasonicSensors` etc. |
