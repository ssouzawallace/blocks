# BIG-001 — Novos Cenários de Simulação

> **Categoria:** Alto Impacto  
> **Status:** Proposta  
> **Prioridade:** Alta  
> **Estimativa:** 10–15 dias  
> **Responsável:** —  

---

## Resumo

Criar uma biblioteca de novos cenários de simulação (labirinto, seguidor de linha, corrida com obstáculos, classificação por cores) que aumentem o valor educacional do projeto e permitam desafios progressivos em sala de aula.

---

## Motivação / Problema

O simulador atual possui cenários limitados (`ScenarioType` enum com poucos valores), o que restringe os exercícios que professores podem propor. Plataformas de robótica educacional de referência (LEGO Mindstorms, Scratch) oferecem múltiplos ambientes de desafio progressivo. Sem isso, o produto esgota seu valor pedagógico rapidamente.

---

## Solução Proposta

Criar 4 cenários novos como `GameObject` separados, registrados no `ScenarioController`:

### Cenário A — Labirinto
- Grade 5×5 com paredes internas
- Objetivo: navegar do ponto A ao ponto B sem colidir
- Ensina: condicionais com ultrassônico, tomada de decisão

### Cenário B — Seguidor de Linha Curva
- Pista com linha preta curvada em superfície branca
- Objetivo: seguir a linha usando sensores de cor
- Ensina: loops, leitura de sensor de luz, controle proporcional básico

### Cenário C — Corrida com Obstáculos
- Pista reta com obstáculos móveis e estáticos
- Objetivo: completar o percurso no menor tempo
- Ensina: velocidade variável, desvio de obstáculos

### Cenário D — Classificação por Cores
- Área com objetos de cores diferentes e zonas de destino coloridas
- Objetivo: mover cada objeto à zona correspondente
- Ensina: estrutura if/else em cadeia, leitura de sensor de cor

---

## Critérios de Aceite

- [ ] Os 4 cenários aparecem no dropdown do Robot Simulation Editor
- [ ] Cada cenário tem colisões funcionais que o robô detecta com os sensores existentes
- [ ] Troca de cenário em tempo de execução funciona sem reiniciar o Play Mode
- [ ] Cada cenário tem documentação de uso no `README` ou wiki
- [ ] Performance aceitável (>30fps) com o cenário mais complexo

---

## Escopo

### Inclui
- 4 cenários listados acima
- Assets visuais básicos (podem ser primitivos Unity: cubos, planos, materiais coloridos)
- Registro no `ScenarioController` e `ScenarioType` enum

### Não Inclui (Out of Scope)
- Editor de cenários customizados pelo usuário
- Sistema de pontuação/ranking por cenário (pode ser feature futura)
- Assets 3D complexos ou animações de cenário

---

## Impacto Técnico

| Área | Impacto | Observação |
|------|---------|------------|
| `Assets/Scripts/Scenario/ScenarioController.cs` | Alto | Adicionar 4 novos tipos ao enum e switch |
| Novas cenas/prefabs de cenário | Alto | Um prefab por cenário |
| `Assets/Scripts/Robot/UltrasonicSensorController.cs` | Baixo | Paredes do labirinto devem estar na layer correta |
| `Assets/Scripts/Robot/ColorSensorController.cs` | Baixo | Linha preta/branca deve ter Material correto para raycast |

---

## Dependências

- MED-004 (Blocos de Sensor) — para que os cenários de labirinto e seguidor de linha sejam plenamente utilizáveis via blocos

---

## Riscos

| Risco | Probabilidade | Severidade | Mitigação |
|-------|--------------|------------|-----------|
| Assets visuais muito simples reduzem engajamento dos alunos | Média | Média | Usar materiais com cores vibrantes; shapes geométricas claras |
| Colisões do labirinto com físicas do robô causam travamentos | Média | Alta | Usar colliders simples; testar com robô em velocidade máxima |
