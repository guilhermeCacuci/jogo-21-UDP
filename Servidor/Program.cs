using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;

/// <summary>
/// Servidor do jogo 21. Gerencia salas e partidas, usando UDP.
/// </summary>
class Servidor
{
    static UdpClient servidorUdp = new UdpClient(5000);
    static ConcurrentDictionary<string, Sala> salas = new();

    static void Main()
    {
        Console.WriteLine("Servidor iniciado na porta 5000...");

        while (true)
        {
            IPEndPoint origem = new IPEndPoint(IPAddress.Any, 0);
            byte[] dadosRecebidos = servidorUdp.Receive(ref origem);
            string mensagem = Encoding.UTF8.GetString(dadosRecebidos);

            Console.WriteLine($"Recebido de {origem}: {mensagem}");

            ProcessarMensagem(mensagem, origem);
        }
    }

    static void ProcessarMensagem(string mensagem, IPEndPoint origem)
    {
        if (mensagem.StartsWith("ENTRAR:"))
        {
            var partes = mensagem.Split(':');
            string nomeJogador = partes[1];
            string nomeSala = partes.Length > 2 ? partes[2] : "geral";

            var sala = salas.GetOrAdd(nomeSala, _ => new Sala { Nome = nomeSala });
            var jogador = new Jogador(nomeJogador) { Endereco = origem };
            sala.Jogadores[origem] = jogador;

            EnviarParaCliente($"BEM_VINDO:{nomeJogador}", origem);
        }
        else if (mensagem == "PEDIR_CARTA" || mensagem == "PARAR")
        {
            foreach (var sala in salas.Values)
            {
                if (sala.Jogadores.TryGetValue(origem, out var jogador))
                {
                    if (sala.Jogadores.Count < 2)
                    {
                        EnviarParaCliente("INFO:Jogadores insuficientes. Esperando mais jogadores...", origem);
                        return;
                    }

                    if (jogador.Parou || jogador.Pontuacao > 21)
                    {
                        EnviarParaCliente("INFO:Você já terminou sua rodada. Aguarde os outros jogadores.", origem);
                        return;
                    }

                    if (mensagem == "PEDIR_CARTA")
                    {
                        int novaCarta = new Random().Next(1, 12);
                        jogador.Pontuacao += novaCarta;
                        jogador.Cartas.Add(novaCarta);

                        EnviarParaCliente($"CARTA:{novaCarta}:{jogador.Pontuacao}", origem);

                        if (jogador.Pontuacao > 21)
                        {
                            EnviarParaCliente($"FIM:PERDEU:{jogador.Pontuacao}", origem);
                            jogador.Parou = true;
                        }
                        else if (jogador.Pontuacao == 21)
                        {
                            EnviarParaCliente($"FIM:GANHOU:{jogador.Pontuacao}", origem);
                            jogador.Parou = true;
                        }
                    }
                    else if (mensagem == "PARAR")
                    {
                        EnviarParaCliente($"FIM:PAROU:{jogador.Pontuacao}", origem);
                        jogador.Parou = true;
                    }

                    sala.VerificarSePartidaTerminou();
                    break;
                }
            }
        }
    }

    public static void EnviarParaCliente(string mensagem, IPEndPoint destino)
    {
        byte[] dados = Encoding.UTF8.GetBytes(mensagem);
        servidorUdp.Send(dados, dados.Length, destino);
    }

    class Jogador
    {
        public string Nome { get; }
        public List<int> Cartas { get; set; } = new();
        public int Pontuacao { get; set; } = 0;
        public bool Parou { get; set; } = false;
        public IPEndPoint Endereco { get; set; }

        public Jogador(string nome) => Nome = nome;
    }

    class Sala
    {
        public string Nome { get; set; }
        public ConcurrentDictionary<IPEndPoint, Jogador> Jogadores = new();

        public void VerificarSePartidaTerminou()
        {
            if (Jogadores.Count < 2) return;

            if (Jogadores.Values.All(j => j.Parou || j.Pontuacao > 21))
            {
                var vencedor = Jogadores.Values
                    .Where(j => j.Pontuacao <= 21)
                    .OrderByDescending(j => j.Pontuacao)
                    .FirstOrDefault();

                string msg = vencedor != null
                    ? $"FIM_PARTIDA:GANHADOR:{vencedor.Nome}:{vencedor.Pontuacao}"
                    : "FIM_PARTIDA:NINGUEM";

                foreach (var entry in Jogadores)
                {
                    Servidor.EnviarParaCliente(msg, entry.Key);
                }

                Jogadores.Clear(); // reinicia a sala após a partida
            }
        }
    }
}
