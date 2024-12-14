namespace QuickApi.Example.Data.Configs;

public class PostgresOptions
{
    public const string Postgres = "Postgres";
    public string ConnectionString { get; set; } = null!;
}