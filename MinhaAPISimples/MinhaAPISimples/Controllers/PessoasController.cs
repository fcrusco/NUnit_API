using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinhaAPISimples.Data;
using MinhaAPISimples.Models;

namespace MinhaAPISimples.Controllers
{
    // [ApiController] ativa recursos de API, como inferência de [FromBody]
    [ApiController]
    // [Route] define o padrão de URL. "api/[controller]" será "api/Pessoas"
    [Route("api/[controller]")]
    public class PessoasController : ControllerBase // Deve herdar de ControllerBase
    {
        // GET: api/Pessoas
        [HttpGet]
        public ActionResult<List<Pessoa>> GetPessoas()
        {
            var pessoas = PessoaRepository.GetPessoas();
            // Results.Ok() (Minimal API) -> Ok() (Controller)
            return Ok(pessoas);
        }


        // POST: api/Pessoas
        [HttpPost]
        public ActionResult<Pessoa> CreatePessoa([FromBody] PessoaCreateRequest request)
        {
            var pessoa = PessoaRepository.AddPessoa(request);

            // Results.Created() (Minimal API) -> CreatedAtAction() ou Created() (Controller)
            // Vamos usar Created() para replicar o comportamento anterior
            // e manter o teste de Location Header válido.
            var locationUrl = $"/api/pessoas/{pessoa.Id}";

            // Retorna 201 Created com a URL do novo recurso e o objeto criado
            return Created(locationUrl, pessoa);
        }

        // NOTA: Uma forma mais "correta" de fazer isso seria ter um endpoint GetById(int id)
        // e então usar: return CreatedAtAction(nameof(GetById), new { id = pessoa.Id }, pessoa);
        // Mas para manter os 2 endpoints usamos Created().
    }
}
