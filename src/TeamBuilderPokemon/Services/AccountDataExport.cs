using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamBuilderPokemon.Data;

namespace TeamBuilderPokemon.Services;

public static class AccountDataExport
{
    public static async Task<byte[]> BuildAsync(ApplicationDbContext db,IdentityUser user)
    {
        var teams=await db.Teams.Where(t => t.UserId != null && t.UserId == user.Id)
            .Include(t => t.Slots).ThenInclude(s => s.Pokemon).Include(t => t.Revisions).AsSplitQuery().ToListAsync();
        var personal=typeof(IdentityUser).GetProperties().Where(p => p.GetCustomAttribute<PersonalDataAttribute>() is not null)
            .ToDictionary(p => p.Name,p => p.GetValue(user));
        var data=new { schemaVersion=1, exportedAt=DateTime.UtcNow, personalData=personal,
            teams=teams.Select(t => new { t.Id,t.Name,t.IsPublic,t.CreatedAtUtc,t.DeletedAtUtc,
                portable=JsonSerializer.Deserialize<JsonElement>(TeamCodec.Export(t)),
                revisions=t.Revisions.OrderBy(r => r.CreatedAtUtc).Select(r => new { r.Id,r.CreatedAtUtc,r.SnapshotJson }) }) };
        return JsonSerializer.SerializeToUtf8Bytes(data,new JsonSerializerOptions {WriteIndented=true});
    }
}
