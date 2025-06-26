using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// Cliente do jogo "21", que envia ações e recebe atualizações do servidor via UDP.
/// </summary>
class Cliente
{
    static UdpClient clienteUdp = new UdpClient(); // Cliente UDP para comunicação
    static IPEndPoint servidorEndereco = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
    static string nomeJogador = "";
    static string nomeSala = "";
    static bool esperandoResultadoFinal = false;

    static void Main()
    {
        Console.WriteLine("=== Jogo 21 - Cliente ===");

        Console.Write("Digite seu nome: ");
        nomeJogador = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Digite o nome da sala: ");
        nomeSala = Console.ReadLine()?.Trim() ?? "geral";

        if (string.IsNullOrWhiteSpace(nomeJogador))
        {
            Console.WriteLine("Nome inválido. Encerrando...");
            return;
        }

        // Solicita entrada na sala
        EnviarParaServidor($"ENTRAR:{nomeJogador}:{nomeSala}");
        ReceberMensagensServidor();

        while (true)
        {
            if (esperandoResultadoFinal)
            {
                ReceberMensagensServidor(); // Aguarda FIM_PARTIDA
                Thread.Sleep(500); // Evita uso alto de CPU
                continue;
            }

            Console.Write("\nEscolha uma ação - (C)arta ou (P)arar: ");
            string escolha = Console.ReadLine()?.Trim().ToLower() ?? "";

            if (escolha == "c")
            {
                EnviarParaServidor("PEDIR_CARTA");
            }
            else if (escolha == "p")
            {
                EnviarParaServidor("PARAR");
            }
            else
            {
                Console.WriteLine("Opção inválida.");
                continue;
            }

            ReceberMensagensServidor();
        }
    }

    static void EnviarParaServidor(string mensagem)
    {
        byte[] dados = Encoding.UTF8.GetBytes(mensagem);
        clienteUdp.Send(dados, dados.Length, servidorEndereco);
    }

    static void ReceberMensagensServidor()
    {
        do
        {
            IPEndPoint origem = new IPEndPoint(IPAddress.Any, 0);
            byte[] dadosRecebidos = clienteUdp.Receive(ref origem);
            string mensagem = Encoding.UTF8.GetString(dadosRecebidos);

            if (mensagem.StartsWith("BEM_VINDO:"))
            {
                Console.WriteLine($"\n>> Bem-vindo, {mensagem.Split(':')[1]}!");
            }
            else if (mensagem.StartsWith("CARTA:"))
            {
                var partes = mensagem.Split(':');
                Console.WriteLine($"\n>> Você recebeu a carta: {partes[1]}");
                Console.WriteLine($">> Sua pontuação atual: {partes[2]}");
            }
            else if (mensagem.StartsWith("FIM:"))
            {
                var partes = mensagem.Split(':');
                string tipo = partes[1];
                int pontos = int.Parse(partes[2]);

                string resultado = tipo switch
                {
                    "GANHOU" => $" Você ganhou com {pontos} pontos!",
                    "PERDEU" => $" Você perdeu com {pontos} pontos!",
                    "PAROU" => $" Você parou com {pontos} pontos.",
                    _ => $"Resultado desconhecido: {mensagem}"
                };

                Console.WriteLine($"\n>> {resultado}");
                esperandoResultadoFinal = true;
            }
            else if (mensagem.StartsWith("FIM_PARTIDA:"))
            {
                var partes = mensagem.Split(':');
                if (partes[1] == "GANHADOR")
                {
                    Console.WriteLine($"\n Vencedor da sala: {partes[2]} com {partes[3]} pontos!");
                }
                else
                {
                    Console.WriteLine("\n Ninguém venceu a partida.");
                }

                Console.WriteLine("\nObrigado por jogar!");
                Environment.Exit(0);
            }
            else if (mensagem.StartsWith("INFO:"))
            {
                Console.WriteLine($"\n>> {mensagem.Substring(5)}");
            }
            else
            {
                Console.WriteLine($">> Mensagem desconhecida: {mensagem}");
            }

        } while (clienteUdp.Available > 0);
    }
}
