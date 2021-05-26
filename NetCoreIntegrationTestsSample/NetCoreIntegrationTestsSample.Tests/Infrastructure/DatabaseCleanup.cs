using Npgsql;

namespace NetCoreIntegrationTestsSample.Tests.Infrastructure
{
    public class DatabaseCleanup
    {
        public static void ClearDatabase(string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var cmdText = @"DO $$
                                BEGIN
                                  DECLARE tables_cur CURSOR FOR
                                    SELECT tablename, schemaname FROM pg_catalog.pg_tables WHERE schemaname = 'public';
                                  BEGIN
                                    FOR tbl IN tables_cur LOOP
                                      EXECUTE 'TRUNCATE TABLE ' || quote_ident(tbl.schemaname) || '.' || quote_ident(tbl.tablename) || ' CASCADE;';
                                    END LOOP;
                                  END;
                                END;
                                $$;";
                
                new NpgsqlCommand(cmdText, connection).ExecuteNonQuery();
            }
        }
    }
}