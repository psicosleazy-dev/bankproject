using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

// brumbank

public abstract class Conta
{
    public int NumeroConta { get; protected set; }
    public string? Titular { get; protected set; }
    public double Saldo { get; protected set; }

    protected Conta(int numero, string titular, double saldo)
    {
        NumeroConta = numero;
        Titular = titular;
        Saldo = saldo;
    }

    public void Sacar(double valor)
    {
        if (Saldo >= valor)
        {
            Saldo -= valor;
            Console.WriteLine($"Saque de R${valor} realizado com sucesso. Saldo atual: R${Saldo}");
        }
        else
        {
            throw new SaldoInsuficienteException("Saldo insuficiente para realizar o saque.");
        }
    }

    public void Depositar(double valor)
    {
        if (valor <= 0) throw new ArgumentException("Valor de depósito deve ser maior que zero.");
        Saldo += valor;
        Console.WriteLine($"Depósito de R${valor} realizado com sucesso. Saldo atual: R${Saldo}");
    }

    public abstract void ExibirExtrato();
}

public class ContaCorrente : Conta, ITributavel
{
    public double LimiteChequeEspecial { get; set; }

    public ContaCorrente(int numero, string titular, double saldo) : base(numero, titular, saldo) { }

    public double CalcularImposto()
    {
        return Saldo * 0.01; // Exemplo de imposto de 1% sobre o saldo
    }

    public override void ExibirExtrato()
    {
        Console.WriteLine($"[CC] Titular: {Titular} | Saldo: R$ {Saldo:F2} | Imposto: R$ {CalcularImposto():F2}");
    }
}

public class ContaPoupanca : Conta
{
    public ContaPoupanca(int numero, string titular, double saldo) : base(numero, titular, saldo) { }

    public void CalcularRendimento()
    {
        double rendimento = Saldo * 0.05; // Exemplo de rendimento de 5%
        Saldo += rendimento;
        Console.WriteLine($"Rendimento de R${rendimento} adicionado. Saldo atual: R${Saldo}");
    }

    public override void ExibirExtrato()
    {
        Console.WriteLine($"[CP] Titular: {Titular} | Saldo: R$ {Saldo:F2}");
    }
}

[Serializable]
internal class SaldoInsuficienteException : Exception
{
    public SaldoInsuficienteException()
    {
    }

    public SaldoInsuficienteException(string? message) : base(message)
    {
    }

    public SaldoInsuficienteException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

interface ITributavel
{
    double CalcularImposto();

    public class SeguroDeVida : ITributavel
    {
        public double ValorPremio { get; set; }
        public double CalcularImposto()
        {
            return 42.00; // Exemplo de imposto fixo para seguro de vida
        }
    }
}
/*
public class GerenciadorDeImposto : ITributavel
{
    public double TotalImposto { get; private set; }

    public void Registrar(ITributavel tributavel)
    {
        double imposto = tributavel.CalcularImposto();
        TotalImposto += imposto;
        Console.WriteLine($"Imposto registrado: R${imposto:F2}. Total acumulado: R${TotalImposto:F2}");
    }
}*/

public class BancoManager
{
    public static List<Conta> contas = new List<Conta>();

    public static void AdicionarConta(Conta conta)
    {
        contas.Add(conta);
        Console.WriteLine("Conta adicionada com sucesso.");
    }

    public static void buscarConta(int numeroConta)
    {
        var conta = contas.FirstOrDefault(c => c.NumeroConta == numeroConta);
        if (conta != null)
        {
            Console.WriteLine($"Conta encontrada: {conta.Titular} - Saldo: R${conta.Saldo:F2}");
        }
        else
        {
            Console.WriteLine("Conta não encontrada.");
        }
    }

    public static void GerarRelatorioGeral()
    {
        Console.WriteLine("Relatório Geral de Contas:");
        foreach (var conta in contas)
        {
            Console.WriteLine($"Número: {conta.NumeroConta} | Titular: {conta.Titular} | Saldo: R${conta.Saldo:F2}");
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        int opcao;
        do
        {
            Console.WriteLine("Menu:");
            Console.WriteLine("1. Criar Conta (Corrente ou Poupança)");
            Console.WriteLine("2. Realizar Depósito/Saque");
            Console.WriteLine("3. Consultar Extrato");
            Console.WriteLine("0. Sair");
            Console.Write("Escolha uma opção: ");
            opcao = int.Parse(Console.ReadLine());

            switch (opcao)
            {
                case 1:
                    // Lógica para adicionar conta
                    Conta novaConta;
                    Console.Write("Digite o tipo de conta (1 para Corrente, 2 para Poupança): ");
                    int tipoConta = int.Parse(Console.ReadLine());
                    Console.Write("Digite o número da conta: ");
                    int numeroConta = int.Parse(Console.ReadLine());
                    Console.Write("Digite o nome do titular: ");
                    string titular = Console.ReadLine();
                    Console.Write("Digite o saldo inicial: ");
                    double saldoInicial = double.Parse(Console.ReadLine());
                    if (tipoConta == 1)
                    {
                        novaConta = new ContaCorrente(numeroConta, titular, saldoInicial);
                    }
                    else
                    {
                        novaConta = new ContaPoupanca(numeroConta, titular, saldoInicial);
                    }
                    BancoManager.AdicionarConta(novaConta);

                    break;
                case 2:
                    // Lógica para realizar depósito ou saque
                    Console.Write("Digite o número da conta: ");
                    int contaNumero = int.Parse(Console.ReadLine());
                    Console.Write("Digite o valor: ");
                    double valor = double.Parse(Console.ReadLine());
                    var conta = BancoManager.contas.FirstOrDefault(c => c.NumeroConta == contaNumero);
                    if (conta != null)
                    {
                        Console.Write("Digite 1 para Depósito ou 2 para Saque: ");
                        int tipoTransacao = int.Parse(Console.ReadLine());
                        if (tipoTransacao == 1)
                        {
                            conta.Depositar(valor);
                            File.AppendAllText("log_transacoes.txt", $"Depósito de R${valor} na conta {contaNumero} - {DateTime.Now}\n");
                        }
                        else
                        {
                            try
                            {
                                conta.Sacar(valor);
                                File.AppendAllText("log_transacoes.txt", $"Saque de R${valor} na conta {contaNumero} - {DateTime.Now}\n");
                            }
                            catch (SaldoInsuficienteException ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Conta não encontrada.");
                    }
                    break;
                case 3:
                    // Lógica para consultar extrato
                    Console.Write("Digite o número da conta: ");
                    int extratoNumero = int.Parse(Console.ReadLine());
                    var contaExtrato = BancoManager.contas.FirstOrDefault(c => c.NumeroConta == extratoNumero);
                    if (contaExtrato != null)
                    {
                        contaExtrato.ExibirExtrato();
                    }
                    else
                    {
                        Console.WriteLine("Conta não encontrada.");
                    }
                    break;
                case 0:
                    Console.WriteLine("Saindo...");
                    break;
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }
        } while (opcao != 0);
    }
}
