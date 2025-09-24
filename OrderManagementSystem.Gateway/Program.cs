namespace OrderManagementSystem.Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("YARP"));
            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");
            app.MapReverseProxy();

            app.Run();
        }
    }
}
