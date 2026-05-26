# Team Builder Pokémon

Site em ASP.NET Core onde cada pessoa cria uma conta, monta times de até 6 Pokémon e vê na hora quais tipos de ataque são fraqueza ou resistência do time inteiro, com sugestão de quem adicionar pra cobrir os buracos. É o segundo projeto da minha trilogia Pokémon: o primeiro foi o Simulador de Batalha em Python e o terceiro é o Extrator de Dados.

## O que o site faz

- Login com ASP.NET Core Identity. Cada conta só vê os próprios times.
- Catálogo inicial de 36 Pokémon que cobre os 18 tipos. Dá pra trazer qualquer outro pelo nome, buscando na PokéAPI.
- Times com apelido e nível (1 a 100) pra cada Pokémon.
- Análise do time contra os 18 tipos de ataque: quantos membros são fracos, resistentes ou imunes a cada um. Mais da metade fraca vira fraqueza, dois terços ou mais vira fraqueza crítica.
- Sugestões pra cada fraqueza, com destaque pros Pokémon que resistem a duas ou mais fraquezas de uma vez.
- Histórico de alterações de cada time, com opção de voltar pra uma versão anterior.
- Lixeira: time excluído pode ser restaurado ou apagado de vez.
- Simulação de batalha entre dois times, turno a turno, usando a tabela de tipos e o nível de cada Pokémon.
- Página pública só leitura pra compartilhar um time.
- API em JSON: `/api/pokemon`, `/api/teams` e `/api/teams/{id}/analysis` (as duas últimas pedem login).
- Nomes dos tipos em português na tela e tema claro e escuro (Bootstrap 5.3).

## Como rodar

Precisa do SDK do .NET 8 (dotnet.microsoft.com/download).

```bash
cd src/TeamBuilderPokemon
dotnet run
```

O banco SQLite (`app.db`) é criado e atualizado sozinho na primeira execução, pelas migrations do projeto. O terminal mostra a URL local, algo como `https://localhost:7xxx`. Crie uma conta e entre em "Minhas equipes".

A confirmação de cadastro por e-mail vem desligada. Pra ligar, coloque `"Email": { "RequireConfirmedAccount": true }` no `appsettings.json`. Os e-mails não são enviados de verdade: viram arquivos `.html` em `App_Data/emails`, o que dispensa conta em provedor de e-mail. Pra usar em produção, troque o `FileEmailSender` por SendGrid ou SMTP.

Se mudar os modelos e precisar de uma migration nova:

```bash
dotnet tool restore
dotnet ef migrations add NomeDaMudanca --project src/TeamBuilderPokemon
```

O GitHub Actions compila o projeto a cada push.

## Arquitetura

```
TeamBuilderPokemon.sln
src/TeamBuilderPokemon/
  Models/          Pokemon, Team, TeamSlot (apelido e nível), TeamRevision (histórico) e o catálogo inicial
  Services/
    TypeChart.cs        tabela de 18 tipos, a mesma do simulador em Python
    TeamAnalyzer.cs     fraquezas, resistências e sugestões de um time
    TeamRules.cs        regras de um time válido
    BattleSimulator.cs  batalha entre dois times
    PokeApiClient.cs    busca de Pokémon novos na PokéAPI
    TypeNames.cs        nomes dos tipos em português
    FileEmailSender.cs  e-mail salvo em arquivo
  Controllers/     TeamsController (telas) e ApiController (JSON)
  Views/           telas dos times, batalha, lixeira e página pública
  Data/            contexto do EF Core e migrations
```

`TypeChart`, `TeamAnalyzer`, `TeamRules` e `BattleSimulator` não conhecem Entity Framework nem ASP.NET. Assim a regra de negócio inteira roda sem banco e sem HTTP, do mesmo jeito que o simulador em Python separa o cálculo de dano do resto.

## O que eu treinei com esse projeto

ASP.NET Core MVC com ASP.NET Core Identity, Entity Framework Core com SQLite e seed por `HasData`, e um relacionamento muitos-para-muitos com dados extras (posição, apelido e nível) modelado como entidade própria. Também portei a mesma lógica de domínio, a tabela de tipos, de Python pra C#, mantendo os dois lados consistentes.

## Licença

Todos os direitos reservados (veja o arquivo LICENSE).
