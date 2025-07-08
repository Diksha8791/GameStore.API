using GameStore.API.Data;
using GameStore.API.DTOs;
using GameStore.API.Entities;
using GameStore.API.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GameStore.API.EndPoints;

public static class GamesEndPoints
{
    const string GetGameEndPointName = "GetGame";

    public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app)
    {

        var group = app.MapGroup("games")
                        .WithParameterValidation();
        //GET /games
        //minimal API
        group.MapGet("/", async (GameStoreContext dbContext ) =>
                await dbContext.Games
                .Include(game => game.Genre)
                .Select(game => game.ToGameSummaryDTO())
                .AsNoTracking()
                .ToListAsync());

        //GET /games/id
        group.MapGet("/{id:int}", async (int id, GameStoreContext dbContext) =>
        {
            Game? game = await dbContext.Games.FindAsync(id);
            return game is null ? Results.NotFound() : Results.Ok(game.ToGameDetailsDTO());

        })
        .WithName(GetGameEndPointName);

        //POST /games
        group.MapPost("/", async (CreateGameDTO newGame, GameStoreContext dbContext) =>
        {
            Game game = newGame.ToEntity();

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();

            //return 201 created response // also newly created data location header
            return Results.CreatedAtRoute(GetGameEndPointName, new { id = game.Id }, game.ToGameDetailsDTO());
        })
        .WithParameterValidation();

        //Put replaces object completely
        //PUT /games
        group.MapPut("/{id}", async (int id, UpdateGameDTO updateGame, GameStoreContext dbContext) =>
        {
            var existingGame = await dbContext.Games.FindAsync(id);
            if (existingGame is null)
            {
                return Results.NotFound();
            }

            dbContext.Entry(existingGame)
                     .CurrentValues
                     .SetValues(updateGame.ToEntity(id));

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });


        //DELETE /game/id
        group.MapDelete("/{id}", async (int id,  GameStoreContext dbContext) =>
        {
            await dbContext.Games
            .Where(game => game.Id == id)
            .ExecuteDeleteAsync();

            return Results.NoContent();
        });

        return group;
    }
}
