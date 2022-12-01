using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class PokemonController : ControllerBase
{
    private readonly IPokemonRepository pokemonRepository;
    private readonly IMapper mapper;

    public PokemonController(IPokemonRepository pokemonRepository, IMapper mapper)
    {
        this.pokemonRepository = pokemonRepository;
        this.mapper = mapper;
    }

    [HttpGet("allPokemons")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
    public IActionResult GetPokemons()
    {
        var pokemons = mapper.Map<List<PokemonDto>>(pokemonRepository.GetPokemons());

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return Ok(pokemons);
    }

    [HttpGet("{pokeId}")]
    [ProducesResponseType(200, Type = typeof(Pokemon))]
    [ProducesResponseType(400)]
    public IActionResult GetPokemon(int pokeId)
    {
        if (!pokemonRepository.PokemonExists(pokeId))
            return NotFound();

        var pokemon = mapper.Map<PokemonDto>(pokemonRepository.GetPokemon(pokeId));

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return Ok(pokemon);
    }

    [HttpGet("{pokeId}/rating")]
    [ProducesResponseType(200, Type = typeof(decimal))]
    [ProducesResponseType(400)]
    public IActionResult GetPokemonRating(int pokeId)
    {
        if (!pokemonRepository.PokemonExists(pokeId))
            return NotFound();

        var rating = pokemonRepository.GetPokemonRating(pokeId);

        if (!ModelState.IsValid)
            return BadRequest();

        return Ok(rating);
    }
}