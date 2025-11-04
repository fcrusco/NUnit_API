# Testes de Integração em API ASP.NET Core 8 com NUnit

Este repositório é um guia prático e um projeto de exemplo focado em demonstrar como implementar **testes de integração automatizados** para uma API ASP.NET Core (.NET 8) utilizando o framework **NUnit** e o Visual Studio 2022.

<img width="392" height="631" alt="image" src="https://github.com/user-attachments/assets/36a4a584-8d82-491f-8657-406ba12bf876" />

## 🎯 Objetivo Principal

O objetivo deste projeto não é construir uma API complexa, mas sim ilustrar o processo de testes de *endpoints*. O foco é mostrar como usar a classe `WebApplicationFactory` para hospedar a API em memória e usar um `HttpClient` para enviar requisições HTTP reais (GET, POST) e validar as respostas (Status Codes e JSON).

Embora o NUnit seja frequentemente associado a testes *unitários* (testar um método isoladamente), esta abordagem demonstra como usá-lo para testes de *integração* (testar a API de ponta a ponta, da rota ao repositório).

## 🛠️ Tecnologias Utilizadas

* **Plataforma:** .NET 8
* **IDE:** Visual Studio 2022
* **Projeto de API:** ASP.NET Core Web API (usando Controllers)
* **Framework de Teste:** NUnit (v4.x)
* **Biblioteca de Teste de API:** `Microsoft.AspNetCore.Mvc.Testing`

## 📂 Estrutura do Projeto

A solução é dividida em dois projetos principais para garantir uma clara **Separação de Preocupações (Separation of Concerns)**.

### 1. `MinhaApiSimples` (O Projeto da API)

Este é o projeto da API ASP.NET Core.

* `Controllers/PessoasController.cs`:
    * Contém os endpoints (rotas) da nossa API.
    * **`GET /api/pessoas`**: Retorna a lista de todas as pessoas.
    * **`POST /api/pessoas`**: Cria uma nova pessoa.
* `Models/Pessoa.cs`:
    * Define os modelos de dados (`record Pessoa`) e os DTOs (`record PessoaCreateRequest`).
* `Data/PessoaRepository.cs`:
    * **Simula um banco de dados.** É uma classe estática com uma `List<Pessoa>` para armazenar dados em memória.
    * **Importante:** Contém o método `Clear()` para ser usado pelos testes, garantindo que o "banco de dados" seja limpo antes de cada execução de teste.
* `Program.cs`:
    * Configura os serviços da API (como `AddControllers()`) e o pipeline de requisições (`MapControllers()`).
    * Contém a linha `public partial class Program { }` no final, que é **essencial** para que o `WebApplicationFactory` consiga encontrar e iniciar a API no projeto de testes.

### 2. `MinhaApiSimples.Tests` (O Projeto de Teste NUnit)

Este projeto contém todos os testes automatizados para a nossa API.

* `PessoaApiTests.cs`:
    * A classe de teste principal que herda `WebApplicationFactory<Program>`.
    * É responsável por enviar requisições HTTP reais (em memória) para os endpoints da API e verificar se as respostas estão corretas.

#### Pacotes NuGet Essenciais (no projeto de teste)

Para que os testes de integração funcionem, o projeto `MinhaApiSimples.Tests` precisa de:

* `nunit` e `NUnit3TestAdapter` (para o NUnit funcionar).
* `Microsoft.NET.Test.Sdk` (o SDK de teste padrão).
* **`Microsoft.AspNetCore.Mvc.Testing`**: O pacote-chave que nos fornece o `WebApplicationFactory`.

## 🔬 O Conceito: `WebApplicationFactory`

A `WebApplicationFactory<T>` é a ferramenta central para testes de integração no ASP.NET Core. O que ela faz?

1.  **Inicia sua API em Memória:** Ela "levanta" sua aplicação web (o projeto `MinhaApiSimples`) inteira dentro do processo de teste, sem precisar hospedar em um Kestrel ou IIS real.
2.  **Falsifica a Rede:** Ela configura um `TestServer` em memória.
3.  **Fornece um `HttpClient`:** Ela nos dá um `HttpClient` (`_factory.CreateClient()`) que, ao invés de enviar requisições pela rede, as as envia diretamente para o `TestServer` em memória.

Isso torna os testes **extremamente rápidos** e **confiáveis**, pois não dependem de rede, portas ou servidores externos.

## 🧪 Análise Detalhada dos Testes (`PessoaApiTests.cs`)

Usamos os atributos do NUnit para gerenciar o ciclo de vida dos nossos testes.

### `[SetUp]` e `[TearDown]`

O `[SetUp]` (executado antes de CADA teste) é crucial para garantir o **isolamento dos testes**:

    [SetUp]
    public void Setup()
    {
        // 1. Cria a API em memória
        _factory = new WebApplicationFactory<Program>();
        
        // 2. Cria o cliente HTTP para fazer requisições
        _client = _factory.CreateClient();
    
        // 3. LIMPA o "banco de dados" estático
        PessoaRepository.Clear(); 
    }

O `[TearDown]` (executado depois de CADA teste) apenas limpa os recursos:

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

### Os Casos de Teste (Padrão AAA)

Cada teste segue o padrão **Arrange-Act-Assert**:

#### Teste 1: `GetPessoas_QuandoRepositorioVazio...`

* **Objetivo:** Verificar se o endpoint `GET` retorna uma lista vazia quando o banco está limpo.
* **Arrange:** O `[SetUp]` já limpou o banco.
* **Act:** `var response = await _client.GetAsync("/api/pessoas");`
* **Assert:**
    * `Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);` (Verifica o Status Code 200)
    * `Assert.IsEmpty(pessoas);` (Verifica se o JSON da resposta é uma lista vazia)

#### Teste 2: `PostPessoas_ComDadosValidos...`

* **Objetivo:** Verificar se o endpoint `POST` cria um novo usuário corretamente.
* **Arrange:** `var novaPessoaRequest = new PessoaCreateRequest("Fábio", "Crusco");`
* **Act:** `var response = await _client.PostAsJsonAsync("/api/pessoas", novaPessoaRequest);`
* **Assert:**
    * `Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);` (Verifica o Status Code 201)
    * `Assert.AreEqual(1, pessoaCriada.Id);` (Verifica se o JSON de resposta tem o ID 1)
    * `Assert.AreEqual("/api/pessoas/1", response.Headers.Location?.OriginalString);` (Verifica se o Header HTTP `Location` está correto)

#### Teste 3: `GetPessoas_DepoisDeUmPost...`

* **Objetivo:** Verificar se o estado persiste (o `GET` retorna o que o `POST` criou).
* **Arrange:** Fazemos um `POST` primeiro para popular o banco:
    `await _client.PostAsJsonAsync("/api/pessoas", novaPessoaRequest);`
* **Act:** `var response = await _client.GetAsync("/api/pessoas");`
* **Assert:**
    * `response.EnsureSuccessStatusCode();` (Garante um status 2xx)
    * `Assert.AreEqual(1, pessoas.Count);` (Verifica se a lista agora tem 1 item)
    * `Assert.AreEqual("Fábio", pessoas[0].Nome);` (Verifica se o item é o correto)

## 🚀 Como Executar

### 1. Executar a API (Manualmente)

1.  Abra a solução (`.sln`) no Visual Studio 2022.
2.  Defina `MinhaApiSimples` como projeto de inicialização (botão direito > "Set as Startup Project").
3.  Pressione `F5`.
4.  O Swagger UI abrirá no seu navegador, permitindo que você teste os endpoints `api/Pessoas` manualmente.

### 2. Executar os Testes Automatizados

1.  No menu superior do Visual Studio, vá em **Test** -> **Test Explorer**.
2.  O "Test Explorer" listará todos os testes encontrados em `MinhaApiSimples.Tests`.
3.  Clique no ícone **"Run All Tests In View"** (o botão de "play").
4.  Em segundos, todos os testes serão executados e ficarão verdes, confirmando que a API funciona como esperado.

<img width="962" height="324" alt="image" src="https://github.com/user-attachments/assets/7c563aeb-bfad-4885-90e4-abd10c487f3f" />
