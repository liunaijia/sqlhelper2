namespace Test {
    public class DatabaseFactory {
        public static IDatabase CreateDatabase(string connectionStringName = "*") {
            return new Database(connectionStringName);
        }
    }
}
