using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using TeamBuilderPokemon.Controllers;
using TeamBuilderPokemon.Data;
using TeamBuilderPokemon.Models;
using TeamBuilderPokemon.Services;
using TeamBuilderPokemon.Areas.Identity.Pages.Account.Manage;

var tests=new (string Name,Func<Task> Run)[] {
    ("migration preserves orphan team, slots, history and original visibility",Migration),
    ("Identity export isolates accounts; DeleteAsync cascades atomically",IdentityFlow),
    ("catalog import returns JSON without navigation or rewriting draft team",Catalog),
    ("catalog failure and foreign owner preserve teams",CatalogFailure),
    ("JSON round trip preview and confirmation preserve all fields",Exchange),
    ("invalid JSON fields never create partial teams",InvalidExchange),
    ("revision comparison precedes traceable restoration",Revisions),
    ("quarantined owner cannot expose public HTML or anonymous API",Quarantine)
};
foreach(var test in tests){await test.Run();Console.WriteLine("PASS "+test.Name);}
Console.WriteLine($"{tests.Length}/{tests.Length} regressions passed (SQLite in memory, fake API; no production).");
void Check(bool condition,string message){if(!condition)throw new Exception(message);}

async Task Migration(){
    using var f=new Fixture(false);
    var migrator=f.Db.GetService<IMigrator>();await migrator.MigrateAsync("20260923062013_CatalogoTimesEHistorico");
    var id=f.Db.Pokemons.First().Id;
    await f.Db.Database.ExecuteSqlRawAsync("INSERT INTO Teams(Id,Name,UserId,CreatedAtUtc,IsPublic) VALUES (100,'Legacy','missing','2026-01-01',1)");
    await f.Db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO TeamSlots(TeamId,PokemonId,SlotNumber,Level) VALUES (100,{id},1,50)");
    await f.Db.Database.ExecuteSqlRawAsync("INSERT INTO TeamRevisions(TeamId,CreatedAtUtc,SnapshotJson) VALUES (100,'2026-01-01','{{}}')");
    await migrator.MigrateAsync();f.Db.ChangeTracker.Clear();
    var orphan=await f.Db.Teams.Include(t=>t.Slots).Include(t=>t.Revisions).SingleAsync();
    Check(orphan.UserId is null && orphan.LegacyOwnerId=="missing" && orphan.LegacyIsPublic==true && !orphan.IsPublic,"quarantine metadata");
    Check(orphan.Slots.Count==1 && orphan.Revisions.Count==1,"migration lost dependent rows");
    using var command=f.Connection.CreateCommand();command.CommandText="PRAGMA foreign_key_check";using var reader=command.ExecuteReader();Check(!reader.Read(),"foreign key violation after migration");reader.Close();
    try{await f.Db.Database.ExecuteSqlRawAsync("INSERT INTO Teams(Name,UserId,CreatedAtUtc,IsPublic) VALUES ('Bad','missing','2026-01-01',1)");throw new Exception("FK did not reject");}catch(SqliteException){}
    await migrator.MigrateAsync("20260923062013_CatalogoTimesEHistorico");
    command.CommandText="SELECT UserId,IsPublic,(SELECT COUNT(*) FROM TeamSlots),(SELECT COUNT(*) FROM TeamRevisions) FROM Teams WHERE Id=100";
    using(var legacy=command.ExecuteReader()){Check(legacy.Read() && legacy.GetString(0)=="missing" && legacy.GetInt64(1)==1 && legacy.GetInt64(2)==1 && legacy.GetInt64(3)==1,"rollback lost legacy metadata or rows");}
    await migrator.MigrateAsync();
}

async Task IdentityFlow(){
    using var f=new Fixture();var user=await f.User("a");var other=await f.User("b");
    var team=await f.Team(user,"Owned");var second=await f.Team(other,"Other");
    f.Db.TeamRevisions.Add(TeamRevision.FromTeam(team));await f.Db.SaveChangesAsync();
    var page=new DownloadPersonalDataModel(f.Manager,f.Db){PageContext=new PageContext{HttpContext=f.Http(user)}};
    var result=(FileContentResult)await page.OnPostAsync();
    using var json=JsonDocument.Parse(result.FileContents);
    Check(!json.RootElement.GetProperty("personalData").TryGetProperty("PasswordHash",out _),"password hash included in export");
    Check(json.RootElement.GetProperty("teams").GetArrayLength()==1,"export leaked account");
    Check(json.RootElement.GetProperty("teams")[0].GetProperty("Name").GetString()=="Owned","export missing team");
    var deleted=await f.Manager.DeleteAsync(user);Check(deleted.Succeeded,"Identity deletion failed");f.Db.ChangeTracker.Clear();
    Check(!await f.Db.Teams.AnyAsync(t=>t.Id==team.Id),"owned team not cascaded");
    Check(!await f.Db.TeamSlots.AnyAsync(s=>s.TeamId==team.Id) && !await f.Db.TeamRevisions.AnyAsync(s=>s.TeamId==team.Id),"dependent row orphaned");
    Check(await f.Db.Teams.AnyAsync(t=>t.Id==second.Id),"other account modified");
}

async Task Catalog(){
    using var f=new Fixture();var user=await f.User("catalog");var team=await f.Team(user,"Saved draft base");
    var controller=f.Controller(user);
    var result=await controller.ImportPokemon("audit-mon",team.Id);
    Check(result is JsonResult,"import navigates away");
    Check(team.Name=="Saved draft base" && team.Slots[0].Level==50,"import changed saved team");
    Check(await f.Db.Pokemons.AnyAsync(p=>p.Name=="Audit-Mon"),"catalog not updated");
}

async Task CatalogFailure(){
    using var f=new Fixture();var user=await f.User("catfail");var other=await f.User("foreign");var team=await f.Team(other,"Foreign");
    var controller=f.Controller(user,new ApiHandler(true));
    Check(await controller.ImportPokemon("audit-mon",team.Id) is NotFoundObjectResult,"foreign team allowed");
    var result=(ObjectResult)await controller.ImportPokemon("audit-mon",null);
    Check(result.StatusCode==503 && await f.Db.Teams.CountAsync()==1,"failure mutated data");
}

async Task Exchange(){
    using var f=new Fixture();var user=await f.User("exchange");var team=await f.Team(user,"Round trip");team.IsPublic=true;team.Slots[0].Nickname="Faísca";team.Slots[0].Level=73;await f.Db.SaveChangesAsync();
    var controller=f.Controller(user);var text=TeamCodec.Export(team);
    var preview=(ImportTeamViewModel)((ViewResult)await controller.PreviewImport(text)).Model!;
    Check(preview.Preview?.IsPublic==true && preview.Preview.Slots[0].Nickname=="Faísca","preview lost fields");
    Check(await f.Db.Teams.CountAsync()==1,"preview writes data");
    Check(await controller.ConfirmImport(text) is RedirectToActionResult,"confirmation failed");
    var copied=await f.Db.Teams.Include(t=>t.Slots).ThenInclude(s=>s.Pokemon).SingleAsync(t=>t.Id!=team.Id);
    Check(TeamCodec.Export(copied)==text,"round trip changed fields");
}

async Task InvalidExchange(){
    using var f=new Fixture();var user=await f.User("invalid");var team=await f.Team(user,"Valid");var text=TeamCodec.Export(team);
    foreach(var bad in new[]{"{",text.Replace("\"schemaVersion\": 1","\"schemaVersion\": 99"),text.Replace("\"level\": 50","\"level\": 0"),text.Replace("\"level\": 50","\"level\": \"fifty\""),text.Replace("\"schemaVersion\": 1","\"schemaVersion\": 1, \"schemaVersion\": 1"),text.Replace("\"slots\": [","\"slots\": null, \"unexpected\": [")}){
        var result=await f.Controller(user).ConfirmImport(bad);
        Check(result is ViewResult view && ((ImportTeamViewModel)view.Model!).Errors.Count>0,"invalid input accepted");
        Check(await f.Db.Teams.CountAsync()==1,"invalid input partially saved");
    }
}

async Task Revisions(){
    using var f=new Fixture();var user=await f.User("revision");var team=await f.Team(user,"Before");
    var controller=f.Controller(user);
    await controller.Edit(team.Id,new EditTeamInput{Id=team.Id,Name="After",IsPublic=true,Slots=new(){new(){PokemonId=team.Slots[0].PokemonId,Nickname="Changed",Level=75}}});
    var revision=await f.Db.TeamRevisions.SingleAsync();
    var comparison=(RevisionComparison)((ViewResult)await controller.CompareRevision(team.Id,revision.Id)).Model!;
    Check(comparison.Changes.Count==3 && comparison.Changes.Any(c=>c.Contains("nível")),"comparison omits changed fields");
    await controller.RestoreRevision(team.Id,revision.Id);f.Db.ChangeTracker.Clear();
    var restored=await f.Db.Teams.Include(t=>t.Slots).SingleAsync();
    Check(restored.Name=="Before" && !restored.IsPublic && restored.Slots[0].Level==50,"wrong restoration");
    Check(await f.Db.TeamRevisions.CountAsync()==2,"restoration not traceable");
}

async Task Quarantine(){
    using var f=new Fixture();var orphan=new Team{Name="Quarantine",UserId=null,LegacyOwnerId="gone",IsPublic=true};
    orphan.Slots.Add(new TeamSlot{PokemonId=f.Db.Pokemons.First().Id,SlotNumber=1});f.Db.Teams.Add(orphan);await f.Db.SaveChangesAsync();
    var user=await f.User("viewer");Check(await f.Controller(user).Public(orphan.Id) is NotFoundResult,"quarantine public HTML exposed");
    var api=new ApiController(f.Db,f.Manager){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext()}};
    Check(await api.Analysis(orphan.Id) is NotFoundResult,"anonymous API exposed null-owned team");
}

class Fixture:IDisposable{
    public SqliteConnection Connection {get;}=new("Data Source=:memory:");
    public ApplicationDbContext Db {get;}
    public UserManager<IdentityUser> Manager {get;}
    private readonly ServiceProvider _provider;
    public Fixture(bool migrate=true){Connection.Open();var services=new ServiceCollection();services.AddLogging();services.AddDbContext<ApplicationDbContext>(o=>o.UseSqlite(Connection));services.AddIdentityCore<IdentityUser>().AddEntityFrameworkStores<ApplicationDbContext>();_provider=services.BuildServiceProvider();Db=_provider.GetRequiredService<ApplicationDbContext>();Manager=_provider.GetRequiredService<UserManager<IdentityUser>>();if(migrate)Db.Database.Migrate();}
    public async Task<IdentityUser> User(string label){var u=new IdentityUser{UserName=label+"@example.test",Email=label+"@example.test"};var r=await Manager.CreateAsync(u,"Teste123!a");if(!r.Succeeded)throw new Exception(string.Join(",",r.Errors.Select(e=>e.Description)));return u;}
    public async Task<Team> Team(IdentityUser u,string name){var pokemon=Db.Pokemons.First();var t=new Team{Name=name,UserId=u.Id};t.Slots.Add(new TeamSlot{PokemonId=pokemon.Id,Pokemon=pokemon,SlotNumber=1});Db.Teams.Add(t);await Db.SaveChangesAsync();return t;}
    public DefaultHttpContext Http(IdentityUser u)=>new(){User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,u.Id)},"test"))};
    public TeamsController Controller(IdentityUser user,ApiHandler? handler=null){var http=Http(user);return new TeamsController(Db,Manager,new PokeApiClient(new HttpClient(handler??new ApiHandler()){BaseAddress=new Uri("https://fake.invalid/")})){ControllerContext=new ControllerContext{HttpContext=http},TempData=new TempDataDictionary(http,new MemoryTemp())};}
    public void Dispose(){_provider.Dispose();Connection.Dispose();}
}
class ApiHandler(bool fail=false):HttpMessageHandler{protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){if(fail)throw new HttpRequestException("offline fixture");return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent("{\"id\":99999,\"name\":\"audit-mon\",\"types\":[{\"slot\":1,\"type\":{\"name\":\"normal\"}}]}")});}}
class MemoryTemp:ITempDataProvider{public IDictionary<string,object> LoadTempData(HttpContext c)=>new Dictionary<string,object>();public void SaveTempData(HttpContext c,IDictionary<string,object> v){}}
