# Donos, quarentena e exclusão de contas

`Teams.UserId` referencia `AspNetUsers.Id`. Ao excluir uma conta pelo fluxo explícito do Identity, o SQLite remove os times dessa conta, seus slots e revisões na mesma transação da exclusão. Exporte os dados pessoais antes de confirmar: o download do Identity inclui os times ativos, a lixeira e o histórico, além dos campos pessoais do usuário. Ele não inclui times de outras contas nem registros em quarentena.

As migrations `OwnerQuarantineColumns` e `EnforceTeamOwner` preservam os registros que já estavam órfãos. O identificador anterior vai para `LegacyOwnerId`, a visibilidade anterior para `LegacyIsPublic`, e o vínculo atual fica nulo com `IsPublic=false`. Slots e histórico permanecem ligados ao time. A quarentena não aparece nas listas das contas e não pode ser acessada por páginas públicas nem pela API anônima.

Antes de iniciar esta versão com um banco existente:

1. Pare a aplicação e copie o banco, incluindo arquivos WAL/SHM quando presentes. Trabalhe primeiro sobre outra cópia local.
2. Instale o SDK .NET 8 e restaure a ferramenta local com `dotnet tool restore`.
3. Defina `ConnectionStrings__DefaultConnection` para a cópia de teste. A factory de design exige essa variável explicitamente.
4. Gere ou revise o SQL em `docs/migration-owner.sql`. Ele corresponde à passagem da migration de catálogo para as duas migrations de dono.
5. Execute `dotnet ef database update --project src/TeamBuilderPokemon` apenas sobre a cópia configurada e confira `PRAGMA foreign_key_check;` e as contagens de times, slots e revisões.
6. Consulte os registros com `UserId IS NULL`. Atribua um dono somente após confirmar a relação com uma conta válida. Mantenha o time privado durante essa revisão. O procedimento desta auditoria não altera nenhum banco de produção.

O aplicativo também aplica migrations ao iniciar. Por isso, definir a conexão de teste antes do primeiro `dotnet run` é necessário. Os exemplos de variáveis estão em `.env.example`; o .NET não lê esse arquivo automaticamente. `DataProtection__KeysDirectory` pode apontar para `App_Data/keys` para manter as chaves locais fora do perfil do sistema. `App_Data/`, bancos e arquivos de ambiente reais ficam fora do Git.

A reversão das duas migrations restaura o identificador e a visibilidade legados antes de remover os campos de quarentena. Revise o backup e os registros antes de reverter: uma versão antiga da aplicação não tem as proteções de publicação de órfãos. A reversão do esquema não recupera contas ou times que o usuário já excluiu.

## Regressões locais

```bash
dotnet build -c Release
dotnet run --project tests/RegressionTests -c Release
```

O executável de regressão usa SQLite em memória, Identity real e uma PokeAPI simulada. Ele cobre atualização de um banco antigo com órfãos, integridade referencial, isolamento do download, cascata na exclusão, importação de catálogo sem navegação, prévia e confirmação de JSON, entradas inválidas e comparação/restauração de revisões. As oito regressões passaram na auditoria local.
