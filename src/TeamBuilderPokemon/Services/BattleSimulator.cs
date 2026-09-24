using TeamBuilderPokemon.Models;

namespace TeamBuilderPokemon.Services;

/// <summary>Um lutador da simulacao: o Pokemon, o nivel e o HP que sobrou.</summary>
public class Fighter
{
    public Fighter(string name, IReadOnlyList<string> types, int level)
    {
        Name = name;
        Types = types;
        Level = level;
        MaxHp = BattleSimulator.HpForLevel(level);
        Hp = MaxHp;
    }

    public string Name { get; }
    public IReadOnlyList<string> Types { get; }
    public int Level { get; }
    public int MaxHp { get; }
    public int Hp { get; set; }
    public bool Fainted => Hp <= 0;
}

public class BattleReport
{
    /// <summary>"A", "B" ou "Empate" (quando bate o limite de turnos).</summary>
    public string Winner { get; set; } = string.Empty;

    public List<string> Log { get; set; } = new();

    public int Turns { get; set; }
}

/// <summary>
/// Batalha simplificada entre dois times, com a mesma formula de dano do Simulador em Python.
/// O catalogo so tem tipos (sem stats), entao todo mundo tem stats neutros e ataca com um golpe
/// de poder fixo do proprio tipo: quem vence e' decidido por tipo e nivel.
/// </summary>
public static class BattleSimulator
{
    public const int MovePower = 70;
    public const int NeutralStat = 100;
    public const int BaseHp = 80;
    public const int MaxTurns = 400;

    /// <summary>Formula oficial de HP com base 80 e sem IV/EV.</summary>
    public static int HpForLevel(int level) => (2 * BaseHp * level / 100) + level + 10;

    public static List<Fighter> FromSlots(IEnumerable<TeamSlot> slots)
    {
        return slots
            .Where(s => s.Pokemon is not null)
            .OrderBy(s => s.SlotNumber)
            .Select(s => new Fighter(s.DisplayName, s.Pokemon!.GetTypes(), s.Level))
            .ToList();
    }

    /// <summary>Melhor tipo do atacante contra o defensor (maior multiplicador).</summary>
    public static (string Type, double Effectiveness) BestAttack(Fighter attacker, Fighter defender)
    {
        return attacker.Types
            .Select(type => (Type: type, Effectiveness: TypeChart.Effectiveness(type, defender.Types)))
            .OrderByDescending(a => a.Effectiveness)
            .First();
    }

    /// <summary>Dano de um golpe: nucleo da formula oficial x STAB x tipo x variacao (0.85 a 1.0).</summary>
    public static int Damage(Fighter attacker, Fighter defender, Random rng)
    {
        var (_, effectiveness) = BestAttack(attacker, defender);
        if (effectiveness == 0.0)
        {
            return 0;
        }

        var basePower = ((2.0 * attacker.Level / 5 + 2) * MovePower * NeutralStat / NeutralStat / 50) + 2;
        const double stab = 1.5; // o golpe e' sempre do tipo do proprio Pokemon
        var variance = 0.85 + rng.NextDouble() * 0.15;
        return Math.Max(1, (int)(basePower * stab * effectiveness * variance));
    }

    public static BattleReport Simulate(List<Fighter> teamA, List<Fighter> teamB, Random rng)
    {
        var report = new BattleReport();
        if (teamA.Count == 0 || teamB.Count == 0)
        {
            report.Winner = teamA.Count > 0 ? "A" : teamB.Count > 0 ? "B" : "Empate";
            return report;
        }

        var a = teamA.First();
        var b = teamB.First();
        report.Log.Add($"{a.Name} (Nv. {a.Level}) entra contra {b.Name} (Nv. {b.Level})!");

        while (report.Turns < MaxTurns)
        {
            report.Turns++;
            // Nivel maior ataca primeiro; empate decide no sorteio.
            var aFirst = a.Level > b.Level || (a.Level == b.Level && rng.Next(2) == 0);
            var order = aFirst ? new[] { (a, b), (b, a) } : new[] { (b, a), (a, b) };

            foreach (var (attacker, defender) in order)
            {
                if (attacker.Fainted || defender.Fainted)
                {
                    continue;
                }

                var (type, effectiveness) = BestAttack(attacker, defender);
                var damage = Damage(attacker, defender, rng);
                defender.Hp = Math.Max(0, defender.Hp - damage);
                report.Log.Add($"{attacker.Name} ataca com {type}: {damage} de dano{EffectivenessNote(effectiveness)} ({defender.Name}: {defender.Hp}/{defender.MaxHp} HP).");
                if (defender.Fainted)
                {
                    report.Log.Add($"{defender.Name} desmaiou!");
                }
            }

            if (a.Fainted)
            {
                a = teamA.FirstOrDefault(f => !f.Fainted)!;
                if (a is null)
                {
                    report.Winner = "B";
                    break;
                }
                report.Log.Add($"{a.Name} (Nv. {a.Level}) entra na batalha!");
            }

            if (b.Fainted)
            {
                b = teamB.FirstOrDefault(f => !f.Fainted)!;
                if (b is null)
                {
                    report.Winner = "A";
                    break;
                }
                report.Log.Add($"{b.Name} (Nv. {b.Level}) entra na batalha!");
            }
        }

        if (string.IsNullOrEmpty(report.Winner))
        {
            report.Winner = "Empate";
        }
        return report;
    }

    private static string EffectivenessNote(double effectiveness) => effectiveness switch
    {
        0.0 => ", nao afeta",
        > 1.0 => ", super efetivo",
        < 1.0 => ", pouco efetivo",
        _ => string.Empty,
    };
}
