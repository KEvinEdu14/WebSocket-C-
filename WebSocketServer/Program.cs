using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Solo una llamada necesaria

var app = builder.Build();

// Habilitar middleware para servir Swagger como un endpoint JSON.
app.UseSwagger();
// Habilitar middleware para servir swagger-ui (HTML, JS, CSS, etc.),
// especificando el endpoint JSON de Swagger.
app.UseSwaggerUI();

app.MapGet("/", () => "Bienvenido a la API WebSocket").WithTags("Info");

// Ruta para iniciar WebSocket
app.UseWebSockets();
app.MapGet("/ws", async context =>
{
     Console.WriteLine("Solicitud WebSocket recibida");
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine("Conexión WebSocket aceptada");
        await HandleWebSocketCommunication(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
        Console.WriteLine("Solicitud no WebSocket, respondiendo con 400");
    }
}).WithTags("WebSocket").WithName("ConnectWebSocket");

async Task HandleWebSocketCommunication(WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];
    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

    while (result.MessageType != WebSocketMessageType.Close)
    {
        string messageReceived = Encoding.UTF8.GetString(buffer, 0, result.Count);
        Console.WriteLine($"Mensaje recibido: {messageReceived}");

        var messageToSend = $"Hola, recibí tu mensaje: {messageReceived}";
        var byteMessage = Encoding.UTF8.GetBytes(messageToSend);

        await webSocket.SendAsync(new ArraySegment<byte>(byteMessage), WebSocketMessageType.Text, true, CancellationToken.None);
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }

    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cerrando conexión", CancellationToken.None);
}

app.Run();
