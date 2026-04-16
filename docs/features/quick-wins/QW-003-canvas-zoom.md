# QW-003 — Zoom no Canvas de Programação

> **Categoria:** Quick Win  
> **Status:** Proposta  
> **Prioridade:** Média  
> **Estimativa:** 2–3 dias  
> **Responsável:** —  

---

## Resumo

Permitir que o usuário amplie ou reduza a visualização do canvas de blocos usando o scroll do mouse ou botões `+` / `-`, para trabalhar confortavelmente com programas grandes.

---

## Motivação / Problema

Programas com muitos blocos rapidamente extrapolam o espaço visível do canvas, forçando o usuário a rolar a tela sem ter uma visão geral. Não há atualmente nenhum controle de zoom.

---

## Solução Proposta

1. Aplicar `localScale` ao `RectTransform` do container do canvas (`CodeContent`).
2. Capturar `Input.mouseScrollDelta.y` para incrementar/decrementar o zoom.
3. Adicionar botões `+` e `-` na UI para usuários sem scroll (touchpad/touch).
4. Limitar o zoom entre `0.3x` e `2.0x`.
5. Zoom centralizado no cursor do mouse (ajustar `anchoredPosition` para preservar o ponto focal).

---

## Critérios de Aceite

- [ ] Scroll do mouse aumenta/diminui o zoom do canvas
- [ ] Botões `+` e `-` realizam zoom em incrementos fixos (ex: 10%)
- [ ] Zoom mínimo: 30% | Zoom máximo: 200%
- [ ] O zoom é centralizado no ponto onde o cursor está
- [ ] Arrastar e conectar blocos funciona corretamente em qualquer nível de zoom
- [ ] Indicador visual do nível de zoom atual (ex: "75%")

---

## Escopo

### Inclui
- Zoom por scroll do mouse
- Botões `+` e `-` na UI
- Indicador de porcentagem de zoom

### Não Inclui (Out of Scope)
- Pinch-to-zoom (mobile — escopo de BIG-005 futuro)
- Pan (mover canvas) com botão do meio do mouse (pode ser tratado em item separado)
- Persistência do nível de zoom entre sessões

---

## Impacto Técnico

| Área | Impacto | Observação |
|------|---------|------------|
| Novo script: `CanvasZoomController.cs` | Alto | Componente dedicado ao canvas |
| `Assets/Scripts/Programming/Blocks/Block.cs` | Médio | `kMinimumAttachRadius` deve escalar com zoom |
| Prefab/cena principal | Médio | Botões e indicador adicionados ao HUD |

---

## Dependências

Nenhuma dependência externa.

---

## Riscos

| Risco | Probabilidade | Severidade | Mitigação |
|-------|--------------|------------|-----------|
| `kMinimumAttachRadius` hardcoded em `Block.cs` perde calibração com zoom | Alta | Alta | Dividir o raio pelo fator de escala atual ao comparar distâncias |
| Cálculo de posição absoluta das `Connection`s quebra com scale != 1 | Média | Alta | Converter coordenadas para espaço de mundo antes de comparar |
