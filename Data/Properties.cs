using System.Threading.Tasks;
using static iSketch.app.Classes.Database;

namespace iSketch.app.Data
{
    public static class Properties
    {
        public static async Task<string> GetProperty(string Property)
        {
            return await ExecuteScalar<string>(
                CommandText: @"
                    SELECT Value 
                    FROM [System.Properties] 
                    WHERE Property = @PROP@
                ",
                Parameters: [
                    new("@PROP@", Property)
                ]
            );
        }
        public static async Task SetProperty(string Property, string Value)
        {
            await ExecuteNonQuery(
                Command: async (cmd) => {
                    if (Value == null)
                    {
                        cmd.CommandText = "DELETE FROM [System.Properties] WHERE Property = @PROP@";
                    }
                    else
                    {
                        if (await GetProperty(Property) == null)
                        {
                            cmd.CommandText = "INSERT INTO [System.Properties] (Property, Value) VALUES(@PROP@, @VAL@)";
                        }
                        else
                        {
                            cmd.CommandText = "UPDATE [System.Properties] SET Value = @VAL@ WHERE Property = @PROP@";
                        }
                    }
                },
                Parameters: [
                    new("@PROP@", Property),
                    new("@VAL@", Value)
                ]
            );
        }
    }
}
