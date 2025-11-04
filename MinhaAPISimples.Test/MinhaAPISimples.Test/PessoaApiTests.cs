// Importa a classe principal (WebApplicationFactory) para testes de integração de API.
using Microsoft.AspNetCore.Mvc.Testing;

// Importa a classe do repositório (para podermos limpá-la no Setup).
using MinhaAPISimples.Data;

// Importa os modelos de dados (Pessoa, PessoaCreateRequest) que a API usa.
using MinhaAPISimples.Models;

// Importa a enumeração de códigos de status HTTP (ex: HttpStatusCode.OK).
using System.Net;

// Importa métodos de extensão para facilitar o envio e recebimento de JSON via HTTP.
using System.Net.Http.Json;

// Define o "pacote" lógico onde esta classe de teste reside.
namespace MinhaApiSimples.Tests
{
    // Atributo NUnit que marca esta classe como um conjunto de testes.
    [TestFixture]
    // Declaração da classe de teste para a API de Pessoas.
    public class PessoaApiTests
    {
        // Declara um campo para nossa API em memória.
        private WebApplicationFactory<Program> _factory;

        // Declara um campo para o cliente HTTP que fará as requisições à API.
        private HttpClient _client;

        // Atributo NUnit que marca este método para ser executado ANTES de cada teste.
        [SetUp]
        // Método de configuração (Setup).
        public void Setup()
        {
            // Cria uma nova instância da fábrica da API, usando a classe 'Program' como ponto de entrada.
            _factory = new WebApplicationFactory<Program>();

            // Cria um cliente HTTP que já "sabe" como se comunicar com a API em memória.
            _client = _factory.CreateClient();

            // Limpa o repositório estático para garantir que um teste não interfira no outro.
            PessoaRepository.Clear();
        }

        // Atributo NUnit que marca este método para ser executado DEPOIS de cada teste.
        [TearDown]
        // Método de limpeza (TearDown).
        public void TearDown()
        {
            // Libera os recursos (como conexões) usados pelo cliente HTTP.
            _client.Dispose();

            // Desliga o servidor de API em memória e libera seus recursos.
            _factory.Dispose();
        }

        // Atributo NUnit que marca este método como um caso de teste individual.
        [Test]
        // Definição do teste (async Task é necessário por causa do 'await' nas chamadas HTTP).
        public async Task GetPessoas_QuandoRepositorioVazio_DeveRetornarOkEListaVazia()
        {
            // (Comentário do padrão Arrange-Act-Assert) A organização (Arrange) já foi feita no [SetUp].
            // Arrange (O Setup já limpou o repositório)

            // (Comentário do padrão Arrange-Act-Assert) Início da Ação (Act).
            // Act
            // Executa uma requisição HTTP GET para o endpoint "/api/pessoas" e aguarda a resposta.
            var response = await _client.GetAsync("/api/pessoas");

            // (Comentário do padrão Arrange-Act-Assert) Início da Verificação (Assert).
            // Assert 
            // Verifica se o status code da resposta foi 200 (OK).
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Lê o corpo (conteúdo) da resposta e o converte de JSON para uma Lista de Pessoas.
            var pessoas = await response.Content.ReadFromJsonAsync<List<Pessoa>>();

            // Verifica se a lista 'pessoas' não é nula (o JSON foi deserializado com sucesso).
            Assert.IsNotNull(pessoas);

            // Verifica se a lista está vazia (pois o repositório estava limpo).
            Assert.IsEmpty(pessoas, "A lista de pessoas deveria estar vazia.");
        }

        // Atributo NUnit que marca este método como o segundo caso de teste.
        [Test]
        // Definição do segundo teste (cenário de POST).
        public async Task PostPessoas_ComDadosValidos_DeveRetornarCreatedEConterPessoa()
        {
            // (Padrão AAA) Início da Organização (Arrange).
            // Arrange
            // Cria o objeto (DTO) que será enviado no corpo (body) da requisição POST.
            var novaPessoaRequest = new PessoaCreateRequest("Fábio", "Crusco");

            // (Padrão AAA) Início da Ação (Act).
            // Act
            // Serializa o objeto 'novaPessoaRequest' para JSON e o envia via POST para "/api/pessoas".
            var response = await _client.PostAsJsonAsync("/api/pessoas", novaPessoaRequest);

            // (Padrão AAA) Início da Verificação (Assert).
            // Assert (Verifica o POST)
            // Verifica se o status code da resposta foi 201 (Created), o correto para criação.
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            // Lê o corpo da resposta, que deve conter a pessoa recém-criada (com o Id).
            var pessoaCriada = await response.Content.ReadFromJsonAsync<Pessoa>();

            // Verifica se o objeto 'pessoaCriada' não é nulo.
            Assert.IsNotNull(pessoaCriada);

            // Verifica se o Id da pessoa criada é 1 (pois o repositório foi limpo no Setup).
            Assert.AreEqual(1, pessoaCriada.Id);

            // Verifica se o nome está correto.
            Assert.AreEqual("Fábio", pessoaCriada.Nome);

            // Verifica se o sobrenome está correto.
            Assert.AreEqual("Crusco", pessoaCriada.Sobrenome);

            // Verifica se o cabeçalho 'Location' na resposta contém a URL para o novo recurso.
            Assert.AreEqual("/api/pessoas/1", response.Headers.Location?.OriginalString);
        }

        // Atributo NUnit que marca este método como o terceiro caso de teste.
        [Test]
        // Definição do terceiro teste (um cenário combinado de POST seguido de GET).
        public async Task GetPessoas_DepoisDeUmPost_DeveRetornarListaComUmaPessoa()
        {
            // (Padrão AAA) Início da Organização (Arrange).
            // Arrange
            // Cria o DTO para ser usado no POST de preparação.
            var novaPessoaRequest = new PessoaCreateRequest("Fábio", "Crusco");

            // Executa um POST para "preparar" o estado do banco de dados (colocar um item lá).
            await _client.PostAsJsonAsync("/api/pessoas", novaPessoaRequest);

            // (Padrão AAA) Início da Ação (Act).
            // Act
            // Executa a ação principal do teste: um GET para buscar os dados.
            var response = await _client.GetAsync("/api/pessoas");

            // (Padrão AAA) Início da Verificação (Assert).
            // Assert 
            // Garante que a resposta foi bem-sucedida (status 200-299). Se não for, o teste falha aqui.
            response.EnsureSuccessStatusCode();

            // Lê o corpo da resposta e o converte de JSON para uma Lista de Pessoas.
            var pessoas = await response.Content.ReadFromJsonAsync<List<Pessoa>>();

            // Verifica se a lista não é nula.
            Assert.IsNotNull(pessoas);

            // Verifica se a lista agora contém exatamente 1 item (o que foi adicionado no Arrange).
            Assert.AreEqual(1, pessoas.Count);

            // Verifica se o nome do item na lista está correto.
            Assert.AreEqual("Fábio", pessoas[0].Nome);
        }
    } 
} 