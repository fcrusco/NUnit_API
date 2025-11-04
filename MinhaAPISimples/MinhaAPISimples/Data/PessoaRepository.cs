using MinhaAPISimples.Models;

namespace MinhaAPISimples.Data
{
    // Esta classe será nosso "banco de dados em memória"
    // O 'static' garante que a lista persista entre as requisições
    public static class PessoaRepository
    {
        private static readonly List<Pessoa> _pessoas = new();
        private static int _proximoId = 1;

        public static Pessoa AddPessoa(PessoaCreateRequest request)
        {
            // Cria a nova pessoa com um novo Id
            var pessoa = new Pessoa(_proximoId++, request.Nome, request.Sobrenome);
            _pessoas.Add(pessoa);
            return pessoa;
        }

        public static List<Pessoa> GetPessoas()
        {
            return _pessoas;
        }

        public static void Clear()
        {
            _pessoas.Clear();
            _proximoId = 1;
        }
    }
}
