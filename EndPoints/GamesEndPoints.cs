using GameStore.API.DTOs;

namespace GameStore.API.EndPoints;

public static class GamesEndPoints
{
    const string GetGameEndPointName = "GetGame";

    private static readonly List<GameDTO> games = [
        new(
            1,
            "Street Fighter II",
            "Fighting",
            19.99M,
            new DateOnly(1992, 7, 15)
        ),
        new(
            2,
            "Final Fantasy XIV",
            "Roleplaying",
            59.99M,
            new DateOnly(2010, 9, 30)
        ),
        new(
            3,
            "FIFA 23",
            "Sports",
            69.99M,
            new DateOnly(2022, 9, 27)
        )
    ];

    public static WebApplication MapGamesEndpoints(this WebApplication app)
    {

       // var group = app.MapGroup("games");
        //GET /games
        //minimal API
        app.MapGet("games", () => games);

        //GET /games/id
        app.MapGet("games/{id:int}", (int id) =>
        {
            GameDTO? game = games.Find(game => game.Id == id);
            return game is null ? Results.NotFound() : Results.Ok(game);

        })
        .WithName(GetGameEndPointName);

        //POST /games
        app.MapPost("games", (CreateGameDTO newGame) =>
        {
            GameDTO game = new(
                games.Count + 1,
                newGame.Name,
                newGame.Genre,
                newGame.Price,
                newGame.ReleaseDate
            );

            games.Add(game);

            //return 201 created response // also newly created data location header
            return Results.CreatedAtRoute(GetGameEndPointName, new { id = game.Id }, game);
        });

        //Put replaces object completely
        //PUT /games
        app.MapPut("games/{id}", (int id, UpdateGameDTO updateGame) =>
        {
            var index = games.FindIndex(game => game.Id == id);
            if (index == -1)
            {
                return Results.NotFound();
            }

            games[index] = new GameDTO(
                id,
                updateGame.Name,
                updateGame.Genre,
                updateGame.Price,
                updateGame.ReleaseDate
            );

            return Results.NoContent();
        });


        //DELETE /game/id
        app.MapDelete("games/{id}", (int id) =>
        {
            games.RemoveAll(game => game.Id == id);

            return Results.NoContent();
        });

        return app;
    }
}
