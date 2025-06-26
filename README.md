# Jogo 21 - Cliente e Servidor (C# via UDP)

Este projeto implementa o jogo "21" (também conhecido como Blackjack simplificado) utilizando comunicação via **UDP** em C#. O sistema é composto por dois programas:

- `Servidor`: gerencia múltiplas salas e partidas.
- `Cliente`: permite que o jogador entre em uma sala, peça cartas ou pare o jogo.

---

## Como funciona o jogo?

Cada jogador entra com um nome em uma sala. Os jogadores escolhem entre:

- **Pedir uma carta**: recebe um valor de 1 a 11, somado à sua pontuação.
- **Parar**: encerra sua vez.

Objetivo: **atingir 21 pontos** ou ficar o mais próximo possível sem ultrapassar.

O jogo termina quando todos os jogadores da sala pararem ou estourarem 21. O servidor informa o vencedor.

---

## Requisitos

- .NET SDK 6.0 ou superior
- Git instalado (para clonar o repositório)

---

## Compilação e Execução

### 1. Clone o repositório


git clone https://github.com/guilhermeCacuci/jogo-21-udp.git
cd jogo-21-udp


### 2. Compile o servidor


cd Servidor
dotnet build
dotnet run


### 3. Compile o cliente (em outro terminal)


cd ../Cliente
dotnet build
dotnet run


Você pode abrir múltiplos terminais para executar dois ou mais clientes simultaneamente.

---

## Estrutura do Projeto


jogo-21-udp/
├── Cliente/
│   └── Program.cs
├── Servidor/
│   └── Program.cs
└── README.md


---

##  Autores

* Guilherme Caçuci Ladislau
* Kaio César Vidigal
* Paulo Henrique Xavier
* Mateus Freitas

---

## Licença

Este projeto é apenas educacional. Uso livre com créditos aos autores.
