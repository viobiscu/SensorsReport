using FluentMigrator;

namespace SensorsReport.Frontend.Migrations.DefaultDB;
[DefaultDB, MigrationKey(20250903_0200)]
public class DefaultDB_20250903_0200_SensorHistory : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("SensorHistory")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("Tenant").AsString(64).Nullable().Indexed()
            .WithColumn("SensorId").AsString(64).Nullable().Indexed()
            .WithColumn("PropertyKey").AsString(128).Nullable().Indexed()
            .WithColumn("MetadataKey").AsString(128).Nullable().Indexed()
            .WithColumn("ObservedAt").AsDateTimeOffset().Nullable().Indexed()
            .WithColumn("Value").AsDouble().Nullable()
            .WithColumn("Unit").AsString(32).Nullable()
            .WithColumn("CreatedAt").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.Index("IX_SensorHistory_Tenant_SensorId_PropertyKey_MetadataKey_ObservedAt")
            .OnTable("SensorHistory")
            .OnColumn("Tenant").Ascending()
            .OnColumn("SensorId").Ascending()
            .OnColumn("PropertyKey").Ascending()
            .OnColumn("MetadataKey").Ascending()
            .OnColumn("ObservedAt").Descending();

    }
}