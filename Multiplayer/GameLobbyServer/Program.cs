var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

List<LobbyServer> lobbies = new List<LobbyServer>();

app.MapPost("CreateLobby", (string lobbyId, string hostId, string hostName) =>
{
    var hostPlayer = new Player
    {
        Id = 0,  
        Name = hostName
    };
    
    var newLobby = new LobbyServer
    {
        LobbyId = lobbyId,
        HostId = hostId,
        Players = new List<Player> { hostPlayer }
    };

    lobbies.Add(newLobby);
    return Results.Ok(newLobby);
});


app.MapPost("JoinLobby", (string lobbyId, int playerId, string playerName) =>
{
    var lobby = lobbies.FirstOrDefault(lobby => lobby.LobbyId == lobbyId);
    if (lobby == null)
        return Results.NotFound("Lobby not found");
    
    if (lobby.Players.Any(player => player.Id == playerId))
        return Results.BadRequest("Player already in the lobby");
    
    var player = new Player
    {
        Id = playerId,
        Name = playerName
    };

    lobby.Players.Add(player);
    return Results.Ok("Player added to the lobby");
});


app.MapGet("Lobbies", () =>
{
    return Results.Ok(lobbies);
});

app.MapPost("RemovePlayer", (string lobbyId, long playerId) =>
{
    var lobby = lobbies.FirstOrDefault(lobby => lobby.LobbyId == lobbyId);
    if (lobby == null)
        return Results.NotFound("Lobby not found");

    var player = lobby.Players.FirstOrDefault(player => player.Id == playerId);
    if (player == null)
        return Results.NotFound("Player not found in the lobby");

    lobby.Players.Remove(player);
    return Results.Ok($"Player {playerId} removed from lobby {lobbyId}");
});

app.MapDelete("CloseLobby", (string lobbyId) =>
{
    var lobby = lobbies.FirstOrDefault(lobby => lobby.LobbyId == lobbyId);
    if (lobby == null)
        return Results.NotFound("Lobby not found");

    lobbies.Remove(lobby);
    return Results.Ok($"Lobby {lobbyId} closed and removed");
});
  
app.Run();

public class Player
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class LobbyServer
{
    public string? LobbyId { get; set; }
    public string? HostId { get; set; }
    public List<Player> Players { get; set; } = new List<Player>();
}

