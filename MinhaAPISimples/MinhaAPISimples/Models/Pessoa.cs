namespace MinhaAPISimples.Models
{
    // O 'record' é perfeito para modelos de dados imutáveis
    public record Pessoa(int Id, string Nome, string Sobrenome);

    // Este é um DTO (Data Transfer Object) para a requisição de criação
    // Note que não tem 'Id', pois o Id será gerado pelo servidor
    public record PessoaCreateRequest(string Nome, string Sobrenome);
}
