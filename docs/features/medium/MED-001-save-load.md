# MED-001 — Salvar e Carregar Programas

> **Categoria:** Médio  
> **Status:** Proposta  
> **Prioridade:** Alta  
> **Estimativa:** 5–8 dias  
> **Responsável:** —  

---

## Resumo

Permitir que o usuário salve o programa montado em arquivo local e o carregue em sessões futuras, preservando posições, tipos e conexões de todos os blocos.

---

## Motivação / Problema

Todo progresso é perdido ao encerrar o editor. Em contexto educacional, alunos precisam de múltiplas sessões para concluir um exercício. A ausência de persistência é uma das maiores limitações do sistema para uso real em sala de aula.

---

## Solução Proposta

1. **Serialização:** Percorrer o grafo de blocos e gerar um JSON descrevendo cada bloco (tipo, posição no canvas, id, lista de conexões com referências por id).
2. **Salvar:** Gravar o JSON em `Application.persistentDataPath/saves/<nome>.blocks`.
3. **Carregar:** Ler o JSON, instanciar os prefabs correspondentes e restaurar as conexões.
4. **UI:** Botões "Salvar" e "Carregar" na barra de ferramentas; diálogo simples para nomear o arquivo.

---

## Critérios de Aceite

- [ ] Clicar em "Salvar" persiste o programa atual em arquivo local
- [ ] Clicar em "Carregar" abre a lista de programas salvos e restaura o selecionado
- [ ] Após carregar, o programa executa produzindo o mesmo código que antes do salvamento
- [ ] Posições dos blocos no canvas são restauradas corretamente
- [ ] Conexões entre blocos são restauradas corretamente
- [ ] Arquivo corrompido/inválido exibe mensagem de erro sem travar o editor

---

## Escopo

### Inclui
- Serialização/deserialização do grafo de blocos em JSON
- Armazenamento local (`persistentDataPath`)
- UI básica de salvar/carregar (lista de arquivos, botão de confirmar)

### Não Inclui (Out of Scope)
- Sincronização em nuvem
- Múltiplos slots de salvamento com pré-visualização
- Controle de versão de arquivos

---

## Estrutura do JSON (Exemplo)

```json
{
  "version": "1.0",
  "blocks": [
    {
      "id": "block-0",
      "type": "StartBlock",
      "position": { "x": 120.0, "y": -80.0 }
    },
    {
      "id": "block-1",
      "type": "SimpleInstructionBlock",
      "instruction": "thisway",
      "position": { "x": 120.0, "y": -117.0 },
      "connections": [
        { "connectionIndex": 0, "attachedBlockId": "block-0", "attachedConnectionIndex": 1 }
      ]
    }
  ]
}
```

---

## Impacto Técnico

| Área | Impacto | Observação |
|------|---------|------------|
| Novo script: `ProgramSerializer.cs` | Alto | Serialização/deserialização |
| Novo script: `SaveLoadManager.cs` | Alto | Orquestração do fluxo salvar/carregar |
| `Assets/Scripts/Programming/Blocks/Block.cs` | Médio | Adicionar campo `blockType` serializável |
| Todos os subtipos de `Block` | Médio | Expor dados específicos para serialização |
| `Assets/Tests/EditMode/` | Alto | Testes de round-trip (salvar → carregar → comparar código gerado) |

---

## Dependências

Nenhuma dependência obrigatória (QW-004 `Ctrl+S` é opcional).

---

## Riscos

| Risco | Probabilidade | Severidade | Mitigação |
|-------|--------------|------------|-----------|
| Refatorações futuras nos tipos de bloco invalidam saves antigos | Média | Alta | Versionar o formato JSON; migração automática |
| Conexões circulares no grafo causam stack overflow na serialização | Baixa | Alta | Percorrer grafo iterativamente ou usar `visited` set |
