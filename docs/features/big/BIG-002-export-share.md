# BIG-002 — Exportar e Compartilhar Programas

> **Categoria:** Alto Impacto  
> **Status:** Proposta  
> **Prioridade:** Média  
> **Estimativa:** 5–8 dias  
> **Responsável:** —  

---

## Resumo

Permitir que o usuário exporte seu programa como arquivo `.blocks` (JSON) para compartilhar com colegas ou professor, e que importe arquivos recebidos. Opcionalmente, gerar o código Python legível para fins educacionais.

---

## Motivação / Problema

Hoje, programas existem apenas na memória de uma sessão. Não há como:
- Um professor avaliar o trabalho de um aluno
- Um aluno mostrar seu programa para um colega
- Criar uma biblioteca de programas de exemplo

Isso isola completamente o uso do editor e impede atividades colaborativas.

---

## Solução Proposta

### Exportar
1. Serializar o programa no mesmo formato JSON definido em MED-001.
2. Salvar como arquivo `.blocks` em local escolhido pelo usuário (`StandaloneFileBrowser` ou diálogo nativo Unity).
3. Opcional: botão "Exportar Python" que salva o código gerado por `GetCode()` como `.py`.

### Importar
1. Abrir diálogo de arquivo para selecionar `.blocks`.
2. Deserializar e instanciar os blocos, substituindo o programa atual (com confirmação se houver programa não salvo).

### Compartilhamento (futuro)
- O arquivo `.blocks` é pequeno (texto JSON) e pode ser enviado por e-mail, chat ou plataforma escolar (Google Classroom, etc.) sem necessidade de infraestrutura adicional.

---

## Critérios de Aceite

- [ ] Botão "Exportar" salva arquivo `.blocks` no disco
- [ ] Botão "Importar" carrega arquivo `.blocks` e restaura o programa
- [ ] O programa importado produz o mesmo código Python que o original
- [ ] "Exportar Python" gera arquivo `.py` com o código indentado legível
- [ ] Confirmação é solicitada ao importar se o programa atual não foi salvo
- [ ] Arquivos exportados de versões anteriores são compatíveis (versioning)

---

## Escopo

### Inclui
- Exportar/importar arquivo `.blocks`
- Exportar código Python `.py`
- Diálogo de arquivo nativo ou simplificado

### Não Inclui (Out of Scope)
- Upload/download para servidor remoto
- Compartilhamento por QR Code ou link
- Integração direta com plataformas LMS (Moodle, Google Classroom)

---

## Impacto Técnico

| Área | Impacto | Observação |
|------|---------|------------|
| `Assets/Scripts/Programming/ProgramSerializer.cs` (de MED-001) | Alto | Reutilizado aqui |
| Novo script: `ExportImportManager.cs` | Alto | Diálogos de arquivo e orquestração |
| `Assets/Scripts/Programming/PythonEditorWindow.cs` | Baixo | Pode expor a string de código para exportação |

---

## Dependências

- **MED-001** (Salvar/Carregar) — reutiliza a lógica de serialização JSON

---

## Riscos

| Risco | Probabilidade | Severidade | Mitigação |
|-------|--------------|------------|-----------|
| API de diálogo de arquivo não disponível em todas as plataformas Unity | Alta | Alta | Usar `StandaloneFileBrowser` (asset open source) ou fallback para campo de texto com caminho |
| Arquivo `.blocks` compartilhado com versão futura incompatível | Média | Alta | Versionar o formato JSON; escrever migração ao carregar |
