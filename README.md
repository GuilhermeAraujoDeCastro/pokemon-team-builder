# Team Builder Pokémon

Site em ASP.NET Core onde cada usuário cria conta, monta até 6 times de Pokémon e recebe na hora uma análise de quais tipos de ataque são fraqueza ou resistência do time inteiro, com sugestão de quem adicionar pra cobrir os buracos. É o segundo projeto da minha trilogia Pokémon: o primeiro foi o Simulador de Batalha em Python (linha de comando), esse aqui troca o terminal por um site com login e banco de dados de verdade, e o terceiro vai ser um extrator e analisador de dados sobre os jogos.

## Como configurar

Precisa do SDK do .NET 8 instalado (dotnet.microsoft.com/download).

```bash
cd team-builder-pokemon
dotnet restore
```

Se ainda não tiver a ferramenta de linha de comando do Entity Framework Core instalada:

```bash
dotnet tool install --global dotnet-ef
```

Se já tiver uma versão antiga e der erro de versão, troque o `install` por `dotnet tool update --global dotnet-ef`.

## Como rodar

```bash
cd src/TeamBuilderPokemon
dotnet ef migrations add AddPokemonTeams
dotnet ef database update
dotnet run
```

O primeiro comando gera a migration que cria as tabelas de Pokémon, Time e Slot (a migration da parte de login já vem pronta desde a criação do projeto). O segundo aplica todas as migrations num banco SQLite novo, `app.db`, criado automaticamente dentro da pasta do projeto e fora do controle de versão. O terceiro sobe o site: o terminal mostra a URL local, algo como `https://localhost:7xxx`.

Abra essa URL, clique em "Criar conta", cadastre um e-mail e uma senha (não precisa confirmar por e-mail, o projeto não manda e-mail nenhum) e você já entra direto. Dali, "Minhas equipes" e depois "Criar nova equipe".

## Sobre esse projeto eu não consegui compilar sozinho (leia antes de rodar)

Diferente dos outros dois projetos da trilogia, que eu testei e rodei inteiros antes de entregar, esse eu escrevi sem conseguir compilar nem uma vez. O ambiente onde eu rodo código não tem acesso à NuGet, o repositório oficial de pacotes do .NET, então todo `dotnet restore` falha com erro de rede, e sem restore não tem como compilar nada em C#. Confirmei que não era algo específico desse projeto tentando até um `dotnet new console` vazio, que falhou do mesmo jeito: é uma restrição do ambiente inteiro, não um problema deste código.

O que eu fiz pra compensar: usei o `dotnet new mvc --auth Individual` de verdade pra gerar a base do projeto (login, banco, estrutura de pastas), porque essa parte funciona mesmo sem internet, só o restore automático no final que falha. Todo o código novo (os modelos de Pokémon, Time e Slot, a tabela de tipos, a análise de time, o controller e as telas) foi escrito em cima dessa base real, e reli cada arquivo linha por linha procurando erro, em vez de só confiar que ia funcionar. Nessa releitura encontrei e corrigi um bug de verdade que já vinha na base gerada pelo template: faltava a linha `app.UseAuthentication()` no `Program.cs`, e sem ela o login nunca teria funcionado de verdade (o site aceitaria a senha, mas esqueceria quem você era assim que a página seguinte carregasse). Também abri o `app.db` que o template tinha criado, pra confirmar que o esquema de login já estava com a migration certa registrada, e apaguei esse arquivo antes de te mandar o projeto: o `dotnet ef database update` recria ele do zero, e um banco montado do zero pelas migrations é mais confiável do que um arquivo pronto de origem incerta.

A tabela de efetividade de tipos é a mesma matriz de 18 tipos que eu já testei e usei no Simulador de Batalha, o primeiro projeto da trilogia. Copiei os mesmos números, tipo por tipo, então se aquele projeto está certo, esse também está.

Mesmo assim, é código C# que nunca rodou de ponta a ponta. Segue os passos de "Como rodar" acima e, se `dotnet restore`, `dotnet ef` ou `dotnet run` derem algum erro, me manda a mensagem completa que eu conserto. Acho bem provável que dê tudo certo de primeira, mas prefiro avisar antes do que prometer sem poder confirmar.

## O que o site faz

Cada usuário só vê os próprios times: login via ASP.NET Core Identity, senha com hash, tudo isolado por conta. Um time tem nome e até 6 Pokémon, escolhidos de um catálogo fixo de 36 que cobre os 18 tipos que existem. Na tela de detalhes de cada time, o site calcula, pra cada um dos 18 tipos de ataque, quantos Pokémon do time são fracos, resistentes ou imunes a ele. Marca como fraqueza quando mais da metade do time toma dano dobrado ou mais, como fraqueza crítica quando são dois terços ou mais, e como resistência quando mais da metade toma dano reduzido. Pra cada fraqueza, sugere até 5 Pokémon do catálogo, entre os que ainda não estão no time, que resistem aquele tipo.

## Rodando os testes

```bash
cd team-builder-pokemon
dotnet test
```

São 9 testes com xUnit, todos sobre a lógica pura de tipos e análise de time, sem tocar em banco de dados ou HTTP. Cinco confirmam a tabela de efetividade: super efetivo, pouco efetivo, imunidade, os dois tipos de um Pokémon multiplicando junto, e que maiúscula ou minúscula no nome do tipo não muda o resultado. Quatro confirmam a análise de time: fraqueza crítica quando a maioria do time é fraca a um tipo, sugestão de um Pokémon do catálogo que resolve essa fraqueza, um time vazio que não quebra o cálculo, e a garantia de que a análise sempre devolve exatamente 18 linhas, uma por tipo.

## Arquitetura

```
TeamBuilderPokemon.sln
src/TeamBuilderPokemon/
  Models/
    Pokemon.cs                     # Id, Nome, Tipo1, Tipo2 (opcional)
    Team.cs                        # Id, Nome, dono (UserId), lista de slots
    TeamSlot.cs                    # liga um Time a um Pokemon numa posição (1 a 6)
    PokemonSeedData.cs             # catálogo fixo com os 36 Pokémon
  Services/
    TypeChart.cs                   # a mesma tabela de 18 tipos do simulador em Python
    TeamAnalyzer.cs                # calcula fraquezas e resistências de um time inteiro
  Controllers/
    TeamsController.cs             # criar, listar, ver análise e excluir times
  Views/Teams/                     # telas de listar, criar e ver detalhes de um time
  Data/ApplicationDbContext.cs     # contexto do EF Core (Identity + Pokemon/Team/TeamSlot)
  Areas/Identity/                  # páginas de login e cadastro, geradas pelo template oficial
tests/TeamBuilderPokemon.Tests/
  TypeChartTests.cs                # 5 testes da tabela de tipos
  TeamAnalyzerTests.cs             # 4 testes da análise de time
```

A separação em `Services/TypeChart.cs` e `Services/TeamAnalyzer.cs` é de propósito: nenhuma das duas classes conhece Entity Framework, ASP.NET ou banco de dados, então dá pra testar a regra de negócio inteira sem simular HTTP nem banco, do mesmo jeito que o simulador em Python separa o cálculo de dano do resto.

## O que eu treinei com esse projeto

ASP.NET Core MVC com autenticação individual via ASP.NET Core Identity, Entity Framework Core com SQLite e seed de dados por `HasData`, e um relacionamento muitos-para-muitos com atributo extra (a posição no time) modelado como entidade própria em vez de uma tabela de junção simples. Também treinei portar a mesma lógica de domínio, a tabela de tipos, de uma linguagem pra outra mantendo os dois lados consistentes. E foi a primeira vez que escrevi um projeto inteiro sem poder compilar nem uma vez, o que me obrigou a ler cada arquivo com mais cuidado do que o normal.

## Próximos passos possíveis

Deixar o usuário editar apelido e nível de cada Pokémon do time, simular uma batalha entre dois times usando o motor do Simulador de Batalha em Python, ou uma versão da mesma fórmula aqui em C#, e publicar o site de verdade num serviço como Azure ou Render pra virar um link que dá pra colocar no currículo em vez de só um repositório.
